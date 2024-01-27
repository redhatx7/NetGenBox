using System.Text.RegularExpressions;
using CliFx;
using NetGenBox.Core;
using NetGenBox.Core.Xilinx;



namespace NetGenBox.CLI;


class Program
{
    static async Task<int> Main() => await new CliApplicationBuilder()
        .SetTitle("A Simple tool to generate verilog netlist using edif file")
        .SetExecutableName("netgenbox")
        .AddCommand<SynthesisCommand>()
        .AddCommand<NetListCommand>()
        .Build()
        .RunAsync();

}