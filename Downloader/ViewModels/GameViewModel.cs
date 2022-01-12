using ReactiveUI.Fody.Helpers;

namespace Patcher.ViewModels;

public class GameViewModel : ViewModelBase
{
    [Reactive]
    public string Name { get; set; }
    
    [Reactive]
    public string Version { get; set; }
    
    [Reactive]
    public string ReleaseDate { get; set; }

    public static GameViewModel[] GameVersions { get; set; } = {
        new() {Name = "Skyrim SE", Version = "1.5.97.0", ReleaseDate = "20 Nov, 2019"},
        new() {Name = "Skyrim AE", Version = "1.6.318.0", ReleaseDate = "11 Nov, 2021"},
        new() {Name = "Skyrim AE", Version = "1.6.323.0", ReleaseDate = "22 Nov, 2021"},
        new() {Name = "Skyrim AE", Version = "1.6.342.0", ReleaseDate = "12 Dec, 2021"},
        new() {Name = "Skyrim AE", Version = "1.6.353.0", ReleaseDate = "6 Jan, 2022"}
    };
}