using Wabbajack.Hashing.xxHash64;
using Wabbajack.Paths;

namespace DTOs;

public class Instruction
{
    public ResultType Method { get; set; }
    public string Path { get; set; }
    public string PatchFile { get; set; }
    public long SrcHash { get; set; }
    public long DestHash { get; set; }
}