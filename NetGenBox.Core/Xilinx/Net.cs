namespace NetGenBox.Core.Xilinx;


public class PortReference
{
    public required string PortName { get; set; }
    public XilinxInstance? Instance { get; set; }

    public override string ToString()
    {
        return $"{PortName} ------> {Instance?.InstanceName}";
    }
}

public class PortReferenceBus : PortReference
{
    public int BusIndex { get; set; }
}

public class Net
{
    public required string NetName { get; set; }

    public string? RenamedNetName { get; set; }

    public string PreferredName => string.IsNullOrEmpty(RenamedNetName) ? NetName : RenamedNetName;
    public required XilinxCell BelongsToCell { get; set; }
    //public Dictionary<(string portName, string instanceName)> portReferences { get; set; }
    public List<PortReference> PortReferences { get; set; } = new();

}