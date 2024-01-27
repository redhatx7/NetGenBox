using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;

namespace NetGenBox.CLI;

[Command("synthesis")]
public class SynthesisCommand : ICommand
{
    [CommandOption("ise", 'i', Description = "Path of Xilinx ISE, ../ISE/")]
    public required string IsePath { get; set; }

    [CommandOption("device-family", 'd',Description = "Device Family",IsRequired = false)]
    public string DeviceFamily { get; set; }
    
    [CommandOption("sources", 's',Description = "List of verilog sources, seperated by comma")]
    public required IReadOnlyList<string> VerilogSources { get; set; }

    [CommandOption("tcl", 't', Description = "Use custom tcl script")]
    public string CustomTclScript { get; set; }
    
    public ValueTask ExecuteAsync(IConsole console)
    {
        foreach (var item in VerilogSources)
        {
            Console.WriteLine(item);
        }
        return default;
    }
}