
using Avalonia.Controls.Mixins;
using Avalonia.ReactiveUI;
using Patcher.ViewModels;
using ReactiveUI;

namespace Patcher.Views;

public partial class GameView : ReactiveUserControl<GameViewModel>
{
    public GameView()
    {
        InitializeComponent();
        
        this.WhenActivated(disposables =>
        {
            ViewModel.WhenAnyValue(vm => vm.Name)
                .BindTo(this, view => view.GameName.Text)
                .DisposeWith(disposables);
            ViewModel.WhenAnyValue(vm => vm.Version)
                .BindTo(this, view => view.Version.Text)
                .DisposeWith(disposables);
            ViewModel.WhenAnyValue(vm => vm.ReleaseDate)
                .BindTo(this, view => view.Date.Text)
                .DisposeWith(disposables);

        });
    }
}