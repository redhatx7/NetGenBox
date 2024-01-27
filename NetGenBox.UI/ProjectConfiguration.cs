using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NetGenBox.UI;

public class ProjectConfiguration
{
    [JsonPropertyName("basePath")]
    public string BaseProject { get; set; }
    
    [JsonPropertyName("verilogFiles")]
    public List<string> VerilogFiles { get; set; }
    
    [JsonPropertyName("topModule")] public string TopModule { get; set; }
    
    [JsonPropertyName("edifFile")]
    public string EdifFile { get; set; }
    
    [JsonPropertyName("customModulePath")]
    public string CustomModulePath { get; set; }

    public async Task Save()
    {
        FileStream fs = File.OpenWrite(Path.Combine(BaseProject, "netgen.json"));
        await JsonSerializer.SerializeAsync(fs, this);
    }

    public static async Task<ProjectConfiguration?> ReadConfigAsync(string path)
    {
        if (path.EndsWith("netgen.json") == false)
            path = Path.Combine(path, "netgen.json");
        var fs = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<ProjectConfiguration>(fs);
    }
}