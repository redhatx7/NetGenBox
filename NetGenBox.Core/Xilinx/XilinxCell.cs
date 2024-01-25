using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace NetGenBox.Core.Xilinx;


public enum CellType
{
    GND = 0,
    LD = 1,
    FD = 2,
    INV = 3,
    AND = 4,
    OR = 5,
    LDCP = 6,
    FDCE = 7,
    IBUF = 8,
    OBUF = 9,
    Other = 10
}

public class XilinxCell
{
    public required string Name { get; set; }

    public string RenamedFrom { get; set; } = string.Empty;

    public string LibraryName { get; set; }
    
    public XilinxView View { get; set; }
    //public List<Pin> ModuleIOs { get; set; }

    public ImmutableList<Pin> Inputs => View.Interfaces.Where(t => t.IoType == Pin.PinType.Input).ToImmutableList();
    
    public ImmutableList<Pin> Outputs => View.Interfaces.Where(t => t.IoType == Pin.PinType.Output).ToImmutableList();

    public CellType CellType => GetCommonCellType();
    
    private CellType GetCommonCellType()
    {
        if (Name.Contains("GND"))
            return CellType.GND;

        if (Name.Contains("LD"))
            return CellType.LD;

        if (Name.StartsWith("FD"))
            return CellType.FD;
        
        if (Name.StartsWith("INV"))
            return CellType.INV;
        
        if (Name.StartsWith("AND"))
            return CellType.AND;
        
        if (Name.StartsWith("OR"))
            return CellType.OR;
        
        if (Name.StartsWith("LDCP"))
            return CellType.LDCP;
        
        if (Name.StartsWith("IBUF"))
            return CellType.IBUF;
        
        if (Name.StartsWith("LDCP"))
            return CellType.LDCP;
        
        if (Name.StartsWith("OBUF"))
            return CellType.OBUF;

        return CellType.Other;
    }
    
    

   
    
    public override string ToString()
    {
        return $"Cell Name= {Name} ----\n [Inputs = {string.Join(", ", Inputs.ToList())}]\n [Outputs = {string.Join(", ", Outputs.ToList())}]";
    }
}