using System.Collections.ObjectModel;

namespace RswareDesign.Models;

public class DriveTreeNode
{
    public string Name { get; set; } = "";
    public string IconKind { get; set; } = "Folder";
    public string? CustomIconPath { get; set; }
    public string NodeType { get; set; } = "";
    public bool IsExpanded { get; set; }
    public bool IsLeaf => Children.Count == 0;
    public bool HasCustomIcon => !string.IsNullOrEmpty(CustomIconPath);
    public ObservableCollection<DriveTreeNode> Children { get; set; } = [];
}
