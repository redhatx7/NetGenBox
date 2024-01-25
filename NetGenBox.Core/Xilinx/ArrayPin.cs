namespace NetGenBox.Core.Xilinx;

public class ArrayPin : Pin
{
    public required int StartIndex { get; set; }

    public required int StopIndex { get; set; }

    public int BusLength => StopIndex - StartIndex + 1;
    
    public string? RenamedFrom { get; set; }

    public string this[uint index] => $"{Name}_{index}";
}