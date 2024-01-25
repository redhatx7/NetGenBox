using System.Text.RegularExpressions;
using NetGenBox.Core;
using NetGenBox.Core.Xilinx;

namespace NetGenBox.CLI;


class NetInstance
{
    public required XilinxInstance Instance { get; set; }

    public List<(string netName, string portName)> WireConnections { get; set; } = new();

    public void Connect(string netName, string portName)
    {
        WireConnections.Add(new (netName, portName));
    }

    public override string ToString()
    {
        return $"{Instance.ReferenceCell.Name} {(string.IsNullOrEmpty(Instance.RenamedName) ? Instance.RenamedName : Instance.InstanceName)} " +
               $"({string.Join(", ", WireConnections.Select(t => $".{t.portName}({t.netName})"))})";
    }
}

class Program
{
    private static Dictionary<string, VerilogInstance> _instances = new ();
    
    public static void ParseVerilogModule(string verilogCode)
    {
        // Pattern to match the module name and its ports
        string modulePattern = @"module\s+(\w+)\s*\((.*?)\);";
        // Pattern to match input and output declarations
        string ioPattern = @"(input|output)\s+((?:\[\d+:\d+\])?\s*\w+)";

        // Find the module declaration
        var moduleMatch = Regex.Match(verilogCode, modulePattern);
        if (moduleMatch.Success)
        {
            string moduleName = moduleMatch.Groups[1].Value;
            Console.WriteLine("Module Name: " + moduleName);

            string ports = moduleMatch.Groups[2].Value;
            Console.WriteLine("Ports: " + ports);

            // Find all input and output declarations
            var matches = Regex.Matches(verilogCode, ioPattern);
            foreach (Match m in matches)
            {
                Console.WriteLine($"{m.Groups[1].Value} - {m.Groups[2].Value}");
            }
        }
        else
        {
            Console.WriteLine("No module declaration found.");
        }
    }

    
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        var file = File.ReadAllText("/home/amirreza/TestProj/CPU.v");
        ParseVerilogModule(file);





        return;
        //
        string edifPath = "/home/amirreza/TestProj/CPU.ndf";
        string contents = File.ReadAllText(edifPath);
        var lexer = new Parser.Lexer(contents);
        var parser = new Parser.Parser(lexer);
        var list = parser.ParseProgram();
        var netGenBox = new NetListExtractor(list);
        netGenBox.Parse();

        var instances = netGenBox.XilinxInstances;
        var nets = netGenBox.Nets;

        /*
        var dataPathCell = netGenBox.XilinxCells.FirstOrDefault(t => t.Name == "DataPath_dp");
        var dataPathNets = netGenBox.Nets.Where(t => t.BelongsToCell == dataPathCell).ToList();
        var dataPathInstances = netGenBox.XilinxInstances.Where(t => t.ParentCell == dataPathCell).ToList();

        Dictionary<XilinxInstance, NetInstance> dic = new();
        foreach (var net in dataPathNets)
        {
            //string netName = net.RenamedNetName ?? net.NetName;

            
            foreach (var portRef in net.PortReferences)
            {
                var instance = portRef.Instance;
                if(instance is null)
                    continue;
                if(!dic.ContainsKey(instance))
                    dic.Add(instance, new NetInstance(){Instance = instance});

                var netInstance = dic[instance];
                netInstance.Connect(net.PreferredName, portRef.PortName);
            }
            
        }

        foreach (var instance in dic.Values)
        {
            Console.WriteLine(instance.ToString());
        }
        */

        var counts = new int[11];
        int i = 1;
        List<string> wires = new List<string>();
        Dictionary<string, string> renamedCells = new()
        {
            { "AND2", "and_n" },
            {"FDCE", "dff"},
            {"FD", "dff"},
            {"LD", "lde"},
            {"LDCE", "lde"},
            {"LDCP", "lde"},
            {"OBUFE", "bufif1"},
            {"IBUFE", "bufif1"},
            {"BUFE", "bufif1"},
            {"IBUF", "bufg"},
            {"OBUF", "bufg"},
            {"BUF", "bufg"},
            {"INV", "notg"},
            {"OR2", "or_n"},
            {"GND", "GND"},
        };
        foreach (var net in nets)
        {
            string netName = $"wire_{i++}";
            wires.Add(netName);
            foreach (var portRef in net.PortReferences)
            {
                if (portRef.Instance is not null)
                {
                    if (_instances.ContainsKey(portRef.Instance.InstanceName) == false)
                    {
                        _instances.Add(portRef.Instance.InstanceName, new VerilogInstance()
                        {
                            ReferenceModuleName = renamedCells.ContainsKey(portRef.Instance.ReferenceCell.Name) ? renamedCells[portRef.Instance.ReferenceCell.Name] : portRef.Instance.ReferenceCell.Name,
                            InstanceName = $"{portRef.Instance.ReferenceCell.Name}_{counts[(int)portRef.Instance.ReferenceCell.CellType]++}"
                        });
                    }

                    var instance = _instances[portRef.Instance.InstanceName];
                    AddPortToInstance(instance, portRef, netName);

                }
                else
                {
                    
                }
            }
            
        }

        string output = "";
        output += "wire ";
        output += string.Join(",\n\t", wires);
        output += ";\n";
        foreach (var verilogInstance in _instances.Values)
        { 
            output += verilogInstance.ToString() + "\n";
        }
        
        File.WriteAllText("/home/amirreza/outver.v", output);
        
        
        Console.Read();
    }

    private static void AddPortToInstance(VerilogInstance instance, PortReference portRef, string netName)
    {
        switch (portRef.Instance?.ReferenceCell.CellType)
        {
            case CellType.AND or CellType.OR:
                ParseAndOr(instance, portRef, netName);
                break;
            case CellType.FD:
                ParseFd(instance, portRef, netName);
                break;
            case CellType.LD:
                ParseLd(instance, portRef, netName);
                break;
            case CellType.INV:
                ParseInv(instance, portRef, netName);
                break;
            case CellType.LDCP:
                ParseLdcp(instance, portRef, netName);
                break;
            case CellType.FDCE:
                ParseFdce(instance, portRef, netName);
                break;
            case CellType.OBUF or CellType.IBUF:
                ParseBuf(instance, portRef, netName);
                break;
                
        }
    }

    private static void ParseBuf(VerilogInstance instance, PortReference portRef, string netName)
    {
        if (portRef.PortName == "I" && instance.Inputs.Count == 0)
        {
            instance.Inputs.Add(netName);
        }
        else if (portRef.PortName == "O" && instance.Outputs.Count == 0)
        {
            instance.Outputs.Add(netName);
        }
    }

    private static void ParseFdce(VerilogInstance instance, PortReference portRef, string netName)
    {
        if (portRef.PortName == "D" || portRef.PortName == "C" 
                                    || portRef.PortName == "CLR" || portRef.PortName == "CE")
        {
            instance.Inputs.Add(netName);
        }
        else if (portRef.PortName == "Q" && instance.Outputs.Count == 0)
        {
            instance.Outputs.Add(netName);
        }
    }

    private static void ParseLdcp(VerilogInstance instance, PortReference portRef, string netName)
    {
        if (portRef.PortName == "D" || portRef.PortName == "G" 
            || portRef.PortName == "CLR" || portRef.PortName == "PRE")
        {
            instance.Inputs.Add(netName);
        }
        else if (portRef.PortName == "Q" && instance.Outputs.Count == 0)
        {
            instance.Outputs.Add(netName);
        }
    }

    private static void ParseInv(VerilogInstance instance, PortReference portRef, string netName)
    {
        if (portRef.PortName == "I" && instance.Inputs.Count == 0)
        {
            instance.Inputs.Add(netName);
        }
        else if (portRef.PortName == "O" && instance.Outputs.Count == 0)
        {
            instance.Outputs.Add(netName);
        }
    }

    private static void ParseLd(VerilogInstance instance, PortReference portRef, string netName)
    {
        if (portRef.PortName == "D" || portRef.PortName == "G")
        {
            instance.Inputs.Add(netName);
        }
        else if (portRef.PortName == "Q" && instance.Outputs.Count == 0)
        {
            instance.Outputs.Add(netName);
        }
    }

    private static void ParseFd(VerilogInstance instance, PortReference portRef, string netName)
    {
        if (portRef.PortName == "C" || portRef.PortName == "D")
        {
            instance.Inputs.Add(netName);
        }
        else if (portRef.PortName == "Q" && instance.Outputs.Count == 0)
        {
            instance.Outputs.Add(netName);
        }
    }

    private static void ParseAndOr(VerilogInstance instance, PortReference portReference, string netName)
    {
        if (portReference.PortName.StartsWith("I"))
        {
            instance.Inputs.Add(netName);
        }
        else if(portReference.PortName == "O" && instance.Outputs.Count == 0)
        {
            instance.Outputs.Add(netName);
        }
    }

    

    public class VerilogInstance
    {
        public required string InstanceName { get; set; }
        public required string ReferenceModuleName { get; set; }
        
        public List<string> Outputs { get; set; } = new();
        public List<string> Inputs { get; set; } = new();
        public override string ToString()
        {
            return $"{ReferenceModuleName} {InstanceName} ({string.Join(", ", Outputs)}, {{ {string.Join(", ", Inputs)} }})";
        }
    }
  
}