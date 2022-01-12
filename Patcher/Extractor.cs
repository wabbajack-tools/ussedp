using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DTOs;
using Patcher.ViewModels;
using Wabbajack.Paths;
using Wabbajack.Paths.IO;

namespace Ussedp;

public class Extractor
{
    private static Dictionary<string, Entry> _offsets = new();
    private static bool _isInited = false;

    public static async Task InitTable(MainWindowViewModel main)
    {
        if (_isInited) return;
        try
        {
            _isInited = true;
            main.Log($"Checking for magic header in {Environment.ProcessPath?.ToAbsolutePath()}");
            var bc = Constants.ExtractorMagic.Length;
            await using var file = Environment.ProcessPath?.ToAbsolutePath().Open(FileMode.Open);
            
            var br = new BinaryReader(file);
            file.Position = file.Length - (10 * 1024);
            long offset = Constants.ExtractorMagic.Length;
            var found = false;
            while (offset < 10 * 1024)
            {
                file.Position = file.Length - offset;
                var bytes = br.ReadBytes(bc);
                if (bytes.SequenceEqual(Constants.ExtractorMagic))
                {
                    found = true;
                    break;
                }
                offset++;
            }

            if (!found)
            {
                main.Log("No header magic found");
                return;
            }
            
            main.Log("Header matches, loading offsets");
            file.Position = file.Length - offset - sizeof(long) - sizeof(long);
            var jsonOffset = br.ReadInt64();
            var jsonSize = br.ReadInt64();
            file.Position = jsonOffset;


            _offsets = JsonSerializer.Deserialize<Dictionary<string, Entry>>(br.ReadBytes((int)jsonSize))!;

        }
        catch (Exception ex)
        {
            File.WriteAllText(@"c:\tmp\crashlong.txt", ex.ToString());
        }
    }

    public static async Task<byte[]> LoadFile(string name)
    {
        if (!_offsets.TryGetValue(name, out var entry)) 
            return await File.ReadAllBytesAsync(Path.Combine(name));
        
        await using var file = Environment.ProcessPath?.ToAbsolutePath().Open(FileMode.Open);
        file.Position = entry.Offset;
        var data = new byte[entry.Size];
        file.Read(data);
        return data;
    }
}