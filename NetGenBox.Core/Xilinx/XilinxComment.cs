namespace NetGenBox.Core.Xilinx;

public class XilinxComment
{
    public required string Name { get; set; }
    public required string Value { get; set; }

    

    public override string ToString()
    {
        return $"Xilinx Comment (${Name}) ---> {Value}";
    }
}