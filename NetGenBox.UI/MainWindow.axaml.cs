using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Microsoft.Extensions.Configuration;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia.Models;
using NetGenBox.Core;

namespace NetGenBox.UI;

public partial class MainWindow : Window
{
    public List<XilinxItem> XilinxItems { get; set; }

    public List<string> VerilogFiles { get; set; }

    public IReadOnlyList<IStorageFile> _selectedFiles;

    private Dictionary<string, string> _fileContents= new();

    private string _selectedDirectory = "";
    public IConfiguration Configuration { get; set; }

    private NetGenBox.Core.NetListExtractor _netgenResult;

    public List<string> ModuleNames { get; set; }

    private string _edifFileName;
    

    public MainWindow()
    {
        InitializeComponent();
    }

    private void CustomModuleMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        VerilogEdit verilogEdit = new VerilogEdit()
        {
            WorkDir = _selectedDirectory
        };
        verilogEdit.ShowDialog(this);
       
    }

    private async void OpenFilesMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        // Start async operation to open the dialog.
         _selectedFiles = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Verilog Files",
            AllowMultiple = true,
            FileTypeFilter = new FilePickerFileType[]
            {
                new FilePickerFileType("Verilog File | (*.v)")
                {
                    Patterns = new []{"*.v"}
                }
            },
        });

        this.VerilogTopInstanceCombo.ItemsSource = _selectedFiles.Select(t => t.Name).ToList();
        this.VerilogTopInstanceCombo.SelectedIndex = 0;
        this.SelectedFilesListBox.ItemsSource = _selectedFiles.Select(t => t.Name).ToList();
        if (_selectedFiles.Count >= 1)
        {
            this._selectedDirectory = Path.GetDirectoryName(_selectedFiles.First().Path.AbsolutePath);
            ModuleNames = new List<string>();
            ModuleNames.Clear();
            _fileContents.Clear();

            foreach (var storageFile in _selectedFiles)
            {
                using var sr = new StreamReader(storageFile.Path.AbsolutePath, Encoding.UTF8);
                var content = await sr.ReadToEndAsync();
                var modules = ExtractModuleNames(content);
                ModuleNames.AddRange(modules);
                _fileContents.Add(storageFile.Name.Replace(".v",""), content);
            }

            this.VerilogTopInstanceCombo.ItemsSource = ModuleNames;
            this.VerilogTopInstanceCombo.SelectedIndex = 0;
        }
      
    }

    private async void Control_OnLoaded(object? sender, RoutedEventArgs e)
    {
        
        var fs =AssetLoader.Open(new Uri("avares://NetGenBox.UI/ise.json"));
        //using var fs = new FileStream("ise.json", FileMode.Open);
        var xilinxItems = await System.Text.Json.JsonSerializer.DeserializeAsync<List<XilinxItem>>(fs);
        this.XilinxItems = xilinxItems;

        this.IseFamilyCombo.ItemsSource = xilinxItems.Select(t => t.Family).ToList();
        IseFamilyCombo.SelectedItem = xilinxItems.First(t => t.Family == "CoolRunner2 CPLDS").Family;
    }

    private void IseDeviceCombo_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
     

    }

    private void IseFamilyCombo_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        string selected = this.IseFamilyCombo.SelectionBoxItem?.ToString() ?? "";
        var xilinxItem = XilinxItems.FirstOrDefault(t => t.Family == selected);
        if (xilinxItem is not null)
        {
            this.IseDeviceCombo.ItemsSource = xilinxItem.Devices.Select(t => t.DeviceName).ToImmutableList();
            this.IseDeviceCombo.SelectedIndex = 0;
        }
    }

    private async void SettingMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        Settings settings = new Settings();
        await settings.ShowDialog(this);
    }

    private async void ExitMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        var box = MessageBoxManager
            .GetMessageBoxStandard("Exit", "Are you sure you want to exit?",
                ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Warning);

        var result = await box.ShowWindowDialogAsync(this);
        if (result == ButtonResult.Yes)
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopApp)
            {
                desktopApp.Shutdown();
            }
        }
    }

    private async void AboutMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
          var box = MessageBoxManager.GetMessageBoxCustom(
            new MessageBoxCustomParams
            {
                ButtonDefinitions = new List<ButtonDefinition>
                {
                   new ButtonDefinition()
                   {
                       Name = "Close",
                       IsCancel = true,
                   }
                },
                ContentTitle = "About",
                ContentMessage = "NetGenBox - Amirreza Azadpour\n" +
                                 "This is an experimental NetList generator for Xilinx ISE\n" +
                                 "It may not work as expected and still is in testing",
                Icon = MsBox.Avalonia.Enums.Icon.Info,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false,
                MaxWidth = 500,
                MaxHeight = 800,
                SizeToContent = SizeToContent.WidthAndHeight,
                ShowInCenter = true,
                Topmost = false,
                HyperLinkParams = new HyperLinkParams
                {
                    Text = "GitHub Repository: http://github.com/redhatx7/NetGenBox",
                    Action = new Action(() =>
                    {
                        var url = "https://github.com/redhatx7/NetGenBox";
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });

                            return;
                        }

                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                        {
                            Process.Start("xdg-open", url);
                            return;
                        }

                        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                            throw new Exception("invalid url: " + url);
                        Process.Start("open", url);
                        return;
                    })
                }
            });
        await box.ShowWindowDialogAsync(this);
    }

    private async void RunButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (VerilogTopInstanceCombo.SelectedItem is null)
        {
            var box = MessageBoxManager
                .GetMessageBoxStandard("Select Top Module", "First, Select Top Module in order to begin synthesize",
                    ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Success);
            await box.ShowWindowDialogAsync(this);
            return;
        }

        string topModule = VerilogTopInstanceCombo.SelectedItem.ToString()!;
        ISEShell iseShell = new ISEShell(Configuration["isePath"]);
        IseProgressBar.IsVisible = true;
        IseProgressBar.IsIndeterminate = true;
        IseProgressBar.Value = 0;
        StdOutTextBox.Clear();
        StdErrTextBox.Clear();
        StdOutTextBox.Text += $"{DateTime.Now}\n";
        StdOutTextBox.Text += "\n\nStandard Output:\n";
        StdErrTextBox.Text += "Standard Error\n\n";
        var result = await iseShell.CreateProject(topModule,_selectedDirectory, _selectedFiles.Select(t => t.Path.AbsolutePath).ToList(),
             async stdOut =>
             {
                  Dispatcher.UIThread.Invoke(() =>
                 {
                     StdOutTextBox.Text += stdOut;
                     StdOutTextBox.CaretIndex = StdOutTextBox.Text.Length;
                 });
             },
             stdErr =>
            {
                 Dispatcher.UIThread.Invoke(() =>
                {
                    StdErrTextBox.Text += stdErr;
                    StdErrTextBox.CaretIndex = StdErrTextBox.Text.Length;
                });
            } );
        IseProgressBar.IsIndeterminate = false;
        IseProgressBar.Value = 100;
        
       

       
        if (result == 0)
        {
            var box = MessageBoxManager
                .GetMessageBoxStandard("Result", "Synthesize completed successfully",
                    ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Success);
            await box.ShowWindowDialogAsync(this);
        }

    }

    private async void GenerateEdifButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (VerilogTopInstanceCombo.SelectedItem is null)
        {
            var box = MessageBoxManager
                .GetMessageBoxStandard("Select Top Module", "First, Select Top Module in order to begin synthesize",
                    ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Success);
            await box.ShowWindowDialogAsync(this);
            return;
        }

        string topModule = VerilogTopInstanceCombo.SelectedItem.ToString()!;
        ISEShell iseShell = new ISEShell(Configuration["isePath"]);
        IseProgressBar.IsVisible = true;
        IseProgressBar.IsIndeterminate = true;
        IseProgressBar.Value = 0;
        StdOutTextBox.Clear();
        StdErrTextBox.Clear();
        StdOutTextBox.Text += $"{DateTime.Now}\n";
        StdOutTextBox.Text += "\n\nStandard Output:\n";
        StdErrTextBox.Text += "Standard Error\n\n";
        var result = await iseShell.WriteEdif(topModule, topModule+"_edif",_selectedDirectory,true,
            async stdOut =>
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    StdOutTextBox.Text += stdOut;
                    StdOutTextBox.CaretIndex = StdOutTextBox.Text.Length;
                });
            },
            stdErr =>
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    StdErrTextBox.Text += stdErr;
                    StdErrTextBox.CaretIndex = StdErrTextBox.Text.Length;
                });
            } );
        IseProgressBar.IsIndeterminate = false;
        IseProgressBar.Value = 100;
        
       

       
        if (result == 0)
        {
            var box = MessageBoxManager
                .GetMessageBoxStandard("Result", "EDIF generation completed successfully",
                    ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Success);
            await box.ShowWindowDialogAsync(this);
            _edifFileName = _selectedDirectory + "/" + topModule + "_edif.ndf";
        }
    }

    //Process "Synthesize - XST" completed successfully
    private async void TreeViewMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        /*
        if (string.IsNullOrEmpty(_selectedDirectory))
        {
            var box = MessageBoxManager
                .GetMessageBoxStandard("No File Selected", "Open Verilog Files from Menu -> Open Files",
                    ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Warning);
            await box.ShowWindowDialogAsync(this);
            return;
        }
        */
        if (string.IsNullOrEmpty(_edifFileName))
        {
            var topLevel = TopLevel.GetTopLevel(this);

            // Start async operation to open the dialog.
            var edifFiles = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open EDIF Files",
                AllowMultiple = false,
                FileTypeFilter = new FilePickerFileType[]
                {
                    new FilePickerFileType("EDIF File")
                    {
                        Patterns = new []{"*.edif", "*.ndf"}
                    }
                }
            });
            if (edifFiles.Count == 0)
            {
                return;
            }

            _edifFileName = edifFiles.First().Path.AbsolutePath;

        }
        
        var edifFile = await File.ReadAllTextAsync(_edifFileName);
        var lexer = new Parser.Lexer(edifFile);
        var parser = new Parser.Parser(lexer);
        var list = parser.ParseProgram();
        var netGenBox = new NetListExtractor(list);
        netGenBox.Parse();
        TreeView treeView = new TreeView()
        {
            DataContext = new TreeViewModel(netGenBox.XilinxInstances, netGenBox.XilinxCells, netGenBox.Nets)
        };
        await treeView.ShowDialog(this);
    }

    

    private void SelectedFilesListBox_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (SelectedFilesListBox.SelectedItem is not null)
        {
            var filePath = _selectedFiles.FirstOrDefault(t => t.Name == SelectedFilesListBox.SelectedItem.ToString());
            if (filePath is not null)
            {
                var codeViewer = new CodeViewer(filePath.Path)
                {
                    Title = filePath.Path.AbsolutePath
                };
                codeViewer.Show(this);
            }
        }
    }

    private List<string> ExtractModuleNames(string content)
    {
        Regex regex = new Regex(@"module\s+(\w+)\s*\(", RegexOptions.Compiled);
        var matches = regex.Matches(content);
        List<string> matchedStrings = new List<string>();
        foreach (Match match in matches)
        {
            string moduleName = match.Groups.Values.Last().Value;
            matchedStrings.Add(moduleName);
        }

        return matchedStrings;
    }

    private async void GenerateNetlistMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (File.Exists(_edifFileName) == false)
        {
            var topLevel = TopLevel.GetTopLevel(this);

            var selectedFiles = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open NDF File",
                AllowMultiple = false,
                FileTypeFilter = new FilePickerFileType[]
                {
                    new FilePickerFileType("NDF File | (*.ndf)")
                    {
                        Patterns = new []{"*.ndf"}
                    }
                }
            });
            if (selectedFiles.Count == 1)
            {
                _edifFileName = selectedFiles.First().Path.AbsolutePath;
            }
            else
            {
                return;
            }
        }
        var edifFileContents = await File.ReadAllTextAsync(_edifFileName);
        var lexer = new Parser.Lexer(edifFileContents);
        var parser = new Parser.Parser(lexer);
        var list = parser.ParseProgram();
        var netGenBox = new NetListExtractor(list);
        netGenBox.Parse();
        var verilogGenerator = new VerilogGenerator(netGenBox.Nets);
        var verilogFile = verilogGenerator.GenerateGateLevel();
        var selectedModule = _fileContents[VerilogTopInstanceCombo?.SelectedValue as string];
        var moduleParser = new VerilogModuleParser(selectedModule);
        var module = moduleParser.ParseModule();
        var codeViewer = new CodeViewer(module.ExportString(verilogFile))
        {
            Title = "NetList"
        };
        codeViewer.Show(this);
    }
}