
using System.IO.Compression;
using System.Text.Json;
using DTOs;
using Ussedp;
using Wabbajack.Hashing.xxHash64;
using Wabbajack.Paths;
using Wabbajack.Paths.IO;

var fromDir = args[0].ToAbsolutePath();
var toDir = args[1].ToAbsolutePath();
var outputDir = args[2].ToAbsolutePath();


var results = new List<Instruction>();

foreach (var fromFile in fromDir.EnumerateFiles())
{
    var relative = fromFile.RelativeTo(fromDir);
    var toFile = relative.RelativeTo(toDir);

    results.Add(await Generator.CompareAndGenerate(fromFile, toFile, relative));
}

var toPatch = results.Where(p => p.Method == ResultType.Patched).ToList();

Console.WriteLine($"Building {toPatch.Count} patches");

foreach (var patch in toPatch)
{
    patch.PatchFile = (await Generator.CreatePatch(patch.Path.ToRelativePath(), fromDir, toDir, outputDir)).ToString();
}

var instructions = results.Where(r => r.Method != ResultType.Identical).ToList();

Console.WriteLine($"Found {instructions.Count} instructions");

await using var os = outputDir.Combine("instructions.json").Open(FileMode.Create, FileAccess.ReadWrite, FileShare.None);
await JsonSerializer.SerializeAsync(os, instructions, new JsonSerializerOptions
{
    WriteIndented = true,
});
await os.DisposeAsync();

await using var outputStream = outputDir.Combine("patch.zip").Open(FileMode.Create, FileAccess.ReadWrite);
using var archive = new ZipArchive(outputStream, ZipArchiveMode.Create, true);

foreach (var file in outputDir.EnumerateFiles().Where(f => f.Extension != new Extension(".zip")))
{
    Console.WriteLine($"Compressing {file.RelativeTo(outputDir)}");
    archive.CreateEntryFromFile(file.ToString(), file.RelativeTo(outputDir).ToString(), CompressionLevel.SmallestSize);
    file.Delete();
}
