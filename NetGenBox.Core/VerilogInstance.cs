namespace NetGenBox.Core;

public class VerilogInstance
{
    public required string InstanceName { get; set; }
    public required string ReferenceModuleName { get; set; }

    public Dictionary<string, string> Inputs = new();
    public Dictionary<string, string> Outputs = new();
    
    public override string ToString()
    {
        var @in = ReferenceModuleName.EndsWith("_n") ? $"{{ {string.Join(", ", Inputs.Values)} }}" : 
                $"{string.Join(", ", Inputs.Keys.Order().Select(t => $".{t}({Inputs[t]})").ToList())}";
        
        var @out = ReferenceModuleName.EndsWith("_n")
            ? $"{string.Join(", ", Outputs.Values)}"
            : $"{string.Join(", ", Outputs.Keys.Order().Select(t => $".{t}({Outputs[t]})").ToList())}";
            
        return $"{ReferenceModuleName}\t{InstanceName} ({@out},  {@in})";
    }
}