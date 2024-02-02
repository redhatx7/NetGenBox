using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using AvaloniaEdit.Utils;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ReactiveUI;

namespace NetGenBox.UI.Views;

public partial class XilinxNetGen : ReactiveWindow<XilinxNetGenViewModel>
{
    public XilinxNetGen()
    {
        this.DataContext = new XilinxNetGenViewModel();
        InitializeComponent();
        
#if DEBUG
        this.AttachDevTools();
#endif
        
        this.WhenActivated(action => action(ViewModel!.OpenFileDialog.RegisterHandler(ShowOpenFileDialogAsync)));
        this.WhenActivated(action =>
            action(ViewModel!.SaveNetgenFileDialog.RegisterHandler(ShowSaveNetGenDialogAsync)));
        this.WhenActivated(t => t(ViewModel!.GenerateNetListCommand.Subscribe(GenerateNetList)));

    }
    


    private async Task ShowOpenFileDialogAsync(InteractionContext<Unit, IStorageFile?> interaction)
    {
        var top = GetTopLevel(this);
        if (top is not null)
        {
            var file = await top.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select File",
                AllowMultiple = false,
                FileTypeFilter = new FilePickerFileType[]
                {
                    new FilePickerFileType($"NGD File | (*.ngd)")
                    {
                        Patterns = new []{"*.ngd"}
                    }
                },
            });
            if (file.Count != 1)
            {
                interaction.SetOutput(null);
                return;
            }

            var selectedFile = file.First();
            interaction.SetOutput(selectedFile);
        }
    }
    
    private async Task ShowSaveNetGenDialogAsync(InteractionContext<Unit, IStorageFile?> interaction)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        if (topLevel is null)
        {
            interaction.SetOutput(null);
            return;
        }
        
        var selectedFile = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
        {
            Title = "Save Verilog Netgen file",
            FileTypeChoices = new FilePickerFileType[]
            {
                new FilePickerFileType("Verilog File | (*.v)")
                {
                    Patterns = new []{"*.v"}
                }
            },
            DefaultExtension = "*.v"
        });
        
        
        interaction.SetOutput(selectedFile);
        
    }

    private async  void GenerateNetList(bool result)
    {
        if (result)
        {
            await MessageBoxManager.GetMessageBoxStandard("Success", "Verilog Netlist generated successfully", ButtonEnum.Ok,
                MsBox.Avalonia.Enums.Icon.Success).ShowAsPopupAsync(this);
        }
        else
        {
            await MessageBoxManager.GetMessageBoxStandard("Error", "An error occured during netlist generation process", ButtonEnum.Ok,
                MsBox.Avalonia.Enums.Icon.Error).ShowAsPopupAsync(this);
        }
    }
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
}