using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls.Shapes;
using Avalonia.Platform.Storage;
using NetGenBox.Core;
using ReactiveUI;
using Path = System.IO.Path;

namespace NetGenBox.UI;

public class XilinxNetGenViewModel : ViewModelBase
{

    private bool _generateTestBench;
    private bool _insertGlobalModule = true;
    private bool _flattenOutputNetlist = true;
    private bool _overwrite = true;
    private bool _dontEscapeNames = true;
    private bool _addModifiedDff = true;
    private bool _addBistPins = true;

    private string _netgenFilePath;
    private string _ngdFilePath;
    
    public bool GenerateTestBench { get => _generateTestBench;
        set => this.RaiseAndSetIfChanged(ref _generateTestBench, value);
    }
    public bool InsertGlobalModule { get => _insertGlobalModule;
        set => this.RaiseAndSetIfChanged(ref _insertGlobalModule, value);
    }
    
    public bool DontEscapeNames
    {
        get => _dontEscapeNames;
        set => this.RaiseAndSetIfChanged(ref _dontEscapeNames, value);
    }
    public bool Overwrite
    {
        get => _overwrite;
        set => this.RaiseAndSetIfChanged(ref _overwrite, value);
    }
    
    public bool FlattenOutputNetlist { get => _flattenOutputNetlist;
        set => this.RaiseAndSetIfChanged(ref _flattenOutputNetlist, value);
    }

    public bool AddModifiedDffs
    {
        get => _addModifiedDff;
        set
        {
            this.RaiseAndSetIfChanged(ref _addModifiedDff, value);
            this.RaisePropertyChanged(nameof(AddBistPins));
        }
    }

    public bool AddBistPins
    {
        get => _addBistPins && AddModifiedDffs;
        set => this.RaiseAndSetIfChanged(ref _addBistPins, value && AddModifiedDffs);
    }
    
    public string NgdFilePath { get => _ngdFilePath ; private set => this.RaiseAndSetIfChanged(ref _ngdFilePath, value); }
    public string NetgenFilePath { get => _netgenFilePath; private set => this.RaiseAndSetIfChanged(ref _netgenFilePath, value); }


    public XilinxNetGenViewModel()
    {
        OpenFileDialog = new Interaction<Unit, IStorageFile?>();
        SaveNetgenFileDialog = new Interaction<Unit, IStorageFile?>();
      
        SelectNgdFileCommand = ReactiveCommand.CreateFromTask(SelectNgdFileTask);
        SelectVerilogFileCommand = ReactiveCommand.CreateFromTask(SelectVerilogFileTask);

        GenerateNetListCommand = ReactiveCommand.CreateFromTask(GenerateNetListTask);
    }

  

    public Interaction<Unit, IStorageFile?> OpenFileDialog { get; }
    public Interaction<Unit, IStorageFile?> SaveNetgenFileDialog { get; }
    
    public ReactiveCommand<Unit, bool> GenerateNetListCommand { get; }
    
    public ICommand SelectNgdFileCommand { get; }
    public ICommand SelectVerilogFileCommand { get; }
    

    private  async Task SelectNgdFileTask()
    {
        var result = await OpenFileDialog.Handle(Unit.Default);
        if (result is not null)
        {
            NgdFilePath = result.Path.LocalPath;
            NetgenFilePath = Path.Combine(Path.GetDirectoryName(NgdFilePath), Path.GetFileNameWithoutExtension(NgdFilePath) + "_netlist.v") ;
        }
    }

    private async Task SelectVerilogFileTask()
    {
        var result = await SaveNetgenFileDialog.Handle(Unit.Default);
        if (result is not null)
        {
            NetgenFilePath = result.Path.LocalPath;
        }
    }

    private async Task<bool> GenerateNetListTask()
    {

        string dff = @"module DFF 
#(parameter tphl = 0, tplh = 0)
(O, I, CE, CLK, SET, RST, NbarT, Si, global_reset); 

input I, CE, CLK, SET, RST, NbarT, Si, global_reset; 
output reg O; 
reg val; 

//tri0 GSR = glbl.GSR;

always @(posedge CLK or posedge SET or posedge RST or posedge global_reset) begin
	if(RST || global_reset)
    val = 1'b0;
	else	if(SET)
		val = 1'b1;
	else	if(NbarT)
		val = Si; 
	else	if(CE)	val = I; 
end
   
always@(posedge val) #tplh O =  val;
always@(negedge val) #tphl O =  val;    
endmodule
                     ";
        
        string? isePath = ServiceLocator.GetConfiguration("isePath");
        if (string.IsNullOrEmpty(isePath))
            return false;
        ISEShell shell = new ISEShell(isePath);
        var result =  await shell.GenerateXilinxFormatNetList(NgdFilePath, NetgenFilePath, Overwrite, GenerateTestBench,
            InsertGlobalModule, DontEscapeNames, FlattenOutputNetlist);
        if (result == false)
            return false;

        if (AddModifiedDffs == false)
            return true;
        
        string xilinxNetList = await File.ReadAllTextAsync(NetgenFilePath);
        var instances = VerilogInstanceParser.ParseInstances(xilinxNetList);
        var firstInstance = instances.First();
        AddBist(firstInstance, "dff_1", "Si", "DFF");
        string prevOutput = firstInstance.Pins["O"];
        for (int i = 1; i < instances.Count ; i++)
        {
            AddBist(instances[i], $"dff_{i + 1}", prevOutput, "DFF");
            prevOutput = instances[i].Pins["O"];
        }

        var lastInstanceWire = instances.Last().Pins["O"];
        string newNetList = Regex.Replace(xilinxNetList, VerilogInstanceParser.InstancePattern, "");
        //module\s+(\w+)\s*\(\s*(.*?)\s*\);([\s\S]*?)(wire[\s\S]*)\s*(endmodule\s*)
        string fullModuleRegex = @"module\s+(\w+)\s*\(\s*(.*?)\s*\);(\s*[output|input]+\s+[\s\S]+?;)(\s*wire\s+[\s\S]+?)(endmodule+)";
        //
        var matches = Regex.Matches(newNetList, fullModuleRegex, RegexOptions.Compiled);
        var match = matches.FirstOrDefault(t => t.Groups[1].Value.ToLower() == Path.GetFileNameWithoutExtension(NgdFilePath).ToLower());
        if (match is not null)
        {
            string moduleName = match.Groups[1].Value;
            string pinsDefinition = match.Groups[2].Value;
            string ioDefinitions = match.Groups[3].Value;
            string moduleBody = match.Groups[4].Value;

            string contents = dff + "\n\n\n\n" + $"module {moduleName} ({pinsDefinition}, Si, So, NbarT, global_reset);\n" +
                              "  input Si, NbarT, global_reset;\n" +
                              $"{ioDefinitions}\n" +
                              "  output So;\n" +
                              moduleBody + "\n" +
                              $"{string.Join("\n  ",instances)}\n" +
                              $"  assign So = {lastInstanceWire};\n"+
                              "endmodule";

            contents = newNetList.Replace(match.Value, "") + "\n" + contents;

            contents = Regex.Replace(contents, @"//[\s\S].*\s*", "");
            await File.WriteAllTextAsync(NetgenFilePath, contents);
        }
        
        
        return true;
    }

    private void AddBist(VerilogInstance instance, string instanceName, string si, string? referenceName = null)
    {
        if (referenceName is not null)
            instance.ReferenceModuleName = referenceName;
        instance.InstanceName = instanceName;
        if (AddBistPins)
        {
            instance.Pins.Add("Si", si);
            instance.Pins.Add("NbarT", "NbarT");
            instance.Pins.Add("global_reset", "rst");
        }
    }
    
    
}