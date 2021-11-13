using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Mixins;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Patcher.ViewModels;
using ReactiveUI;
using Wabbajack.Paths;
using Wabbajack.Paths.IO;

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
            string[] result = new string[1];
            var task = Task.Run(async () =>
            {
                var fod = new OpenFileDialog();
                fod.Filters = new List<FileDialogFilter>
                {
                    new()
                    {
                        Name = "SkyrimSE.exe",
                        Extensions = new[] {"exe"}.ToList()
                    }
                };
                result = await fod.ShowAsync(this);
                if (result == null) return;
            });
            task.Wait();
            
            ViewModel!.GamePath = result.First().ToAbsolutePath().Parent;
        }
    }
}