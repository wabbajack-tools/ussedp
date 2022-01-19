using Avalonia.OpenGL;
using DTOs;
using Wabbajack.Paths;
using F23.StringSimilarity;
using Ussedp;
using Wabbajack.Common;
using Wabbajack.Hashing.xxHash64;
using Wabbajack.Paths.IO;
using Wabbajack.VFS;

namespace BuildAllPatches;

public class Generator
{
    public static async IAsyncEnumerable<Instruction> GeneratePlan(Version fromVersion, Version toVersion,
        Dictionary<RelativePath, AbsolutePath> fromFiles, Dictionary<RelativePath, AbsolutePath> toFiles,
        FileHashCache fileHashCache)
    {
        var scomp = new Levenshtein();

        var filesToDelete = fromFiles.Where(f => !toFiles.ContainsKey(f.Key))
            .Select(f => f.Key)
            .ToHashSet();

        foreach (var file in filesToDelete)
        {
            yield return new Instruction
            {
                Path = file.ToString(),
                Method = ResultType.Deleted
            };
        }

        foreach (var toFile in toFiles)
        {
            if (fromFiles.TryGetValue(toFile.Key, out var found))
            {
                if (found.Size() == toFile.Value.Size())
                {
                    var toHash = fileHashCache.FileHashCachedAsync(toFile.Value, CancellationToken.None);
                    var fromHash = fileHashCache.FileHashCachedAsync(found, CancellationToken.None);

                    if (await toHash == await fromHash)
                    {
                        continue;
                    }

                    yield return new Instruction
                    {
                        FromFile = toFile.Key.ToString(),
                        Path = toFile.Key.ToString(),
                        SrcHash = (long) await fromHash,
                        DestHash = (long) await toHash,
                        PatchFile = $"{(await fromHash).ToHex()}_{(await toHash).ToHex()}",
                        Method = ResultType.Patched
                    };
                }
            }
            
            var fromFile = fromFiles.Where(f => !filesToDelete.Contains(f.Key))
                .OrderBy(f => scomp.Distance(f.Key.ToString(), toFile.Key.ToString()))
                .First();

            var fromHash2 = fileHashCache.FileHashCachedAsync(fromFile.Value, CancellationToken.None);
            var toHash2 = fileHashCache.FileHashCachedAsync(toFile.Value, CancellationToken.None);

            yield return new Instruction
            {
                FromFile = fromFile.Key.ToString(),
                SrcHash = (long) await fromHash2,
                Path = toFile.Key.ToString(),
                DestHash = (long) await toHash2,
                PatchFile = $"{(await fromHash2).ToHex()}_{(await toHash2).ToHex()}",
                Method = ResultType.Patched
            };
        }
    }

    public static bool HavePatch(AbsolutePath baseDir, long srcHash, long destHash)
    {
        var hashName = Hash.FromLong(srcHash).ToHex() + "_" + Hash.FromLong(destHash).ToHex();
        return baseDir.Combine("patches", hashName).FileExists();
    }

    public static async Task GeneratePatch(AbsolutePath workingFolder, Build build, Instruction inst)
    {
        var fromPath = inst.FromFile.ToRelativePath().RelativeTo(build.FromPath);
        var toPath = inst.Path.ToRelativePath().RelativeTo(build.ToPath);
        
        var oldData = await fromPath.ReadAllBytesAsync();
        var newData = await toPath.ReadAllBytesAsync();

        var guid = (await oldData.Hash()).ToHex() + "_" + (await newData.Hash()).ToHex();
        await using var os = guid.ToRelativePath().RelativeTo(workingFolder.Combine("patches")).Open(FileMode.Create, FileAccess.ReadWrite);
        
        Console.WriteLine($"Diffing: {inst.FromFile} -> {inst.Path}");
        OctoDiff.Create(oldData, newData, os);
    }
}