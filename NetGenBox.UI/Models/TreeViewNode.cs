using System.Collections.ObjectModel;

namespace NetGenBox.UI;

public class TreeViewNode
{
    public string Title { get; set; } = null!;
    public ObservableCollection<TreeViewNode>? SubNodes { get; set; }

    public TreeViewNode(string title)
    {
        Title = title;
    }
    
    public TreeViewNode(string title, ObservableCollection<TreeViewNode> subNodes)
    {
        Title = title;
        SubNodes = subNodes;
    }
}
