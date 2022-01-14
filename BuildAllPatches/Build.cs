using Wabbajack.Paths;

namespace BuildAllPatches;

public class Build
{
    public Version FromVersion { get; init; }
    public AbsolutePath FromPath { get; init; }
    public Version ToVersion { get; init; }
    public AbsolutePath ToPath { get; init; }
    public bool BestOfBothWorlds { get; init; }

    public string Postfix => BestOfBothWorlds ? "bestofboth" : "full";
}