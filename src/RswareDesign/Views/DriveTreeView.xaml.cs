using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using CommunityToolkit.Mvvm.Messaging;
using RswareDesign.Models;
using RswareDesign.ViewModels;

namespace RswareDesign.Views;

public partial class DriveTreeView : UserControl
{
    private string? _activeDrive; // null = collapsed, "A"/"B"/"C"/"D" = expanded
    private const double FeaturePanelHeight = 48;

    public DriveTreeView()
    {
        InitializeComponent();

        WeakReferenceMessenger.Default.Register<FavoriteAnimationMessage>(this, (_, msg) =>
        {
            ShowFavoriteAnimation(msg.IsAdded);
        });

        // Light up A/B/C/D buttons when any panel for that drive is visible
        WeakReferenceMessenger.Default.Register<ComparePanelChangedMessage>(this, (_, _) =>
        {
            UpdateDriveIndicators();
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

    // ═══════════════════════════════════════════════════════════
    //  DRIVE SELECTION → slide feature panel
    // ═══════════════════════════════════════════════════════════

    private void OnDriveClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;
        var driveId = btn.Tag?.ToString() ?? "A";

        if (_activeDrive == driveId)
        {
            // Same drive clicked again → collapse
            CollapseFeaturePanel();
            _activeDrive = null;
        }
        else
        {
            // Different drive → update & expand
            _activeDrive = driveId;
            UpdateDriveHighlight();
            UpdateFeatureLabel();
            UpdateFeatureButtonHighlight();

            if (FeaturePanel.Height == 0)
                ExpandFeaturePanel();
        }
    }

    private void UpdateDriveHighlight()
    {
        var buttons = new[] { (BtnDriveA, TxtDriveA, "A", "PanelAAccent"),
                              (BtnDriveB, TxtDriveB, "B", "PanelBAccent"),
                              (BtnDriveC, TxtDriveC, "C", "PanelCAccent"),
                              (BtnDriveD, TxtDriveD, "D", "PanelDAccent") };

        var vm = DataContext as MainWindowViewModel;

        foreach (var (btn, txt, driveId, accentKey) in buttons)
        {
            bool isActive = btn.Tag?.ToString() == _activeDrive;
            bool hasVisiblePanel = IsDriveVisible(vm, driveId);

            if (isActive)
            {
                btn.Background = GetWpfBrush(accentKey);
                txt.Foreground = GetWpfBrush("TextOnPrimary");
            }
            else if (hasVisiblePanel)
            {
                // Drive has visible panels but is not the active selection → dim indicator
                var accentBrush = GetWpfBrush(accentKey);
                txt.Foreground = accentBrush;
                btn.Background = Brushes.Transparent;
            }
            else
            {
                btn.Background = Brushes.Transparent;
                txt.Foreground = GetWpfBrush("TextSecondary");
            }
        }
    }

    /// <summary>
    /// Update drive button indicators when panel visibility changes.
    /// Called from ComparePanelChangedMessage handler.
    /// </summary>
    private void UpdateDriveIndicators()
    {
        UpdateDriveHighlight();
        UpdateFeatureButtonHighlight();
    }

    private static bool IsDriveVisible(MainWindowViewModel? vm, string driveId)
    {
        if (vm == null) return false;
        // Check parameter panels, graph instances, or control instances
        bool hasParam = driveId switch
        {
            "A" => vm.IsPanelAVisible,
            "B" => vm.IsPanelBVisible,
            "C" => vm.IsPanelCVisible,
            "D" => vm.IsPanelDVisible,
            _ => false,
        };
        return hasParam || vm.ActiveGraphDrives.Contains(driveId) || vm.ActiveControlDrives.Contains(driveId);
    }

    private void UpdateFeatureButtonHighlight()
    {
        if (_activeDrive == null)
        {
            ResetFeatureButton(BtnParam);
            ResetFeatureButton(BtnGraph);
            ResetFeatureButton(BtnChart);
            return;
        }

        var vm = DataContext as MainWindowViewModel;
        if (vm == null) return;

        var accentKey = $"Panel{_activeDrive}Accent";
        var brushKey = $"Panel{_activeDrive}Brush";

        // Param: check IsPanelXVisible
        bool hasParam = _activeDrive switch
        {
            "A" => vm.IsPanelAVisible, "B" => vm.IsPanelBVisible,
            "C" => vm.IsPanelCVisible, "D" => vm.IsPanelDVisible, _ => false,
        };
        SetFeatureButton(BtnParam, hasParam, accentKey, brushKey);

        // Graph: check ActiveGraphDrives
        bool hasGraph = vm.ActiveGraphDrives.Contains(_activeDrive);
        SetFeatureButton(BtnGraph, hasGraph, accentKey, brushKey);

        // Control: check ActiveControlDrives
        bool hasControl = vm.ActiveControlDrives.Contains(_activeDrive);
        SetFeatureButton(BtnChart, hasControl, accentKey, brushKey);
    }

    private void SetFeatureButton(Button btn, bool isActive, string accentKey, string brushKey)
    {
        if (isActive)
        {
            btn.Background = GetWpfBrush(brushKey);
            // Set all child TextBlocks to TextOnPrimary
            if (btn.Content is StackPanel sp)
                foreach (var child in sp.Children)
                {
                    if (child is System.Windows.Controls.TextBlock tb)
                        tb.Foreground = GetWpfBrush("TextOnPrimary");
                    else if (child is MaterialDesignThemes.Wpf.PackIcon icon)
                        icon.Foreground = GetWpfBrush("TextOnPrimary");
                }
        }
        else
        {
            ResetFeatureButton(btn);
        }
    }

    private void ResetFeatureButton(Button btn)
    {
        btn.Background = Brushes.Transparent;
        // Restore each button's original icon color
        string iconColor = btn == BtnParam ? "PrimaryBrush"
                         : btn == BtnGraph ? "SuccessBrush"
                         : "WarningBrush"; // BtnChart (Control)
        if (btn.Content is StackPanel sp)
            foreach (var child in sp.Children)
            {
                if (child is System.Windows.Controls.TextBlock tb)
                    tb.Foreground = GetWpfBrush("TextPrimary");
                else if (child is MaterialDesignThemes.Wpf.PackIcon icon)
                    icon.Foreground = GetWpfBrush(iconColor);
            }
    }

    private void UpdateFeatureLabel()
    {
        var accentKey = _activeDrive switch
        {
            "A" => "PanelAAccent", "B" => "PanelBAccent",
            "C" => "PanelCAccent", "D" => "PanelDAccent", _ => "TextSecondary"
        };
        TxtSelectedDrive.Text = $"Drive {_activeDrive}";
        TxtSelectedDrive.Foreground = GetWpfBrush(accentKey);
    }

    private void ExpandFeaturePanel()
    {
        var anim = new DoubleAnimation(0, FeaturePanelHeight, TimeSpan.FromMilliseconds(200))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        FeaturePanel.BeginAnimation(HeightProperty, anim);
    }

    private void CollapseFeaturePanel()
    {
        var anim = new DoubleAnimation(FeaturePanelHeight, 0, TimeSpan.FromMilliseconds(150))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };
        anim.Completed += (_, _) =>
        {
            UpdateDriveHighlight(); // clear all highlights
        };
        FeaturePanel.BeginAnimation(HeightProperty, anim);
    }

    // ═══════════════════════════════════════════════════════════
    //  FEATURE BUTTONS → send message for active drive
    // ═══════════════════════════════════════════════════════════

    private void OnGraphClick(object sender, RoutedEventArgs e)
    {
        if (_activeDrive == null) return;
        WeakReferenceMessenger.Default.Send(
            new ToggleMonitorSectionMessage("Oscilloscope", _activeDrive));
    }

    private void OnChartClick(object sender, RoutedEventArgs e)
    {
        if (_activeDrive == null) return;
        WeakReferenceMessenger.Default.Send(
            new ToggleMonitorSectionMessage("ControlPanel", _activeDrive));
    }

    private void OnParamClick(object sender, RoutedEventArgs e)
    {
        if (_activeDrive == null) return;
        var vm = DataContext as MainWindowViewModel;
        if (vm == null) return;

        switch (_activeDrive)
        {
            case "A": vm.IsPanelAVisible = !vm.IsPanelAVisible; break;
            case "B": vm.IsPanelBVisible = !vm.IsPanelBVisible; break;
            case "C": vm.IsPanelCVisible = !vm.IsPanelCVisible; break;
            case "D": vm.IsPanelDVisible = !vm.IsPanelDVisible; break;
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  FAVORITES ANIMATION
    // ═══════════════════════════════════════════════════════════

    private void ShowFavoriteAnimation(bool isAdded)
    {
        var favoritesItem = FindTreeViewItemByNodeType(driveTree, "Favorites");
        if (favoritesItem == null) return;

        var transform = favoritesItem.TransformToAncestor(animationCanvas);
        var itemPos = transform.Transform(new Point(0, 0));

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

        Canvas.SetLeft(text, itemPos.X + favoritesItem.ActualWidth - 20);
        Canvas.SetTop(text, itemPos.Y + 2);
        animationCanvas.Children.Add(text);

        var translateTransform = (TranslateTransform)text.RenderTransform;
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

                var found = FindTreeViewItemByNodeType(tvi, nodeType);
                if (found != null)
                    return found;
            }
        }
        return null;
    }

    private static Brush GetWpfBrush(string key)
    {
        return Application.Current.TryFindResource(key) is Brush brush ? brush : Brushes.Gray;
    }
}
