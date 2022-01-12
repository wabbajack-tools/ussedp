using System.Text;
using System.Text.Json.Serialization;

namespace DTOs;

public class Constants
{
    public static byte[] ExtractorMagic = Encoding.UTF8.GetBytes("WJ_TOOLS_SELF_EXTRACTOR");
}

public class Entry
{
    [JsonPropertyName("offset")]
    public long Offset { get; set; }
    
    [JsonPropertyName("size")]
    public long Size { get; set; }
    
}