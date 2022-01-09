using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using DTOs;
using GameFinder.StoreHandlers.Origin.DTO;
using GameFinder.StoreHandlers.Steam;
using MessageBox.Avalonia;
using Microsoft.Extensions.Logging;
using Octodiff.Core;
using Octodiff.Diagnostics;
using Patcher.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Ussedp;
using Wabbajack.Common;
using Wabbajack.DTOs;
using Wabbajack.Hashing.xxHash64;
using Wabbajack.Networking.Http.Interfaces;
using Wabbajack.Networking.Steam;
using Wabbajack.Paths;
using Wabbajack.Paths.IO;

namespace Patcher.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, IActivatableViewModel
    {
        private readonly Client _steamClient;
        private readonly ILogger<MainWindowViewModel> _logger;
        private readonly LoggerProvider _loggerProvider;
        private readonly ITokenProvider<SteamLoginState> _token;
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


        public void Log(string line)
        {
            LogLines = LogLines.Append(line).ToArray();
        }
        
        public MainWindowViewModel(ILogger<MainWindowViewModel> logger, Client steamClient, LoggerProvider loggerProvider, 
            ITokenProvider<SteamLoginState> token)
        {
            _logger = logger;
            _token = token;
            _steamClient = steamClient;
            _loggerProvider = loggerProvider;
            Activator = new ViewModelActivator();

            _loggerProvider.Messages
                .Subscribe(l => Log(l.LongMessage));
            
            _logger.LogInformation("Started USSEDP");

            SetSteamStatus().FireAndForget();
            
            //var tsk = LocateAndSetGame(Game.SkyrimSpecialEdition);
            StartPatching = ReactiveCommand.CreateFromTask(() => Start(), 
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
                IsLoggedIn = false;
                return;
            }

            var token = await _token.Get();

            if (string.IsNullOrEmpty(token?.User) || string.IsNullOrEmpty(token?.Password))
            {
                IsLoggedIn = false;
                return;
            }

            SteamUsername = token!.User;

            if ((token?.SentryFile ?? Array.Empty<byte>()).Length == 0)
            {
                IsLoggedIn = false;
                return;
            }

            IsLoggedIn = true;
        }

        private async Task Start()
        {
            _logger.LogInformation("Starting Patching");

        }

    }
}
