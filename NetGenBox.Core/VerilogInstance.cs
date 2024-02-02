using System.Collections.ObjectModel;

namespace NetGenBox.Core;

public class VerilogInstance
{
    public required string InstanceName { get; set; }
    public required string ReferenceModuleName { get; set; }

    public Dictionary<string, string> Pins = new();
 
    
    public override string ToString()
    {
        if (Pins.Count == 0)
            return string.Empty;
        var pins = ReferenceModuleName.EndsWith("_n") ? $"{{ {string.Join(", ", Pins.Values)} }}" : 
                $"{string.Join(", ", Pins.Keys.Order().Select(t => $".{t}({Pins[t]})").ToList())}";
        
        return $"{ReferenceModuleName}\t{InstanceName} ({pins});";
    }
    
    
}