using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using AvaloniaEdit.Utils;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using NetGenBox.Core;
using TextMateSharp.Grammars;
using Observable = System.Reactive.Linq.Observable;

namespace NetGenBox.UI;

public partial class CustomModules : Window
{

    private CustomModuleViewModel _customModuleViewModel;
    public required string BasePath { get; set; }
    
    private readonly TextMate.Installation _textMateInstallation;
    private readonly VerilogRegistryOption _verilogOption;
    private string _contents;
    public CustomModules()
    {
        InitializeComponent();
        _customModuleViewModel = new CustomModuleViewModel();
        DataContext = _customModuleViewModel;
        _verilogOption = new VerilogRegistryOption(ThemeName.Light);
        _textMateInstallation = OriginalModuleEditor.InstallTextMate(_verilogOption);
        _textMateInstallation.SetGrammar("source.verilog");
        //_textEditor.TextArea.Background = this.Background;
        _textMateInstallation = CustomModuleEditor.InstallTextMate(_verilogOption);
        _textMateInstallation.SetGrammar("source.verilog");
      
        OriginalModuleEditor.Background = CustomModuleEditor.Background = Brushes.Transparent;
    }

    

    private void Control_OnLoaded(object? sender, RoutedEventArgs e)
    {
        string searchPath = BasePath +  "/verilog/src/unisims";
        if (!Directory.Exists(searchPath))
            return;
        var files = Directory.GetFiles(searchPath, "*.v");
        var modules = new List<XilinxModuleModel>();
        foreach (var file in files)
        {

            var xilinxModule = new XilinxModuleModel()
            {
                ModuleName = Path.GetFileName(file).Replace(".v", ""),
                ModulePath = file
            };
            modules.Add(xilinxModule);
        }

        this._customModuleViewModel.SetDataSource(modules);

    }

    private async void SelectingItemsControl_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var selectedItem = _customModuleViewModel.SelectedItem;
        OriginalModuleEditor.Text = await selectedItem.ReadModuleTextAsync();
        var pins = VerilogModuleParser.ParseModule(OriginalModuleEditor.Text);
        if (pins is not null)
        {
            _customModuleViewModel.OriginalModulePins = new ObservableCollection<string>(pins.IoDefinitions);
        }
    }


    private void ButtonEditCustomModule_OnClick(object? sender, RoutedEventArgs e)
    {
        CustomModuleEditor.Text = $"module {TextBoxCustomModuleName.Text} ( );\n\n\n\n\n\nendmodule;";
        CustomModuleEditor.Focus();
        CustomModuleEditor.CaretOffset = CustomModuleEditor.Text.IndexOf("(", StringComparison.Ordinal) + 1;
    }

    private void CustomModuleEditor_OnTextChanged(object? sender, EventArgs e)
    {
        var pins = VerilogModuleParser.ParseModule(CustomModuleEditor.Text);
        if (pins is not null)
        {
            _customModuleViewModel.CustomModulePins = new ObservableCollection<string>(pins.IoDefinitions);
            var list = pins.IoDefinitions.Select(t => new IoDefinition()
            {
                CustomPin = t,
                OriginalPin = string.Empty
            }).ToList();
            _customModuleViewModel.PinDefinitions = new ObservableCollection<IoDefinition>(list);
        }
    }


    private async void SaveMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        
        var topLevel = TopLevel.GetTopLevel(this);

        // Start async operation to open the dialog.
        var selectedFile = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
        {
            Title = "Open Verilog Files",
            FileTypeChoices = new FilePickerFileType[]
            {
                new FilePickerFileType("Verilog File | (*.v)")
                {
                    Patterns = new []{"*.v"}
                }
            },
            DefaultExtension = "*.v"
        });

        if (selectedFile is null)
            return;
        
        var code = CustomModuleEditor.Text;
        string pattern = @"module\s+[\w\s,]*?\([\w\s,]*?\)\s*;[\s\S]*?endmodule;";
        
        var match = Regex.Match(code, pattern);
        
        if (match.Success)
        {
            var module = VerilogModuleParser.ParseModule(code);
            if (module is null)
                return;
            var moduleName = module.ModuleName;
            var originalModuleName = _customModuleViewModel.SelectedItem.ModuleName;
            var ports = string.Join("\n",_customModuleViewModel.PinDefinitions
                .Select(t => $"// MainPin={t.OriginalPin} -> {t.CustomPin}").ToList());
            var exportCode = $"// CustomModuleName={moduleName}\n" +
                             $"// OriginalModuleName={originalModuleName}\n"
                             + ports + "\n"
                             + match.Value + "\n";
            await File.WriteAllTextAsync(selectedFile.Path.AbsolutePath,exportCode, Encoding.ASCII);
            await MessageBoxManager.GetMessageBoxStandard("Saved", $"Module {moduleName} saved\nPath: {selectedFile.Path.AbsolutePath}", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Success)
                .ShowAsPopupAsync(this);
        }
        else
        {
            Console.WriteLine("No match found.");
        }
    }
}