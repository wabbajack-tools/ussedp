namespace ussedp;

public class RegistryVersion
{
    public string Name { get; set; }
    public string Version { get; set; }

    public static RegistryVersion[] Registry = {
        new() {Name = "1.5.97.0 - Skyrim SE", Version = "1.5.97.0"},
        new() {Name = "1.6.318.0 - Skyrim AE - Nov 11th, 2021", Version = "1.5.97.0"},
        new() {Name = "1.6.323.0 - Skyrim AE - Nov 22th, 2021", Version = "1.6.323.0"},
        new() {Name = "1.6.324.0 - Skyrim AE - Dec 13th, 2021", Version = "1.6.324.0"},
        new() {Name = "1.6.353.0 - Skyrim AE - Jan 6th, 2021", Version = "1.6.353.0"}
    };
}