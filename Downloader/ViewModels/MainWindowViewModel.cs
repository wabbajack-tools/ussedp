using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Patcher.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SteamKit2;
using Wabbajack.Common;
using Wabbajack.DTOs;
using Wabbajack.DTOs.DownloadStates;
using Wabbajack.DTOs.JsonConverters;
using Wabbajack.Networking.Http.Interfaces;
using Wabbajack.Networking.Steam;
using Wabbajack.Paths;
using Wabbajack.Paths.IO;
using Wabbajack.RateLimiter;

namespace Patcher.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, IActivatableViewModel
    {
        public static Extension EXE = new Extension(".exe");
        public static Extension DLL = new Extension(".dll");
        
        private readonly Client _steamClient;
        private readonly ILogger<MainWindowViewModel> _logger;
        private readonly LoggerProvider _loggerProvider;
        private readonly ITokenProvider<SteamLoginState> _token;
        private readonly DTOSerializer _dtos;
        public ViewModelActivator Activator { get; }
        
        [Reactive]
        public AbsolutePath GamePath { get; set; }
        
        [Reactive]
        public ReactiveCommand<Unit, Unit> StartPatching { get; set; }

        [Reactive] public string[] LogLines { get; set; } = Array.Empty<string>();

        [Reactive] 
        public bool IsLoggedIn { get; set; }
        
        [Reactive]
        public string SteamUsername { get; set; }
        
        [Reactive]
        public string SteamPassword { get; set; }
        
        
        [Reactive]
        public ReactiveCommand<Unit, Task> LoginCommand { get; set; }
        
        [Reactive]
        public ReactiveCommand<Unit,Task> LogoutCommand { get; set; }

        public IEnumerable<GameViewModel> GameVersions => GameViewModel.GameVersions;
        
        [Reactive]
        public GameViewModel? SelectedVersion { get; set; }
        
        [Reactive]
        public bool BestOfBothWorlds { get; set; }

        private IJob? _currentJob;
        private IJob? _totalJob;
        private readonly Wabbajack.Networking.WabbajackClientApi.Client _wjClient;

        [Reactive]
        public Percent JobProgress { get; set; }
        
        [Reactive]
        public string JobText { get; set; }
        
        [Reactive]
        public Percent TotalProgress { get; set; }
        
        [Reactive]
        public string TotalText { get; set; }

        
        public void Log(string line)
        {
            LogLines = LogLines.Append(line).ToArray();
        }
        
        public MainWindowViewModel(ILogger<MainWindowViewModel> logger, Client steamClient, LoggerProvider loggerProvider, 
            ITokenProvider<SteamLoginState> token, DTOSerializer dtos, Wabbajack.Networking.WabbajackClientApi.Client wjClient)
        {
            _logger = logger;
            _token = token;
            _steamClient = steamClient;
            _loggerProvider = loggerProvider;
            _dtos = dtos;
            _wjClient = wjClient;
            Activator = new ViewModelActivator();

            _loggerProvider.Messages
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(l => Log(l.LongMessage));
            
            _logger.LogInformation("Started Downloader");

            SetSteamStatus().FireAndForget();

            this.WhenAnyValue(vm => vm.GamePath)
                .Subscribe(gp => InferVersion(gp));

            //var tsk = LocateAndSetGame(Game.SkyrimSpecialEdition);
            StartPatching = ReactiveCommand.CreateFromTask(async () =>
                {
                    try
                    {
                        await Task.Run(async () =>
                        {
                            await Start();
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "While running");
                    }
                },
                this.WhenAnyValue(vm => vm.GamePath)
                    .Select(gp => gp != default && gp.DirectoryExists())
                    .CombineLatest(this.WhenAnyValue(vm => vm.IsLoggedIn),
                        this.WhenAnyValue(vm => vm.SelectedVersion))
                    .Select(t => t.First && t.Second && t.Third != null));
            
            LoginCommand = ReactiveCommand.Create(async () =>
            {
                Login().FireAndForget();

            }, this.WhenAnyValue(vm => vm.IsLoggedIn)
                    .CombineLatest(this.WhenAnyValue(vm => vm.SteamUsername),
                        this.WhenAnyValue(vm => vm.SteamPassword))
                    .Select(t => !t.First && !string.IsNullOrWhiteSpace(t.Second) && !string.IsNullOrWhiteSpace(t.Third)));

            LogoutCommand = ReactiveCommand.Create(async () =>
            {
                await Logout();
            }, this.WhenAnyValue(vm => vm.IsLoggedIn));

            RxApp.MainThreadScheduler.SchedulePeriodic(0, TimeSpan.FromSeconds(0.25), _ =>
            {
                var job = _currentJob;
                if (job == null)
                {
                    JobProgress = Percent.Zero;
                    JobText = "";
                }
                else
                {
                    var percent = job.Size == 0 ? Percent.Zero : Percent.FactoryPutInRange((long)job.Current, (long)job.Size);
                    JobProgress = percent;
                    JobText = $"({percent}%) {job.Description}";
                }
                
                job = _totalJob;
                if (job == null)
                {
                    TotalProgress = Percent.Zero;
                    TotalText = "";
                }
                else
                {
                    var percent = job.Size == 0 ? Percent.Zero : Percent.FactoryPutInRange((long)job.Current, (long)job.Size);
                    TotalProgress = percent;
                    TotalText = $"({percent}%) {job.Description}";
                }

                
            });


        }

        private void InferVersion(AbsolutePath gp)
        {
            if (gp == default) return;
            foreach (var version in GameViewModel.GameVersions)
            {
                var postfix = string.Join("_", version.Version.Split(".").Take(3));
                if (gp.Combine("skse64_" + postfix + ".dll").FileExists())
                {
                    _logger.LogInformation("SKSE for {Version} found in install folder", version.Version);
                    SelectedVersion = version;
                    break;
                }
            }
        }


        private async Task Login()
        {
            await _token.SetToken(new SteamLoginState
            {
                User = SteamUsername,
                Password = SteamPassword
            });
            await _steamClient.Login();
            await SetSteamStatus();
        }

        private async Task Logout()
        {
            await _token.Delete();
            await SetSteamStatus();
        }


        private async Task SetSteamStatus()
        {
            if (!_token.HaveToken())
            {
                RxApp.MainThreadScheduler.Schedule(false, (_, _) =>
                {
                    IsLoggedIn = false;
                });
                return;
            }

            var token = await _token.Get();

            if (string.IsNullOrEmpty(token?.User) || string.IsNullOrEmpty(token?.Password))
            {
                RxApp.MainThreadScheduler.Schedule(false, (_, _) =>
                {
                    IsLoggedIn = false;
                });
                return;
            }

            SteamUsername = token!.User;

            if ((token?.SentryFile ?? Array.Empty<byte>()).Length == 0)
            {
                RxApp.MainThreadScheduler.Schedule(false, (_, _) =>
                {
                    IsLoggedIn = false;
                });
                return;
            }

            RxApp.MainThreadScheduler.Schedule(true, (_, _) =>
            {
                IsLoggedIn = true;
            });
        }

        private async Task Start()
        {
            _logger.LogInformation("Starting Patching");
            _logger.LogInformation("Building Install Plan");

            var outputPath = GamePath;

            var seAppId = (uint) Game.SkyrimSpecialEdition.MetaData().SteamIDs.First();

            var selectedVersion = BestOfBothWorlds ? GameViewModel.GameVersions.Last().Version : SelectedVersion!.Version;
            var exeVersion = SelectedVersion!.Version;


            _wjClient.SendMetric("started_ussedp", $"{exeVersion}_{selectedVersion}").FireAndForget();
            
            var versions = new (uint AppId, string Version)[]
            {
                (seAppId, selectedVersion),
                (seAppId, exeVersion)
            };
            versions = versions.Distinct().ToArray();
            
            await _steamClient.Login();
            
            _logger.LogInformation("Caching manifests");

            var resolvedRecords = new Dictionary<string, ResolveRecord>();
            
            foreach (var v in versions)
            {
                await using var rs =
                    typeof(MainWindowViewModel).Assembly.GetManifestResourceStream(
                        $"Patcher.GameDefinitions.{v.Version}.json");
                
                var archives = JsonSerializer.Deserialize<Archive[]>(rs!, _dtos.Options)!;
            
                await using var ms =
                    typeof(MainWindowViewModel).Assembly.GetManifestResourceStream(
                        $"Patcher.GameDefinitions.{v.Version}_steam_manifests.json");
                var steamManifests = JsonSerializer.Deserialize<Wabbajack.DTOs.SteamManifest[]>(ms!, _dtos.Options)!;


                var resolved = new List<DepotManifest>();
                foreach (var m in steamManifests)
                {
                    
                    _logger.LogInformation("Getting Manifest {DepotID} - {ManifestId}", m.Depot, m.Manifest);
                    resolved.Add(await _steamClient.GetAppManifest(v.AppId, m.Depot, m.Manifest));
                }
                resolvedRecords.Add(v.Version, new ResolveRecord
                {
                    AppId = v.AppId,
                    Archives = archives,
                    Manifest = steamManifests,
                    DepotManifest = resolved.ToArray(),
                    Version = v.Version
                });
            }

            _logger.LogInformation("Creating installation plan");

            var oldExtensions = new HashSet<Extension>()
            {
                EXE, DLL
            };

            var exeFiles = resolvedRecords[exeVersion].Archives
                .Where(f => oldExtensions.Contains(((GameFileSource) f.State).GameFile.Extension))
                .Select(f => new {Version = exeVersion, File = f});
            
            var assetFiles = resolvedRecords[selectedVersion].Archives
                .Where(f => !oldExtensions.Contains(((GameFileSource) f.State).GameFile.Extension))
                .Select(f =>  new {Version = selectedVersion, File = f});;

            var filesQuery = from tuple in exeFiles.Concat(assetFiles)
                let srcRecords = resolvedRecords[tuple.Version]
                let file = tuple.File
                let state = (GameFileSource)tuple.File.State
                from manifest in srcRecords.DepotManifest
                from manifestFile in manifest.Files
                where manifestFile.FileName == state.GameFile.ToString()
                where (ulong)file.Size == manifestFile.TotalSize
                select new ResolvedFile
                {
                    AppId = srcRecords.AppId,
                    DepotId = manifest.DepotID,
                    ManifestId = manifest.ManifestGID,
                    FileData = manifestFile,
                    Path = state.GameFile,
                    State = state,
                    Archive = file
                };

            var files = filesQuery.ToList();
            _logger.LogInformation("Resolved {TotalSize} of files to download", files.Sum(f => (long)f.FileData.TotalSize).ToFileSizeString());
            
            _logger.LogInformation("Starting download");

            _totalJob = new DummyJob
            {
                Description = "Downloading Skyrim",
                Size = files.Sum(f => (long)f.FileData.TotalSize) >> 8
            };

            foreach (var file in files)
            {
                _logger.LogInformation("Checking {Filename}", file.Path);
                var output = file.Path.RelativeTo(outputPath);

                if (output.FileExists())
                {
                    if (output.Size() == file.Archive.Size)
                    {
                        var hash = await output.Hash();
                        if (hash == file.Archive.Hash)
                        {
                            _logger.LogInformation("File is unmodified, skipping");
                            await _totalJob.Report((int)(file.FileData.TotalSize >> 8), CancellationToken.None);
                            continue;
                        }
                        
                    }
                    output.Delete();
                }
                
                _logger.LogInformation("Downloading {Filename}", file.Path);
                output.Parent.CreateDirectory();
                _currentJob = new DummyJob()
                {
                    Description = $"Downloading {file.FileData.FileName}",
                    Size = (long) file.FileData.TotalSize
                };
                await _steamClient.Download(file.AppId, file.DepotId, file.ManifestId, file.FileData, output, CancellationToken.None, _currentJob);
                await _totalJob.Report((int)(file.FileData.TotalSize >> 8), CancellationToken.None);
                _currentJob = null;
            }

            _totalJob = null;
            
            _wjClient.SendMetric("ended_ussedp", $"{exeVersion}_{selectedVersion}").FireAndForget();
            _logger.LogInformation("Worked completed, logging you out of Steam");
            await _token.Delete();
            await SetSteamStatus();
            _logger.LogInformation("Finished download, enjoy your game!");

        }
        
        
        private class DummyJob : IJob
        {
            public async ValueTask Report(int processedSize, CancellationToken token)
            {
                Interlocked.Add(ref _current, processedSize);
            }

            private long _current = 0;

            public ulong ID { get; } = 0;
            public long? Size { get; set; }
            public long Current => _current;
            public string Description { get; init; }
        }

        public class ResolvedFile
        {
            public RelativePath Path { get; set; }
            public uint DepotId { get; set; }
            public uint AppId { get; set; }
            public ulong ManifestId { get; set; }
            public DepotManifest.FileData FileData { get; set; }
            public GameFileSource State { get; set; }
            public Archive Archive { get; set; }
        }

        public class ResolveRecord
        {
            public Archive[] Archives { get; set; }
            public SteamManifest[] Manifest { get; set; }
            public DepotManifest[] DepotManifest { get; set; }
            public string Version { get; set; }
            public uint AppId { get; set; }
        }

    }
}
