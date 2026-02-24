using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.Messaging;
using RswareDesign.Models;
using RswareDesign.ViewModels;

namespace RswareDesign.Views;

public partial class DriveTreeView : UserControl
{
    public DriveTreeView()
    {
        InitializeComponent();
    }

    private void OnTreeNodeSelected(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is DriveTreeNode node)
        {
            WeakReferenceMessenger.Default.Send(
                new TreeNodeSelectedMessage(node.Name, node.NodeType));
        }
    }
}
