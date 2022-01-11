using System;
using System.Net.Http;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Patcher.ViewModels;
using Patcher.Views;
using ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Patcher.Models;
using Wabbajack.DTOs;
using Wabbajack.DTOs.Interventions;
using Wabbajack.DTOs.JsonConverters;
using Wabbajack.DTOs.Logins;
using Wabbajack.Networking.Http.Interfaces;
using Wabbajack.Networking.NexusApi;
using Wabbajack.Networking.Steam;
using Wabbajack.Paths.IO;
using Wabbajack.RateLimiter;
using Wabbajack.Services.OSIntegrated;
using Wabbajack.Services.OSIntegrated.TokenProviders;
using Wabbajack.Networking.WabbajackClientApi;
using Wabbajack.VFS;
using ServiceExtensions = Wabbajack.Networking.WabbajackClientApi.ServiceExtensions;

namespace Patcher
{
    public class App : Application
    {
        public static IServiceProvider Services { get; private set; } = null!;
        public static MainWindow? MainWindow { get; set; }
        public override void Initialize()
        {
            Dispatcher.UIThread.Post(() => Thread.CurrentThread.Name = "UIThread");
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            var host = Host.CreateDefaultBuilder(Array.Empty<string>())
                .ConfigureLogging(c => { c.ClearProviders(); })
                .ConfigureServices((host, service) =>
                {
                    service.AddAllSingleton<ILoggerProvider, LoggerProvider>();
                    service.AddSteam();
                    service
                        .AddAllSingleton<ITokenProvider<SteamLoginState>, EncryptedJsonTokenProvider<SteamLoginState>,
                            SteamTokenProvider>();
                    service
                        .AddAllSingleton<ITokenProvider<WabbajackApiState>,
                            WabbajackApiTokenProvider>();

                    service.AddSingleton<MainWindowViewModel>();
                    
                    service.AddSingleton<HttpClient>();
                    service.AddAllSingleton<IUserInterventionHandler, UserInterventionHandler>();
                    service.AddDTOSerializer();
                    service.AddDTOConverters();
                    service.AddWabbajackClient();

                    service.AddSingleton(c => new Wabbajack.Services.OSIntegrated.Configuration()
                    {
                        LogLocation = KnownFolders.EntryPoint.Combine("logs")
                    });

                    service.AddSingleton<IResource<HttpClient>>(x => new Resource<HttpClient>("Web Requests", 16));
                    service.AddSingleton<IResource<FileHashCache>>(x => new Resource<FileHashCache>("File Hash", 16));

                }).Build();
            Services = host.Services;



            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                MainWindow = new MainWindow();
                desktop.MainWindow = MainWindow;
                MainWindow.ViewModel = Services.GetRequiredService<MainWindowViewModel>();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}