using System.Collections.ObjectModel;
using System.Windows.Input;
using ApiClient.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ApiClient.Controls;

public sealed partial class TabListControl : UserControl
{
    public ObservableCollection<RequestTabViewModel>? RequestTabs
    {
        get; set;
    }
    public RequestTabViewModel? SelectedTab
    {
        get; set;
    }
    public ICommand? SwitchToTabCommand
    {
        get; set;
    }
    public ICommand? CloseTabCommand
    {
        get; set;
    }
    public ICommand? CloseAllTabsCommand
    {
        get; set;
    }

    // 用于关闭弹出窗口的事件
    public event EventHandler? CloseRequested;

    public TabListControl()
    {
        this.InitializeComponent();
    }

    public void UpdateData(ObservableCollection<RequestTabViewModel> tabs, RequestTabViewModel? selectedTab,
        ICommand switchCommand, ICommand closeCommand, ICommand closeAllCommand)
    {
        RequestTabs = tabs;
        SelectedTab = selectedTab;
        SwitchToTabCommand = switchCommand;
        CloseTabCommand = closeCommand;
        CloseAllTabsCommand = closeAllCommand;

        // 设置ListView的数据源
        TabListView.ItemsSource = RequestTabs;

        // 设置当前选中项
        if (selectedTab != null && RequestTabs.Contains(selectedTab))
        {
            TabListView.SelectedItem = selectedTab;
        }

        // 更新UI
        UpdateUI();

        // 监听集合变化以实时更新
        if (RequestTabs != null)
        {
            RequestTabs.CollectionChanged += OnTabsCollectionChanged;
        }
    }

    private void OnTabsCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        UpdateUI();

        // 如果所有标签页都被关闭，自动关闭弹出窗口
        if (RequestTabs?.Count == 0)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    private void UpdateUI()
    {
        var count = RequestTabs?.Count ?? 0;
        /*        if (TabCountText != null)
                {
                    TabCountText.Text = count.ToString();
                }*/
        StatisticsText?.Text = GetTabStatistics(count);
    }

    private void OnTabItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is RequestTabViewModel tab)
        {
            SwitchToTabCommand?.Execute(tab);

            // 关闭弹出窗口
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnCloseButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element && element.DataContext is RequestTabViewModel tab)
        {
            CloseTabCommand?.Execute(tab);
        }
    }

    private void OnCloseAllTabsClick(object sender, RoutedEventArgs e)
    {
        CloseAllTabsCommand?.Execute(null);

        // 关闭弹出窗口
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    private string GetTabStatistics(int count)
    {
        return count switch
        {
            0 => "暂无打开的标签页",
            1 => "共 1 个标签页",
            _ => $"共 {count} 个标签页"
        };
    }

    // 清理资源
    public void Cleanup()
    {
        if (RequestTabs != null)
        {
            RequestTabs.CollectionChanged -= OnTabsCollectionChanged;
        }
    }
}
