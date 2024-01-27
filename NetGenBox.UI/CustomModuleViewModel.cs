using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaEdit.Utils;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ReactiveUI;
using Splat;

namespace NetGenBox.UI;




public class IoDefinition
{
    public string OriginalPin { get; set; }
    public string CustomPin { get; set; }
}
public class CustomModuleViewModel : INotifyPropertyChanged
{
    private IList<XilinxModuleModel> _xilinxModules;
    private ObservableCollection<XilinxModuleModel> _filteredItems;
    private string _filterText;
    private XilinxModuleModel _selectedItem;

    private ObservableCollection<IoDefinition> _pinDefinitions;
    private ObservableCollection<string> _originalModulePins;
    private ObservableCollection<string> _customModulePins;
    private readonly Window _window;

    public void SetDataSource(IList<XilinxModuleModel> source)
    {
        _xilinxModules = source; 
        FilterModules();
    }
    

    public CustomModuleViewModel()
    {
        //SaveCustomModuleCommand = ReactiveCommand.Create(SaveMenuItem);   
    }

    
    
    public ReactiveCommand<Unit,Unit> SaveCustomModuleCommand { get; }
    
    public ObservableCollection<string> OriginalModulePins
    {
        get => _originalModulePins;
        set
        {
            _originalModulePins = value;
            _originalModulePins.AddRange(new []
            {
                "1'b0",
                "1'b1",
                "1'bz",
                "1'bx"
            });
            OnPropertyChanged(nameof(OriginalModulePins));
        }
    }
    
    public ObservableCollection<string> CustomModulePins
    {
        get => _customModulePins;
        set
        {
            _customModulePins = value;
            OnPropertyChanged(nameof(CustomModulePins));
        }
    }

    public ObservableCollection<IoDefinition> PinDefinitions
    {
        get => _pinDefinitions;
        set
        {
            _pinDefinitions = value;
            OnPropertyChanged(nameof(PinDefinitions));
        }
    }
    public ObservableCollection<XilinxModuleModel> XilinxModules
    {
        get => _filteredItems;
        set
        {
            _filteredItems = value;
            OnPropertyChanged(nameof(XilinxModules));
        }
    }
    public string FilterText
    {
        get => _filterText;
        set
        {
            if (_filterText != value)
            {
                _filterText = value;
                OnPropertyChanged(nameof(FilterText));
                FilterModules();
            }
        }
    }

    public XilinxModuleModel SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (_selectedItem != value)
            {
                _selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }
    }

    private void FilterModules()
    {
        if (string.IsNullOrEmpty(_filterText))
            XilinxModules = new ObservableCollection<XilinxModuleModel>(_xilinxModules);
        else
        {
            var result = _xilinxModules.Where(t => t.ModuleName.ToLower().StartsWith(_filterText.ToLower()))
                .ToList();
            XilinxModules = new ObservableCollection<XilinxModuleModel>(result);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
 
    
   
    
}