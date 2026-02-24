using System.Collections.ObjectModel;

namespace RswareDesign.Models;

public class DriveTreeNode
{
    public string Name { get; set; } = "";
    public string IconKind { get; set; } = "Folder";
    public string NodeType { get; set; } = "";
    public bool IsExpanded { get; set; }
    public bool IsLeaf => Children.Count == 0;
    public ObservableCollection<DriveTreeNode> Children { get; set; } = [];
}
