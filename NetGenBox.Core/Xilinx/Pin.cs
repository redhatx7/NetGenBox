namespace NetGenBox.Core.Xilinx;

public class Pin
{
    public enum PinType
    {
        Input,
        Output,
        InputOutput
    }

    public virtual required string Name { get; set; }
    public required PinType IoType { get; set; }
    

    public override string ToString()
    {
        return $"Pin= {Name}, Pin Type= {IoType}";
    }
}