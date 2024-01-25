using System.Drawing;

namespace NetGenBox.Core.Xilinx;

public class XilinxView
{
    public required string ViewName { get; set; }

    public required XilinxCell ParentCell { get; set; }

    public List<XilinxInstance> Contents { get; set; } = new();
    public List<Pin> Interfaces { get; set; } = new();

}