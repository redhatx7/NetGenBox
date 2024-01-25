using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.TextMate;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using TextMateSharp.Grammars;
using Color = System.Drawing.Color;

namespace NetGenBox.UI;

public partial class CodeViewer : Window
{
    private readonly Uri? _filePath;
    private readonly TextMate.Installation _textMateInstallation;
    private readonly TextEditor? _textEditor;
    private readonly VerilogRegistryOption _verilogOption;
    private string _contents;

    public CodeViewer(Uri filePath)
    {
        InitializeComponent();
        _filePath = filePath;
        _verilogOption = new VerilogRegistryOption(ThemeName.Light);
        _textEditor = this.FindControl<TextEditor>("Editor");
        _textMateInstallation = _textEditor.InstallTextMate(_verilogOption);
        _textMateInstallation.SetGrammar("source.verilog");
        //_textEditor.TextArea.Background = this.Background;
        
        _textEditor.TextArea.Caret.PositionChanged +=  CaretOnPositionChanged;
        _textEditor.Background = Brushes.Transparent;
    }

    public CodeViewer(string contents)
    {
        InitializeComponent();
        _contents = contents;
        _verilogOption = new VerilogRegistryOption(ThemeName.Light);
        _textEditor = this.FindControl<TextEditor>("Editor");
        _textMateInstallation = _textEditor.InstallTextMate(_verilogOption);
        _textMateInstallation.SetGrammar("source.verilog");
        //_textEditor.TextArea.Background = this.Background;
        _textEditor.TextArea.Caret.PositionChanged +=  CaretOnPositionChanged;
        _textEditor.Background = Brushes.Transparent;
    }

 

    private void CaretOnPositionChanged(object? sender, EventArgs e)
    {
        StatusText.Text = string.Format("Line {0} Column {1}",
            _textEditor.TextArea.Caret.Line,
            _textEditor.TextArea.Caret.Column);
    }


    private async void Control_OnLoaded(object? sender, RoutedEventArgs e)
    {
        //var t = _textEditor.SyntaxHighlighting;
        
        if(_filePath is not null)
            _contents = await File.ReadAllTextAsync(_filePath.AbsolutePath, Encoding.UTF8);
        Editor.Text = _contents;
    }

    private async void SaveMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        var box = MessageBoxManager
            .GetMessageBoxStandard("Save", "Are you sure you want to modify original verilog file?",
                ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Success);
        if (await box.ShowWindowDialogAsync(this) == ButtonResult.Yes)
        {
            await File.WriteAllTextAsync(_filePath.AbsolutePath, Editor.Text);
        }
    }

    private void CloseMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        this.Close();
    }
}