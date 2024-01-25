using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using AvaloniaEdit;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Folding;
using AvaloniaEdit.Indentation.CSharp;
using AvaloniaEdit.Rendering;
using AvaloniaEdit.TextMate;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using TextMateSharp.Grammars;
using TextMateSharp.Registry;

namespace NetGenBox.UI;
using Pair = KeyValuePair<int, Control>;
public partial class VerilogEdit : Window
{
    private readonly TextMate.Installation _textMateInstallation;
    private readonly TextEditor? _textEditor;
    private readonly VerilogRegistryOption _verilogOption;
    public string WorkDir { get; set; }

    public VerilogEdit()
        {

            InitializeComponent();
            //IRegistryOptions options = new RegistryOptions(ThemeName.Monokai);
             _verilogOption = new VerilogRegistryOption(ThemeName.Light);
            _textEditor = this.FindControl<TextEditor>("Editor");
            _textMateInstallation = _textEditor.InstallTextMate(_verilogOption);
            _textMateInstallation.SetGrammar("source.verilog");
            //_textEditor.TextArea.Background = this.Background;
           // ThemesListComboBox.ItemsSource = Enum.GetValues<ThemeName>().ToList();
            this.Editor.TextArea.Caret.PositionChanged +=  CaretOnPositionChanged;
            _textEditor.Background = Brushes.Transparent;
            //_textEditor.TextArea.Background = Brushes.Aqua;
            _textEditor.Document.TextChanged += TextEditorOnTextChanged;

        }

    private void TextEditorOnTextChanged(object? sender, EventArgs e)
    {
        if (!Editor.Text.Contains($"// {ReplacingMoudleNameTextBox.Text}"))
        {
            Editor.Document.Text = $"// {ReplacingMoudleNameTextBox.Text}\n" +
                          Editor.Text;
        }
    }

    private void CaretOnPositionChanged(object? sender, EventArgs e)
    {
        StatusText.Text = string.Format("Line {0} Column {1}",
            _textEditor.TextArea.Caret.Line,
            _textEditor.TextArea.Caret.Column);
    }


    protected override void OnClosed(EventArgs e)
        {
            //base.OnClosed(e);

            _textMateInstallation.Dispose();
        }


        private void ThemesListComboBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
           
        }

        private void TextBox_OnTextChanged(object? sender, TextChangedEventArgs e)
        {
        
        }

        private async void SaveMenuItem_OnClick(object? sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);

            if (!string.IsNullOrEmpty(WorkDir) && !Directory.Exists(WorkDir + "/netgenbox"))
            {
                try
                {
                    Directory.CreateDirectory(WorkDir + "/netgenbox");
                }
                catch
                {
                    
                }
            }
            // Start async operation to open the dialog.
            var selectedFile = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
            {
                Title = "Open Verilog Files",
                ShowOverwritePrompt = true,
                SuggestedFileName = ReplacingMoudleNameTextBox.Text + ".v",
                DefaultExtension = "*.v",
                FileTypeChoices = new FilePickerFileType[]
                {
                    new FilePickerFileType("Verilog File")
                    {
                        Patterns = new []{"*.v"}
                    }
                },
                SuggestedStartLocation = await StorageProvider.TryGetFolderFromPathAsync(new Uri(WorkDir + "/netgenbox"))
            });
            if (selectedFile is not null)
            {
                await File.WriteAllTextAsync(selectedFile.Path.AbsolutePath, Editor.Text);
                var box = MessageBoxManager
                    .GetMessageBoxStandard("Save", "File Saved Successfully\nPath: " + selectedFile.Path.AbsolutePath,
                        ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Success);

                await box.ShowWindowDialogAsync(this);
            }

        }
}