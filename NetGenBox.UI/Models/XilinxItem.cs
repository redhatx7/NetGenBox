using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NetGenBox.UI;

public class XilinxDevice
{
    [JsonPropertyName("device_name")]
    public string DeviceName { get; set; }

    public List<string> Speeds { get; set; }
}

public class XilinxItem
{
    [JsonPropertyName("family")]
    public string Family { get; set; }

    [JsonPropertyName("devices")]
    public List<XilinxDevice> Devices { get; set; }
    
}