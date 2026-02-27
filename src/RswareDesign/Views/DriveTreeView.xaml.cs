using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using CommunityToolkit.Mvvm.Messaging;
using RswareDesign.Models;
using RswareDesign.ViewModels;

namespace RswareDesign.Views;

public partial class DriveTreeView : UserControl
{
    public DriveTreeView()
    {
        InitializeComponent();

        WeakReferenceMessenger.Default.Register<FavoriteAnimationMessage>(this, (_, msg) =>
        {
            ShowFavoriteAnimation(msg.IsAdded);
        });
    }

    private void OnTreeNodeSelected(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is DriveTreeNode node)
        {
            WeakReferenceMessenger.Default.Send(
                new TreeNodeSelectedMessage(node.Name, node.NodeType));
        }
    }

    private void OnOscilloscopeClick(object sender, RoutedEventArgs e)
    {
        WeakReferenceMessenger.Default.Send(new ToggleMonitorSectionMessage("Oscilloscope"));
    }

    private void OnControlPanelClick(object sender, RoutedEventArgs e)
    {
        WeakReferenceMessenger.Default.Send(new ToggleMonitorSectionMessage("ControlPanel"));
    }

    private void ShowFavoriteAnimation(bool isAdded)
    {
        // Find the Favorites TreeViewItem
        var favoritesItem = FindTreeViewItemByNodeType(driveTree, "Favorites");
        if (favoritesItem == null) return;

        // Get position relative to the animation canvas
        var transform = favoritesItem.TransformToAncestor(animationCanvas);
        var itemPos = transform.Transform(new Point(0, 0));

        // Create floating text
        var text = new TextBlock
        {
            Text = isAdded ? "+1" : "-1",
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(
                ((SolidColorBrush)Application.Current.FindResource("WarningBrush")).Color),
            RenderTransform = new TranslateTransform(),
            Opacity = 1.0,
        };

        // Position: right side of Favorites item, slightly offset
        Canvas.SetLeft(text, itemPos.X + favoritesItem.ActualWidth - 20);
        Canvas.SetTop(text, itemPos.Y + 2);
        animationCanvas.Children.Add(text);

        var translateTransform = (TranslateTransform)text.RenderTransform;

        // Direction: +1 floats up, -1 floats down
        double targetY = isAdded ? -25 : 25;

        var moveAnim = new DoubleAnimation(0, targetY, TimeSpan.FromMilliseconds(600))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        var fadeAnim = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(600))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };

        fadeAnim.Completed += (_, _) =>
        {
            animationCanvas.Children.Remove(text);
        };

        translateTransform.BeginAnimation(TranslateTransform.YProperty, moveAnim);
        text.BeginAnimation(OpacityProperty, fadeAnim);
    }

    private static TreeViewItem? FindTreeViewItemByNodeType(ItemsControl parent, string nodeType)
    {
        foreach (var item in parent.Items)
        {
            if (parent.ItemContainerGenerator.ContainerFromItem(item) is TreeViewItem tvi)
            {
                if (item is DriveTreeNode node && node.NodeType == nodeType)
                    return tvi;

                // Recurse into children
                var found = FindTreeViewItemByNodeType(tvi, nodeType);
                if (found != null)
                    return found;
            }
        }
        return null;
    }
}
