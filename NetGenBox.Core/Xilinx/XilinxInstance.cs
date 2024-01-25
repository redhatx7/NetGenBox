namespace NetGenBox.Core.Xilinx;

public class XilinxInstance
{

    public required XilinxCell ReferenceCell { get; set; }
    public required string InstanceName { get; set; }

    public required XilinxCell ParentCell { get; set; }

    public string RenamedName { get; set; }

    
    
}