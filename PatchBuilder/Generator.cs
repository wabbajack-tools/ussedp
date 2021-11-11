using DTOs;
using Wabbajack.Hashing.xxHash64;
using Wabbajack.Paths;
using Wabbajack.Paths.IO;

namespace Ussedp;

public static class Generator
{
    public static async Task<Hash> HashFile(AbsolutePath file)
    {
        await using var s = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
        return await s.HashingCopy(Stream.Null, CancellationToken.None);
    }
    public static async Task<Instruction> CompareAndGenerate(AbsolutePath src, AbsolutePath dest, RelativePath relativePath, RelativePath fromFileRelative)
    {
        Console.WriteLine($"Checking {relativePath}");
        if (!dest.FileExists())
        {
            Console.WriteLine($"Delete: {relativePath}");
            return new Instruction
            {
                Method = ResultType.Deleted,
                SrcHash = 0,
                DestHash = 0,
                Path = relativePath.ToString()
            };
        }
        
        var hashSrcTask = HashFile(src);
        var hashDestTask = HashFile(dest);

        var srcHash = await hashSrcTask;
        var destHash = await hashDestTask;

        if (srcHash != destHash)
        {
            Console.WriteLine($"Diff: {relativePath}");
            return new Instruction
            {
                Method = ResultType.Patched,
                FromFile = fromFileRelative.ToString(),
                SrcHash = (long)srcHash,
                DestHash = (long)destHash,
                Path = relativePath.ToString()
            };;
        }

        return new Instruction
        {
            Method = ResultType.Identical,
            SrcHash = (long)srcHash,
            DestHash = (long)destHash,
            Path = relativePath.ToString()
        };
    }

    public static async Task<RelativePath> CreatePatch(RelativePath srcPath, RelativePath destPath, AbsolutePath fromPath, AbsolutePath toPath,
        AbsolutePath outDir)
    {
        var oldData = await srcPath.RelativeTo(fromPath).ReadAllBytesAsync();
        var newData = await destPath.RelativeTo(toPath).ReadAllBytesAsync();

        var guid = (await oldData.Hash()).ToHex() + "_" + (await newData.Hash()).ToHex();
        await using var os = guid.ToRelativePath().RelativeTo(outDir).Open(FileMode.Create, FileAccess.ReadWrite);
        
        Console.WriteLine($"Diffing: {srcPath} -> {destPath}");
        OctoDiff.Create(oldData, newData, os);
        return guid.ToRelativePath();
    }
}