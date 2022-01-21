// A "Do it all" utility for building all the versions of the patchers we need. 

using System.Text;
using System.Text.Json;
using BuildAllPatches;
using DTOs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wabbajack.Common;
using Wabbajack.DTOs;
using Wabbajack.Hashing.xxHash64;
using Wabbajack.Networking.NexusApi;
using Wabbajack.Networking.NexusApi.DTOs;
using Wabbajack.Paths;
using Wabbajack.Paths.IO;
using Wabbajack.RateLimiter;
using Wabbajack.Services.OSIntegrated;
using Wabbajack.VFS;

var ModId = 57618;

var workingFolder = args[0].ToAbsolutePath();
var host = Host.CreateDefaultBuilder()
    .ConfigureServices(s =>
    {
        s.AddOSIntegrated();
    });

var built = host.Build();
var nexusClient = built.Services.GetRequiredService<NexusApi>();

var limiter = new Resource<FileHashCache>("File Hashing", 10);
var hashCache = new FileHashCache(workingFolder.Combine("hash_cache.sqlite"), limiter);


var srcVersions = workingFolder.Combine("src_versions");

var versions = srcVersions.EnumerateDirectories(recursive: false)
    .Where(f => Version.TryParse(f.FileName.ToString(), out _))
    .ToDictionary(f => Version.Parse(f.FileName.ToString()), f => f);
    

Console.WriteLine($"Found {versions.Count} versions");


var ordered = versions.OrderByDescending(f => f.Key).ToArray();

var newestVersion = ordered.First().Key;
var oldestVersion = ordered.Last().Key;

Console.WriteLine($"Most recent version is {oldestVersion}");

var pairs = new List<Build>();

for (var skip = 0; skip < ordered.Length - 1; skip++)
{
    var topVersion = ordered.Skip(skip).First();
    pairs.AddRange(ordered.Skip(skip + 1)
        .Select(f => new Build
        {
            BestOfBothWorlds = false,
            FromVersion = topVersion.Key,
            FromPath = topVersion.Value,
            ToVersion = f.Key,
            ToPath = f.Value
        }));
}

Console.WriteLine($"Found {pairs.Count} downgrade groups to process");

var builds = new List<(Build Build, Instruction[] Instructions)>();
foreach (var pair in pairs)
{
    Console.WriteLine($"Generating {pair.FromVersion} -> {pair.ToVersion}");
    var fromFiles = pair.FromPath.EnumerateFiles().ToDictionary(f => f.RelativeTo(pair.FromPath), f => f);
    var toFiles = pair.ToPath.EnumerateFiles().ToDictionary(f => f.RelativeTo(pair.ToPath), f => f);
    
    var plan = (await Generator.GeneratePlan(pair.FromVersion, pair.ToVersion, fromFiles, toFiles, hashCache).ToArray())
        // Don't patch a file until we've patched all files that require it
        .OrderBy(x => x.FromFile == x.Path ? 1 : 0)
        .ToArray();
    Console.WriteLine($"Found {plan.Length} instructions");
    builds.Add((pair, plan));
}

var patchType = new HashSet<Extension>()
{
    new(".exe"),
    new(".dll")
};

Console.WriteLine($"Generating {pairs.Count} Best of Both Worlds variants");
foreach (var pair in pairs)
{
    Console.WriteLine($"Generating {pair.FromVersion} -> {pair.ToVersion} - best of both worlds");
    var fromFiles = pair.FromPath.EnumerateFiles().ToDictionary(f => f.RelativeTo(pair.FromPath), f => f);
    var toFiles = pair.ToPath.EnumerateFiles().ToDictionary(f => f.RelativeTo(pair.ToPath), f => f);

    foreach (var (key, _) in toFiles.ToArray())
    {
        if (!patchType.Contains(key.Extension) && fromFiles.ContainsKey(key))
            toFiles[key] = fromFiles[key];
    }

    foreach (var (key, value) in fromFiles)
    {
        if (!patchType.Contains(key.Extension) && !toFiles.ContainsKey(key))
            toFiles[key] = value;
    }
    
    var plan = (await Generator.GeneratePlan(pair.FromVersion, pair.ToVersion, fromFiles, toFiles, hashCache).ToArray())
        // Don't patch a file until we've patched all files that require it
        .OrderBy(x => x.FromFile == x.Path ? 1 : 0)
        .ToArray();
    Console.WriteLine($"Found {plan.Length} instructions");
    var newBuild = new Build()
    {
        BestOfBothWorlds = true,
        FromPath = pair.FromPath,
        FromVersion = pair.FromVersion,
        ToPath = pair.ToPath,
        ToVersion = pair.ToVersion
    };
    builds.Add((newBuild, plan));
}

Console.WriteLine($"Generated {builds.Count} builds");

var patches = (from build in builds
    from inst in build.Instructions
    where inst.Method == ResultType.Patched
    where !Generator.HavePatch(workingFolder, inst.SrcHash, inst.DestHash)
    group (build.Build, inst) by (inst.SrcHash, inst.DestHash) into grouped
    select grouped).ToList();

Console.WriteLine($"Found {patches.Count} patches to build");

await Parallel.ForEachAsync(patches, async (patch, _) =>
{
    await Generator.GeneratePatch(workingFolder, patch.First().Build, patch.First().inst);
});


Console.WriteLine($"Finished writing patches");

Console.WriteLine("Writing instructions");

foreach (var build in builds)
{
    var path = workingFolder.Combine("instructions")
        .Combine($"{build.Build.FromVersion}-{build.Build.ToVersion}-{build.Build.Postfix}");
    Console.WriteLine($"Writing: {path.FileName}");
    await using var fs = path.Open(FileMode.Create, FileAccess.Write, FileShare.Read);
    await JsonSerializer.SerializeAsync(fs, build.Instructions, new JsonSerializerOptions {WriteIndented = true});
}


Console.WriteLine("Writing app");

var proc = new ProcessHelper()
{
    Path = KnownFolders.WindowsSystem32.Combine("cmd.exe"),
    //-r win-x64 -c Release -p:PublishReadyToRun=true --self-contained -o c:\tmp\publish -p:PublishSingleFile=true -p:DebugType=embedded -p:IncludeAllContentForSelfExtract=true
    Arguments = new object[]
    {
        "/c", "dotnet", "publish", KnownFolders.CurrentDirectory.Parent.Combine("Patcher", "Patcher.csproj"),
        "-r", "win-x64", "-c", "Release", "-p:PublishReadyToRun=true", "--self-contained", "-o",
        workingFolder.Combine("published"), "-p:PublishSingleFile=true", "-p:DebugType=embedded",
        "-p:IncludeAllContentForSelfExtract=true"
    }
};

var procDisposable = proc.Output.Subscribe(p =>
{
    if (p.Type == ProcessHelper.StreamType.Error)
        Console.Error.WriteLine(p.Line);
    else
        Console.Out.WriteLine(p.Line);
});

var result = await proc.Start();
if (result != 0)
{
    Console.WriteLine("Build Error");
    Environment.Exit(result);
}
procDisposable.Dispose();

Console.WriteLine("Writing EXEs");
var exeData = await workingFolder.Combine("published").Combine("Patcher.exe").ReadAllBytesAsync();

var outputFolder = workingFolder.Combine("uploads");

foreach (var build in builds)
{
    var name = outputFolder.Combine(build.Build.Postfix + "_" + build.Build.FromVersion + "-" + build.Build.ToVersion +
                                    ".exe");
    
    if (name.FileExists())
        continue;

    var offsets = new Dictionary<string, Entry>();
    
    var patchNames = build.Instructions
        .Where(p => p.Method == ResultType.Patched)
        .Select(p => Hash.FromLong(p.SrcHash).ToHex() + "_" + Hash.FromLong(p.DestHash).ToHex())
        .Distinct();



    Console.WriteLine($"Writing {name.FileName}");
    await using var outStream = name.Open(FileMode.Create, FileAccess.Write, FileShare.Read);
    
    await outStream.WriteAsync(exeData);


    
    var jsonBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new BuildRecord
    {
        Instructions = build.Instructions,
        Name = $"{build.Build.Postfix}_{build.Build.FromVersion}_{build.Build.ToVersion}_{build.Build.Date}"
    }));
    offsets["instructions.json"] = new Entry {Offset = outStream.Position, Size = jsonBytes.Length};
    await outStream.WriteAsync(jsonBytes);
    
    foreach (var patch in patchNames)
    {
        Console.WriteLine($"- {patch}");
        var patchData = await workingFolder.Combine("patches").Combine(patch).ReadAllBytesAsync();
        offsets[patch] = new Entry {Offset = outStream.Position, Size = patchData.Length};
        await outStream.WriteAsync(patchData);
    }

    var jsonOffsets = outStream.Position;
    await JsonSerializer.SerializeAsync(outStream, offsets);
    var jsonSize = outStream.Position - jsonOffsets;
    var bw = new BinaryWriter(outStream);
    bw.Write(jsonOffsets);
    bw.Write(jsonSize);
    bw.Write(Constants.ExtractorMagic);
    

    
    await outStream.FlushAsync();
    await outStream.DisposeAsync();
    
    Console.WriteLine("Waiting for data to sync");
    await Task.Delay(2000);
    Console.WriteLine("Signing");

    
    var signProc = new ProcessHelper
    {
        Path = @"C:\Program Files (x86)\Windows Kits\10\bin\10.0.19041.0\x64\signtool.exe".ToAbsolutePath(),
        Arguments = new object[]
        {
            "sign", "/t", "http://timestamp.sectigo.com", name
        }
    };

    var signProcDisposable = proc.Output.Subscribe(p =>
    {
        if (p.Type == ProcessHelper.StreamType.Error)
            Console.Error.WriteLine(p.Line);
        else
            Console.Out.WriteLine(p.Line);
    });

    var signResult = await signProc.Start();
    if (signResult != 0)
    {
        Console.WriteLine("Build Error");
        Thread.Sleep(1000);
        Environment.Exit(result);
    }
    signProcDisposable.Dispose();
    
    Console.WriteLine("\n");
}


var (response, info) = await nexusClient.ModFiles(Game.SkyrimSpecialEdition.MetaData().NexusName!, ModId);

foreach (var build in builds)
{
    var name = outputFolder.Combine(build.Build.Postfix + "_" + build.Build.FromVersion + "-" + build.Build.ToVersion +
                                    ".exe");

    string section = "Old";
    if (build.Build.FromVersion == newestVersion && build.Build.ToVersion == oldestVersion)
    {
        if (build.Build.BestOfBothWorlds)
        {
            section = "Optional";
        }
        section = "Main";
    }
    
    
    if (response.Files.Any(f => f.Name == name.FileName.ToString() && f.SizeInBytes == name.Size() && f.CategoryName != "ARCHIVED"))
        continue;

    var filesDesc = build.Build.BestOfBothWorlds ? "only game code" : "all files";

    var definition = new UploadDefinition
    {
        Name = name.FileName.ToString(),
        Version = DateTime.UtcNow.ToString("yyyy-MM-dd"),
        Category = section,
        BriefOverview = $"Downgrades {filesDesc} from {build.Build.FromVersion} to {build.Build.ToVersion}",
        Game = Game.SkyrimSpecialEdition,
        ModId = ModId,
        Path = name,
        NewExisting = false,
        RemoveOldVersion = false
    };

    await nexusClient.UploadFile(definition);
}