using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Mixins;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using Patcher.ViewModels;
using ReactiveUI;
using Wabbajack.Paths;
using Wabbajack.Paths.IO;
using Wabbajack.RateLimiter;

namespace Patcher.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            this.WhenActivated(disposables =>
            {
                this.OneWayBind(ViewModel, vm => vm.GamePath, view => view.GameLocation.Text,
                        p => p.ToString())
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel, vm => vm.LogLines, view => view.Log.Text,
                        p => string.Join("\n", p.Reverse()))
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel, vm => vm.StartPatching, view => view.StartButton)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel, vm => vm.LoginCommand, view => view.LoginButton)
                    .DisposeWith(disposables);
                
                this.BindCommand(ViewModel, vm => vm.LogoutCommand, view => view.LogoutButton)
                    .DisposeWith(disposables);

                ViewModel.WhenAnyValue(vm => vm.IsLoggedIn)
                    .Select(l => !l)
                    .BindTo(this, view => view.SteamUsername.IsEnabled)
                    .DisposeWith(disposables);
                
                ViewModel.WhenAnyValue(vm => vm.IsLoggedIn)
                    .Select(l => !l)
                    .BindTo(this, view => view.SteamPassword.IsEnabled)
                    .DisposeWith(disposables);

                this.Bind(ViewModel, vm => vm.SteamUsername, view => view.SteamUsername.Text)
                    .DisposeWith(disposables);
                
                this.Bind(ViewModel, vm => vm.SteamPassword, view => view.SteamPassword.Text)
                    .DisposeWith(disposables);

                ViewModel.WhenAnyValue(view => view.GameVersions)
                    .BindTo(this, view => view.GameOptions.Items)
                    .DisposeWith(disposables);

                this.Bind(ViewModel, vm => vm.SelectedVersion, view => view.GameOptions.SelectedItem)
                    .DisposeWith(disposables);

                ViewModel.WhenAnyValue(vm => vm.TotalProgress)
                    .Select(v => v.Value)
                    .BindTo(this, view => view.TotalProgress.Value)
                    .DisposeWith(disposables);

                ViewModel.WhenAnyValue(vm => vm.JobProgress)
                    .Select(v => v.Value)
                    .BindTo(this, view => view.JobProgress.Value)
                    .DisposeWith(disposables);

                this.Bind(ViewModel, vm => vm.BestOfBothWorlds, view => view.BestOfBothWorlds.IsChecked)
                    .DisposeWith(disposables);

            });
            
        }



        private void OpenPatreon(object? sender, RoutedEventArgs e)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var info = new ProcessStartInfo()
                {
                    FileName = "cmd.exe".ToRelativePath()
                        .RelativeTo(Environment.GetFolderPath(Environment.SpecialFolder.System).ToAbsolutePath())
                        .ToString(),
                    Arguments = "/C rundll32 url.dll,FileProtocolHandler https://www.nexusmods.com/users/17252164"
                };
                Process.Start(info);
            }
        }

        private void FindGameFile(object? sender, RoutedEventArgs e)
        {
            Task.Run(async () =>
            {
                var fod = new OpenFolderDialog();
                var result = await fod.ShowAsync(this);
                if (result == null) return;

                Dispatcher.UIThread.Post(() => {
                    ViewModel!.GamePath = result.ToAbsolutePath();
                });
            });
        }
    }
}