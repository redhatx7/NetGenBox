using NetGenBox.Core.Xilinx;

namespace NetGenBox.Core;

public class VerilogGenerator
{
    //private readonly NetListExtractor _netListExtractor;

    private int[] _refCounts = new int[Enum.GetNames<CellType>().Length];
    private Dictionary<string, VerilogInstance> _instances = new();
    private readonly IList<Net> _nets;

    private readonly Dictionary<string, string> renamedCells = new()
    {
        { "AND2", "and_n" },
        {"FDCE", "dff"},
        {"FD", "dff"},
        {"LD", "dff"},
        {"LDCE", "dff"},
        {"LDCP", "dff"},
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
    
    public VerilogGenerator(IList<Net> nets)
    {
        _nets = nets;
    }


    void AddPortToInstance(VerilogInstance instance, PortReference portRef, string netName)
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
    
    public string GenerateGateLevel()
    {
        List<string> wires = new();
        int wireCount = 1;
        foreach (var net in _nets)
        {
            string netName = $"wire_{wireCount++}";
            wires.Add(netName);
            foreach (var portRef in net.PortReferences)
            {
                if (portRef.Instance is not null)
                {
                    if (_instances.ContainsKey(portRef.Instance.InstanceName) == false)
                    {
                        var renamedCell = renamedCells.ContainsKey(portRef.Instance.ReferenceCell.Name)
                            ? renamedCells[portRef.Instance.ReferenceCell.Name]
                            : portRef.Instance.ReferenceCell.Name;
                        _instances.Add(portRef.Instance.InstanceName, new VerilogInstance()
                        {
                            ReferenceModuleName = renamedCell,
                            InstanceName = $"{renamedCell}_{_refCounts[(int)portRef.Instance.ReferenceCell.CellType]++}"
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

        return output;
    }
    
     private static void ParseBuf(VerilogInstance instance, PortReference portRef, string netName)
    {
        if (portRef.PortName == "I" || portRef.PortName == "O" )
        {
            instance.Pins.TryAdd(portRef.PortName, netName);
        }
       
    }

    // module FDCE (Q, C, CE, CLR, D);
    private static void ParseFdce(VerilogInstance instance, PortReference portRef, string netName)
    {
        if (portRef.PortName == "D" || portRef.PortName == "C" 
                                    || portRef.PortName == "CLR" || portRef.PortName == "CE" || portRef.PortName == "Q")
        {
            instance.Pins.TryAdd(portRef.PortName, netName);
        }
       
    }

    // module LDCP (Q, CLR, D, G, PRE)
    private static void ParseLdcp(VerilogInstance instance, PortReference portRef, string netName)
    {
        if (portRef.PortName == "D"
            || portRef.PortName == "CLR" || portRef.PortName == "PRE" || portRef.PortName == "Q" )
        {
            instance.Pins.TryAdd(portRef.PortName, netName);
        }
        else if (portRef.PortName == "G" )
        {
            instance.Pins.TryAdd("CE", netName);
        }
      
    }

    private static void ParseInv(VerilogInstance instance, PortReference portRef, string netName)
    {
        if (portRef.PortName == "I" || portRef.PortName == "O" )
        {
            instance.Pins.TryAdd(portRef.PortName, netName);
        }
     
    }

    //(Q, D, C, CLR, PRE, CE)
    private static void ParseLd(VerilogInstance instance, PortReference portRef, string netName)
    {
        //(Q, CLR, D, G, GE);
        if (instance.Pins.Count == 0)
        {
            instance.Pins.Add("CLR", "1'b0");
            instance.Pins.Add("PRE", "1'b0");
            instance.Pins.Add("CE", "1'b1");
        }
        if (portRef.PortName == "D" || portRef.PortName == "G" || portRef.PortName == "Q")
        {
            instance.Pins.TryAdd(portRef.PortName, netName);
        }
        
    }

    private static void ParseFd(VerilogInstance instance, PortReference portRef, string netName)
    {
        if (instance.Pins.Count == 0)
        {
            instance.Pins.Add("CE", "1'b1");
        }
        if (portRef.PortName == "C" || portRef.PortName == "D" || portRef.PortName == "Q")
        {
            instance.Pins.Add(portRef.PortName, netName);
        }
    }

    private static void ParseAndOr(VerilogInstance instance, PortReference portRef, string netName)
    {
        if (portRef.PortName.StartsWith("I") || portRef.PortName == "O" )
        {
            instance.Pins.TryAdd(portRef.PortName, netName);
        }
    }
}