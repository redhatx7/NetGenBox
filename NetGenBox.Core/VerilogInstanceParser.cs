using System.Text.RegularExpressions;

namespace NetGenBox.Core;

public class VerilogInstanceParser
{
    
    public const string InstancePattern = @"(X_FF+)\s+([a-zA-Z0-9_]+)\s*\(\s*((?:\.[a-zA-Z0-9$_]+\([a-zA-Z0-9$_\[\]]+\)\s*,\s*)*(?:\.[a-zA-Z0-9_]+\([a-zA-Z0-9_\[\]]+\)\s*))\);";
    public const string PinsPattern = @"\.([a-zA-Z0-9_]+)\(([a-zA-Z0-9_\[\]]+)\)";
    
    public static VerilogInstance? ParseInstance(string str)
    {
        var  match = Regex.Match(str, InstancePattern);
        if (match.Success)
        {
            string moduleReference = match.Groups[1].Value;
            string instanceName = match.Groups[2].Value;
            string pins = match.Groups[3].Value;
            var pinMatches = Regex.Matches(pins, PinsPattern);

            var pinsDictionary = new Dictionary<string, string>();
            foreach (Match pinMatch in pinMatches)
            {
                string pinName = pinMatch.Groups[1].Value;
                string wireName = pinMatch.Groups[2].Value;
                pinsDictionary.Add(pinName, wireName);
            }

            var verilogInstance = new VerilogInstance()
            {
                InstanceName = instanceName,
                ReferenceModuleName = moduleReference,
                Pins = pinsDictionary
            };
            return verilogInstance;
        }

        return null;
    }

    public static List<VerilogInstance> ParseInstances(string str)
    {
        var  matches = Regex.Matches(str, InstancePattern);
        List<VerilogInstance> instances = new List<VerilogInstance>();
        foreach (Match match in matches)
        {
            string moduleReference = match.Groups[1].Value;
            string instanceName = match.Groups[2].Value;
            string pins = match.Groups[3].Value;
            var pinMatches = Regex.Matches(pins, PinsPattern);

            var pinsDictionary = new Dictionary<string, string>();
            foreach (Match pinMatch in pinMatches)
            {
                string pinName = pinMatch.Groups[1].Value;
                string wireName = pinMatch.Groups[2].Value;
                pinsDictionary.Add(pinName, wireName);
            }

            var verilogInstance = new VerilogInstance()
            {
                InstanceName = instanceName,
                ReferenceModuleName = moduleReference,
                Pins = pinsDictionary
            };
            instances.Add(verilogInstance);
        }

        return instances;
    }
}