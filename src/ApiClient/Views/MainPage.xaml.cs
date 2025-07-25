using System.ComponentModel;
using ApiClient.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace ApiClient.Views;

public sealed partial class MainPage : Page, INotifyPropertyChanged
{
    private RequestTabViewModel? _previousSelectedTab;

    public MainViewModel ViewModel
    {
        get;
    }

    // 简单的属性，只负责更新Grid列宽，不触发复杂的ViewModel逻辑
    public double LeftPanelWidthValue
    {
        get;
        set
        {
            if (Math.Abs(field - value) > 0.1)
            {
                field = value;
                LeftColumn.Width = new GridLength(value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LeftPanelWidthValue)));
            }
        }
    } = 300;

    public event PropertyChangedEventHandler? PropertyChanged;

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();
        
        // 监听SelectedRequestTab变化以处理标签页切换
        ViewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.SelectedRequestTab))
        {
            HandleTabSwitching();
        }
    }

    private void HandleTabSwitching()
    {
        // 在切换到新标签页之前，保存当前UI状态到之前的标签页
        if (_previousSelectedTab != null)
        {
            SaveCurrentUIStateToTab(_previousSelectedTab);
        }

        // 更新到新的标签页
        _previousSelectedTab = ViewModel.SelectedRequestTab;
    }

    private void SaveCurrentUIStateToTab(RequestTabViewModel tab)
    {
        try
        {
            // 保存统一编辑器的内容
            if (UnifiedBodyEditor != null && UnifiedBodyEditor.Visibility == Visibility.Visible && !string.IsNullOrEmpty(UnifiedBodyEditor.Text))
            {
                // 使用UnifiedBodyContent属性，它会根据SelectedBodyType自动保存到正确的属性
                tab.UnifiedBodyContent = UnifiedBodyEditor.Text;
            }

            // 保存响应编辑器的内容（只读，通常不需要保存）
            // ResponseEditor是只读的，所以不需要保存回ViewModel

            // 注意：其他TextBox控件由于双向绑定，通常会自动同步
            // 但如果发现有控件没有正确同步，可以在这里手动保存
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"保存UI状态失败: {ex.Message}");
        }
    }

    private void OnTreeViewSelectionChanged(TreeView sender, TreeViewSelectionChangedEventArgs args)
    {
        if (args.AddedItems.Count > 0 && args.AddedItems[0] is TreeNodeViewModel selectedNode)
        {
            ViewModel.SelectedTreeNode = selectedNode;
        }
    }

    private void OnSearchSubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args) => ViewModel.SearchInterfacesCommand.Execute(null);

    private async void FromUrl_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var dialog = new ImportFromUrlDialog(ViewModel) { XamlRoot = App.MainWindow.Content.XamlRoot };
        await dialog.ShowAsync();
    }

    private async void FromJson_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var dialog = new ImportFromJsonDialog(ViewModel) { XamlRoot = App.MainWindow.Content.XamlRoot };
        await dialog.ShowAsync();
    }

    private void ClearSearch_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ViewModel.SearchKeyword = string.Empty;
        ViewModel.LoadTreeDataCommand.Execute(null);
    }

    private void OnTabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        if (args.Item is RequestTabViewModel tab)
        {
            // 如果关闭的是当前标签页，先保存状态
            if (tab == _previousSelectedTab)
            {
                SaveCurrentUIStateToTab(tab);
                _previousSelectedTab = null;
            }
            
            ViewModel.CloseRequestTabCommand.Execute(tab);
        }
    }

    private void OnAddTabButtonClick(TabView sender, object args) => ViewModel.CreateNewRequestTabCommand.Execute(null);

    private void OnTabViewItemRightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        if (sender is TabViewItem tabViewItem && tabViewItem.DataContext is RequestTabViewModel tabViewModel)
        {
            ShowTabContextMenu(tabViewItem, tabViewModel, e.GetPosition(tabViewItem));
        }
    }

    private void OnTabListButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            ShowTabListFlyout(button);
        }
    }

    private void ShowTabListFlyout(FrameworkElement target)
    {
        // 创建弹出窗口
        var flyout = new Flyout();

        // 创建标签页列表控件
        var tabListControl = new ApiClient.Controls.TabListControl();

        // 监听关闭请求事件
        tabListControl.CloseRequested += (sender, e) =>
        {
            flyout.Hide();
        };

        // 更新数据
        tabListControl.UpdateData(
            ViewModel.RequestTabs,
            ViewModel.SelectedRequestTab,
            ViewModel.SwitchToTabCommand,
            ViewModel.CloseRequestTabCommand,
            ViewModel.CloseAllTabsCommand
        );

        // 当弹出窗口关闭时清理资源
        flyout.Closed += (sender, e) =>
        {
            tabListControl.Cleanup();
        };

        flyout.Content = tabListControl;
        flyout.Placement = Microsoft.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.BottomEdgeAlignedLeft;
        flyout.ShowAt(target);
    }

    private void ShowTabContextMenu(TabViewItem tabViewItem, RequestTabViewModel tabViewModel, Windows.Foundation.Point position)
    {
        var menuFlyout = new MenuFlyout();

        // 关闭所有标签页
        var closeAllItem = new MenuFlyoutItem
        {
            Text = "关闭所有标签页",
            Icon = new SymbolIcon(Symbol.Clear)
        };
        closeAllItem.Click += (s, e) => ViewModel.CloseAllTabsCommand.Execute(null);
        menuFlyout.Items.Add(closeAllItem);

        // 分隔符
        menuFlyout.Items.Add(new MenuFlyoutSeparator());

        // 关闭左侧标签页
        var closeLeftItem = new MenuFlyoutItem
        {
            Text = "关闭左侧标签页",
            Icon = new SymbolIcon(Symbol.Previous)
        };
        closeLeftItem.Click += (s, e) => ViewModel.CloseLeftTabsCommand.Execute(tabViewModel);

        // 检查是否有左侧标签页
        var tabIndex = ViewModel.RequestTabs.IndexOf(tabViewModel);
        closeLeftItem.IsEnabled = tabIndex > 0;
        menuFlyout.Items.Add(closeLeftItem);

        // 关闭右侧标签页
        var closeRightItem = new MenuFlyoutItem
        {
            Text = "关闭右侧标签页",
            Icon = new SymbolIcon(Symbol.Next)
        };
        closeRightItem.Click += (s, e) => ViewModel.CloseRightTabsCommand.Execute(tabViewModel);

        // 检查是否有右侧标签页
        closeRightItem.IsEnabled = tabIndex >= 0 && tabIndex < ViewModel.RequestTabs.Count - 1;
        menuFlyout.Items.Add(closeRightItem);

        // 显示菜单
        menuFlyout.ShowAt(tabViewItem, position);
    }

    public string GetEmptyStateMessage(string searchKeyword)
    {
        return !string.IsNullOrWhiteSpace(searchKeyword)
            ? $"未找到包含 \"{searchKeyword}\" 的接口\n请尝试其他关键词或清空搜索"
            : "还没有导入任何API接口\n点击上方\"导入接口\"按钮开始导入";
    }

    public string GetGenerateExampleTooltip(bool canGenerateFromSchema)
    {
        return canGenerateFromSchema ? "基于Swagger文档的Schema生成示例数据" : "生成通用示例数据（当前接口无Schema信息）";
    }
}
