using System.Reflection;
using System.Text.RegularExpressions;
using NetGenBox.Core.Xilinx;

namespace NetGenBox.Core;




public class VerilogIo
{
    public required string Name { get; set; }
    public Pin.PinType PinType { get; set; }
}

public class VerilogModule
{
    public required string ModuleName { get; set; }
    public List<string> IoDefinitions { get; set; } = new();
    public List<VerilogIo> Inputs { get; set; } = new();
    public List<VerilogIo> Outputs { get; set; } = new();

    public string ExportString(string moduleBody)
    {
        string verilogCode = $"module {ModuleName} ";
        verilogCode += $"( {string.Join(", ", IoDefinitions)} );\n\n";
        Inputs.ForEach(t =>
        {
            verilogCode += "input " + t.Name + ";\n";
        });
        Outputs.ForEach(t =>
        {
            verilogCode += "output " + t.Name + ";\n";
        });
        if (!string.IsNullOrEmpty(moduleBody))
        {
            verilogCode += "\n\n\n\n";
            verilogCode += moduleBody;
        }

        verilogCode += "\n\nendmodule;";
        return verilogCode;
    }
}

public class VerilogModuleParser
{
    
    public static VerilogModule? ParseModule(string module)
    {
        // Pattern to match the module name and its ports
        string modulePattern = @"module\s+(\w+)\s*\((.*?)\);";
        // Pattern to match input and output declarations
        string ioPattern = @"(input|output)\s*((?:\[\d+:\d+\])?\s*\w+)";

        // Find the module declaration
        var moduleMatch = Regex.Match(module, modulePattern);
        if (moduleMatch.Success)
        {
            
            string moduleName = moduleMatch.Groups[1].Value;
            var verilogModule = new VerilogModule()
            {
                ModuleName = moduleName
            };

            string ports = moduleMatch.Groups[2].Value;
            verilogModule.IoDefinitions = ports.Split(',').Select(t => t.TrimStart().TrimEnd()).ToList();

            // Find all input and output declarations
            var matches = Regex.Matches(module, ioPattern);
            foreach (Match m in matches)
            {
                var type = m.Groups[1].Value;
                var pinName = m.Groups[2].Value;
                if (type.ToLower() == "input")
                {
                    verilogModule.Inputs.Add(new VerilogIo()
                    {
                        PinType = Pin.PinType.Input,
                        Name = pinName
                    });
                }
                else if (type.ToLower() == "output")
                {
                    verilogModule.Outputs.Add(new VerilogIo()
                    {
                        Name = pinName,
                        PinType = Pin.PinType.Output
                    });
                }
                //Console.WriteLine($"{m.Groups[1].Value} - {m.Groups[2].Value}");
            }

            return verilogModule;
        }

        return null;
    }
}