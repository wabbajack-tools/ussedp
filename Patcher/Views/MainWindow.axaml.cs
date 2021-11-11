using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Mixins;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Patcher.ViewModels;
using ReactiveUI;

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
        

    }
}