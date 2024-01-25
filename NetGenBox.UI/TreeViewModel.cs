using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using NetGenBox.Core.Xilinx;

namespace NetGenBox.UI;

public class TreeViewModel
{
    public  IList<XilinxInstance> Instances { get; set; }
    public  IList<XilinxCell> Cells { get; set; }
    public IList<Net> Nets { get; set; }

    public TreeViewModel(IList<XilinxInstance> instances,
        IList<XilinxCell> cells, IList<Net> nets)
    {
        Instances = instances;
        Cells = cells;
        Nets = nets;
        ParseXilinxCells();
        ParseXilinxInstances();
        ParseXilinxNets();
    }

    public TreeViewModel()
    {
        
    }
    
    public FlatTreeDataGridSource<XilinxCell> CellsSource { get; set; }
    public FlatTreeDataGridSource<XilinxInstance> InstancesSource { get; set; }
    
    public FlatTreeDataGridSource<Net> NetsSource { get; set; }

    private void ParseXilinxCells()
    {
        CellsSource = new FlatTreeDataGridSource<XilinxCell>(Cells)
        {
            Columns =
            {
               new TextColumn<XilinxCell,string>("Cell Name", t=> t.Name),
               new TextColumn<XilinxCell,string>("Reference Library", t=> t.LibraryName),
               new TextColumn<XilinxCell,string>("View Name", t => t.View.ViewName),
               new TextColumn<XilinxCell,int>("Input Ports Count", t => t.Inputs.Count),
               new TextColumn<XilinxCell,string>("Inputs", t => string.Join("\n", t.Inputs.Select(t => t.Name).ToList())),
               new TextColumn<XilinxCell,int>("Outputs Ports Count", t => t.Outputs.Count),
               new TextColumn<XilinxCell,string>("Outputs", t => string.Join("\n", t.Outputs.Select(t => t.Name).ToList())),
               
            }
        };
    }
    
    private void ParseXilinxInstances()
    {
        InstancesSource = new FlatTreeDataGridSource<XilinxInstance>(Instances)
        {
            Columns =
            {
                new TextColumn<XilinxInstance,string>("Instance Name", t=> t.InstanceName,GridLength.Star),
                new TextColumn<XilinxInstance,string>("Renamed Name", t=> t.RenamedName, GridLength.Star),
                new TextColumn<XilinxInstance,string>("Reference Cell", t => t.ReferenceCell.Name),
                new TextColumn<XilinxInstance,string>("Belong to Cell", t => t.ParentCell.Name),
            },
        };
    }
    
    private void ParseXilinxNets()
    {
        NetsSource = new FlatTreeDataGridSource<Net>(Nets)
        {
            Columns =
            {
                new TemplateColumn<Net>("Net Name", "NetName", null, GridLength.Auto),
                new TemplateColumn<Net>("Ports", "Ports", null, GridLength.Star)

              
            },
        };
    }
}