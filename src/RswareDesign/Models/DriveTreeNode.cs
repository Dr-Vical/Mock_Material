using System.Collections.ObjectModel;

namespace RswareDesign.Models;

public class DriveTreeNode
{
    public string Name { get; set; } = "";
    public string IconKind { get; set; } = "Folder";
    public bool IsExpanded { get; set; }
    public ObservableCollection<DriveTreeNode> Children { get; set; } = [];
}
