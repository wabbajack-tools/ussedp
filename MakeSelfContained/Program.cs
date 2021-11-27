using System.Text;
using System.Text.Json;
using DTOs;
using Wabbajack.Paths;
using Wabbajack.Paths.IO;


var srcExe = args[0].ToAbsolutePath();
var srcFolder = args[1].ToAbsolutePath();
var dest = args[2].ToAbsolutePath();


await using var outFile = dest.Open(FileMode.Create, FileAccess.Write, FileShare.None);

await using (var srcFile = srcExe.Open(FileMode.Open))
{
    Console.WriteLine("Writing base EXE");
    await srcFile.CopyToAsync(outFile);
}

var offsets = new Dictionary<string, Entry>();

foreach (var srcPath in srcFolder.EnumerateFiles())
{
    var relative = srcPath.RelativeTo(srcFolder);
    offsets[relative.ToString()] = new Entry{Offset = outFile.Position, Size = srcPath.Size()};
    await using var srcFile = srcPath.Open(FileMode.Open);
    Console.WriteLine($"Writing {relative}");
    await srcFile.CopyToAsync(outFile);
}

Console.WriteLine("Writing JSON");

var jsonOffset = outFile.Position;
await JsonSerializer.SerializeAsync(outFile, offsets);
var jsonSize = outFile.Position - jsonOffset;
var bw = new BinaryWriter(outFile);
bw.Write(jsonOffset);
bw.Write(jsonSize);
bw.Write(Constants.ExtractorMagic);
