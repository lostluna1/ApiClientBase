using System.Collections.ObjectModel;
using ApiClient.Contracts.Services;
using ApiClient.Core.Contracts.Services;
using ApiClient.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ApiClient.Messages;
using Microsoft.UI.Xaml;

namespace ApiClient.ViewModels;

public partial class MainViewModel : ObservableRecipient, IRecipient<ThemeChangedMessage>
{
    //private readonly IThemeSelectorService _themeSelectorService;
    private readonly IHttpClientService _httpClientService;
    private readonly IRequestHistoryService _historyService;
    private readonly IEnvironmentService _environmentService;
    private readonly ISwaggerImportService _swaggerImportService;
    private readonly IApiCollectionService _collectionService;

    [ObservableProperty]
    public partial TreeNodeViewModel? SelectedTreeNode
    {
        get;
        set;
    }

    [ObservableProperty]
    public partial string SearchKeyword { get; set; } = string.Empty;

    [ObservableProperty]
    public partial double LeftPanelWidth { get; set; } = 300;

    [ObservableProperty]
    public partial RequestTabViewModel? SelectedRequestTab
    {
        get; set;
    }

    /// <summary>
    /// 是否有选中的标签页（用于控制请求配置和响应区域的可见性）
    /// </summary>
    public bool HasSelectedTab => SelectedRequestTab != null;

    partial void OnLeftPanelWidthChanged(double value)
    {
        OnPropertyChanged(nameof(LeftPanelGridLength));
    }

    // 添加GridLength属性用于直接绑定
    public GridLength LeftPanelGridLength
    {
        get => new(LeftPanelWidth);
        set
        {
            if (Math.Abs(value.Value - LeftPanelWidth) > 0.1) // 避免微小变化触发更新
            {
                LeftPanelWidth = value.Value;
            }
        }
    }

    public ObservableCollection<RequestRecord> RequestHistory { get; } = [];

    /// <summary>
    /// TreeView的根节点集合
    /// </summary>
    public ObservableCollection<TreeNodeViewModel> TreeRootNodes { get; } = [];

    /// <summary>
    /// 请求标签页集合
    /// </summary>
    public ObservableCollection<RequestTabViewModel> RequestTabs { get; } = [];

    /// <summary>
    /// 当前主题
    /// </summary>
    [ObservableProperty]
    private ElementTheme currentTheme = ElementTheme.Default;

    public MainViewModel(
        IHttpClientService httpClientService,
        IRequestHistoryService requestHistoryService,
        IEnvironmentService environmentService,
        ISwaggerImportService swaggerImportService,
        IApiCollectionService apiCollectionService)
    {
        _httpClientService = httpClientService;
        _historyService = requestHistoryService;
        _environmentService = environmentService;
        _swaggerImportService = swaggerImportService;
        _collectionService = apiCollectionService;

        // 监听RequestTabs集合变化，通知相关属性
        RequestTabs.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(ShouldShowTabListButton));
        };

        // 注册为消息接收者
        WeakReferenceMessenger.Default.Register<ThemeChangedMessage>(this);

        // 初始化当前主题
        try
        {
            var themeSelectorService = App.GetService<IThemeSelectorService>();
            CurrentTheme = themeSelectorService.Theme;
        }
        catch
        {
            CurrentTheme = ElementTheme.Default;
        }

        // 初始化加载
        _ = LoadTreeDataAsync();
    }

    [RelayCommand]
    private async Task LoadTreeDataAsync()
    {
        try
        {
            var collections = await _collectionService.GetCollectionsAsync();

            TreeRootNodes.Clear();

            foreach (var collection in collections.OrderBy(c => c.Name))
            {
                var collectionNode = TreeNodeViewModel.CreateCollectionNode(collection);

                // 添加集合中的根级请求
                foreach (var request in collection.Requests)
                {
                    var requestNode = TreeNodeViewModel.CreateRequestNode(request);
                    requestNode.Parent = collectionNode;
                    collectionNode.Children.Add(requestNode);
                }

                // 添加文件夹
                foreach (var folder in collection.Folders)
                {
                    var folderNode = CreateFolderNodeRecursive(folder, collectionNode);
                    collectionNode.Children.Add(folderNode);
                }

                TreeRootNodes.Add(collectionNode);
            }
        }
        catch (Exception)
        {
            // 显示错误消息
            // ResponseContent = $"加载接口树失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SearchInterfacesAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchKeyword))
        {
            await LoadTreeDataAsync();
            return;
        }

        try
        {
            // 获取所有集合，然后在内存中进行搜索
            var allCollections = await _collectionService.GetCollectionsAsync();

            TreeRootNodes.Clear();

            foreach (var collection in allCollections.OrderBy(c => c.Name))
            {
                var collectionNode = TreeNodeViewModel.CreateCollectionNode(collection);
                var hasMatchingItems = false;

                // 搜索根级请求
                foreach (var request in collection.Requests)
                {
                    if (RequestMatchesSearch(request, SearchKeyword))
                    {
                        var requestNode = TreeNodeViewModel.CreateRequestNode(request);
                        requestNode.Parent = collectionNode;
                        collectionNode.Children.Add(requestNode);
                        hasMatchingItems = true;
                    }
                }

                // 搜索文件夹中的请求
                foreach (var folder in collection.Folders)
                {
                    var folderNode = CreateFolderNodeWithSearch(folder, SearchKeyword, collectionNode);
                    if (folderNode.Children.Count != 0)
                    {
                        collectionNode.Children.Add(folderNode);
                        hasMatchingItems = true;
                    }
                }

                // 检查集合本身是否匹配搜索条件
                if (!hasMatchingItems && CollectionMatchesSearch(collection, SearchKeyword))
                {
                    // 如果集合名称匹配，显示整个集合
                    foreach (var request in collection.Requests)
                    {
                        var requestNode = TreeNodeViewModel.CreateRequestNode(request);
                        requestNode.Parent = collectionNode;
                        collectionNode.Children.Add(requestNode);
                    }

                    foreach (var folder in collection.Folders)
                    {
                        var folderNode = CreateFolderNodeRecursive(folder, collectionNode);
                        collectionNode.Children.Add(folderNode);
                    }
                    hasMatchingItems = true;
                }

                if (hasMatchingItems)
                {
                    collectionNode.IsExpanded = true;
                    TreeRootNodes.Add(collectionNode);
                }
            }

            // 如果没有找到匹配的项目，显示一个提示
            if (!TreeRootNodes.Any())
            {
                // 可以考虑在UI中显示"无搜索结果"的提示
                // 这里暂时保持TreeView为空
            }
        }
        catch (Exception)
        {
            // ResponseContent = $"搜索失败: {ex.Message}";
        }
    }

    /// <summary>
    /// 创建新的请求标签页
    /// </summary>
    [RelayCommand]
    private void CreateNewRequestTab()
    {
        var newTab = new RequestTabViewModel(_httpClientService, _environmentService);
        RequestTabs.Add(newTab);
        SelectedRequestTab = newTab;
    }

    /// <summary>
    /// 关闭请求标签页
    /// </summary>
    [RelayCommand]
    private void CloseRequestTab(RequestTabViewModel? tab)
    {
        try
        {
            if (tab == null) return;

            var index = RequestTabs.IndexOf(tab);
            RequestTabs.Remove(tab);

            // 如果关闭的是当前选中的标签页，选择邻近的标签页
            if (SelectedRequestTab == tab)
            {
                if (RequestTabs.Count != 0)
                {
                    // 选择前一个标签页，如果没有则选择后一个
                    var newIndex = Math.Max(0, index - 1);
                    if (newIndex < RequestTabs.Count)
                    {
                        SelectedRequestTab = RequestTabs[newIndex];
                    }
                    else if (RequestTabs.Count != 0)
                    {
                        SelectedRequestTab = RequestTabs.Last();
                    }
                }
                else
                {
                    SelectedRequestTab = null;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CloseRequestTab error: {ex.Message}");
        }
    }

    /// <summary>
    /// 关闭所有标签页
    /// </summary>
    [RelayCommand]
    private void CloseAllTabs()
    {
        try
        {
            RequestTabs.Clear();
            SelectedRequestTab = null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CloseAllTabs error: {ex.Message}");
        }
    }

    /// <summary>
    /// 关闭左侧标签页
    /// </summary>
    [RelayCommand]
    private void CloseLeftTabs(RequestTabViewModel? targetTab)
    {
        if (targetTab == null) return;

        var targetIndex = RequestTabs.IndexOf(targetTab);
        if (targetIndex <= 0) return; // 没有左侧标签页

        // 从右向左移除，避免索引变化问题
        for (var i = targetIndex - 1; i >= 0; i--)
        {
            var tabToRemove = RequestTabs[i];
            RequestTabs.RemoveAt(i);

            // 如果移除的是当前选中的标签页，选择目标标签页
            if (SelectedRequestTab == tabToRemove)
            {
                SelectedRequestTab = targetTab;
            }
        }
    }

    /// <summary>
    /// 关闭右侧标签页
    /// </summary>
    [RelayCommand]
    private void CloseRightTabs(RequestTabViewModel? targetTab)
    {
        if (targetTab == null) return;

        var targetIndex = RequestTabs.IndexOf(targetTab);
        if (targetIndex < 0 || targetIndex >= RequestTabs.Count - 1) return; // 没有右侧标签页

        // 从右向左移除，避免索引变化问题
        for (var i = RequestTabs.Count - 1; i > targetIndex; i--)
        {
            var tabToRemove = RequestTabs[i];
            RequestTabs.RemoveAt(i);

            // 如果移除的是当前选中的标签页，选择目标标签页
            if (SelectedRequestTab == tabToRemove)
            {
                SelectedRequestTab = targetTab;
            }
        }
    }

    /// <summary>
    /// 切换到指定的标签页
    /// </summary>
    [RelayCommand]
    private void SwitchToTab(RequestTabViewModel? targetTab)
    {
        if (targetTab != null && RequestTabs.Contains(targetTab))
        {
            SelectedRequestTab = targetTab;
        }
    }

    /// <summary>
    /// 检查是否需要显示标签页列表按钮（标签页数量较多时）
    /// </summary>
    public bool ShouldShowTabListButton => RequestTabs.Count > 5;

    private static bool CollectionMatchesSearch(ApiCollection collection, string keyword)
    {
        var lowerKeyword = keyword.ToLower();
        return collection.Name.Contains(lowerKeyword, StringComparison.CurrentCultureIgnoreCase) ||
               (collection.Description?.ToLower().Contains(lowerKeyword, StringComparison.CurrentCultureIgnoreCase) ?? false) ||
               collection.Tags.Any(tag => tag.Contains(lowerKeyword, StringComparison.CurrentCultureIgnoreCase));
    }

    /// <summary>
    /// 处理TreeView选择变化
    /// </summary>
    partial void OnSelectedTreeNodeChanged(TreeNodeViewModel? value)
    {
        if (value?.NodeType == TreeNodeType.Request && value.RequestRecord != null)
        {
            OpenRequestInNewTab(value.RequestRecord);
        }
    }

    /// <summary>
    /// 处理选中标签页变化，同步TreeView选中状态
    /// </summary>
    partial void OnSelectedRequestTabChanged(RequestTabViewModel? value)
    {
        try
        {
            // 通知HasSelectedTab属性变化
            OnPropertyChanged(nameof(HasSelectedTab));

            // 更新所有标签页的IsCurrentTab状态
            foreach (var tab in RequestTabs)
            {
                tab.IsCurrentTab = (tab == value);
            }

            if (value?.AssociatedRequest != null)
            {
                // 查找对应的TreeView节点
                var targetNode = FindTreeNodeByRequestId(value.AssociatedRequest.Id);
                if (targetNode != null && targetNode != SelectedTreeNode)
                {
                    SelectedTreeNode = targetNode;
                }
            }
            else if (value == null)
            {
                // 没有选中的标签页时，清除TreeView选中状态
                SelectedTreeNode = null;
            }
        }
        catch (Exception ex)
        {
            // 记录异常但不抛出，避免应用程序崩溃
            System.Diagnostics.Debug.WriteLine($"OnSelectedRequestTabChanged error: {ex.Message}");
        }
    }

    /// <summary>
    /// 在新标签页中打开请求
    /// </summary>
    private void OpenRequestInNewTab(RequestRecord request)
    {
        try
        {
            if (request == null) return;

            // 检查是否已经存在相同请求的标签页
            var existingTab = RequestTabs.FirstOrDefault(tab =>
                tab.AssociatedRequest?.Id == request.Id);

            if (existingTab != null)
            {
                // 如果已存在，切换到该标签页
                SelectedRequestTab = existingTab;
                return;
            }

            // 创建新的标签页
            var newTab = new RequestTabViewModel(_httpClientService, _environmentService);
            newTab.LoadFromRequest(request);

            RequestTabs.Add(newTab);
            SelectedRequestTab = newTab;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OpenRequestInNewTab error: {ex.Message}");
        }
    }

    private static TreeNodeViewModel CreateFolderNodeRecursive(ApiFolder folder, TreeNodeViewModel parent)
    {
        var folderNode = TreeNodeViewModel.CreateFolderNode(folder);
        folderNode.Parent = parent;

        // 添加文件夹中的请求
        foreach (var request in folder.Requests)
        {
            var requestNode = TreeNodeViewModel.CreateRequestNode(request);
            requestNode.Parent = folderNode;
            folderNode.Children.Add(requestNode);
        }

        // 递归添加子文件夹
        foreach (var subFolder in folder.SubFolders)
        {
            var subFolderNode = CreateFolderNodeRecursive(subFolder, folderNode);
            folderNode.Children.Add(subFolderNode);
        }

        return folderNode;
    }

    private static TreeNodeViewModel CreateFolderNodeWithSearch(ApiFolder folder, string keyword, TreeNodeViewModel parent)
    {
        try
        {
            var folderNode = TreeNodeViewModel.CreateFolderNode(folder);
            folderNode.Parent = parent;

            // 添加匹配的请求
            foreach (var request in folder.Requests)
            {
                if (RequestMatchesSearch(request, keyword))
                {
                    var requestNode = TreeNodeViewModel.CreateRequestNode(request);
                    requestNode.Parent = folderNode;
                    folderNode.Children.Add(requestNode);
                }
            }

            // 递归搜索子文件夹
            foreach (var subFolder in folder.SubFolders)
            {
                var subFolderNode = CreateFolderNodeWithSearch(subFolder, keyword, folderNode);
                if (subFolderNode.Children.Count != 0)
                {
                    folderNode.Children.Add(subFolderNode);
                }
            }

            if (folderNode.Children.Count != 0)
            {
                folderNode.IsExpanded = true;
            }

            return folderNode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CreateFolderNodeWithSearch error: {ex.Message}");
            // 返回一个空的文件夹节点
            var folderNode = TreeNodeViewModel.CreateFolderNode(folder);
            folderNode.Parent = parent;
            return folderNode;
        }
    }

    private static bool RequestMatchesSearch(RequestRecord request, string keyword)
    {
        var lowerKeyword = keyword.ToLower();
        return request.Name.Contains(lowerKeyword, StringComparison.CurrentCultureIgnoreCase) ||
               request.Url.Contains(lowerKeyword, StringComparison.CurrentCultureIgnoreCase) ||
               request.Method.Contains(lowerKeyword, StringComparison.CurrentCultureIgnoreCase) ||
               (request.Description?.ToLower().Contains(lowerKeyword, StringComparison.CurrentCultureIgnoreCase) ?? false);
    }

    private async Task LoadHistoryAsync()
    {
        var history = await _historyService.GetRequestHistoryAsync();
        RequestHistory.Clear();
        foreach (var item in history.Take(20)) // 只显示最近20条
        {
            RequestHistory.Add(item);
        }
    }

    /// <summary>
    /// 根据请求ID查找对应的TreeView节点
    /// </summary>
    private TreeNodeViewModel? FindTreeNodeByRequestId(string requestId)
    {
        try
        {
            if (string.IsNullOrEmpty(requestId)) return null;

            foreach (var rootNode in TreeRootNodes)
            {
                var found = FindTreeNodeRecursive(rootNode, requestId);
                if (found != null) return found;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"FindTreeNodeByRequestId error: {ex.Message}");
        }
        return null;
    }

    /// <summary>
    /// 递归查找TreeView节点
    /// </summary>
    private static TreeNodeViewModel? FindTreeNodeRecursive(TreeNodeViewModel node, string requestId)
    {
        try
        {
            if (node == null || string.IsNullOrEmpty(requestId)) return null;

            // 检查当前节点
            if (node.NodeType == TreeNodeType.Request &&
                node.RequestRecord?.Id == requestId)
            {
                return node;
            }

            // 递归检查子节点
            foreach (var child in node.Children)
            {
                var found = FindTreeNodeRecursive(child, requestId);
                if (found != null) return found;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"FindTreeNodeRecursive error: {ex.Message}");
        }

        return null;
    }

    /// <summary>
    /// 接收主题变化消息
    /// </summary>
    public void Receive(ThemeChangedMessage message)
    {
        CurrentTheme = message.NewTheme;
        
        // 通知所有RequestTab实例主题变化
        foreach (var tab in RequestTabs)
        {
            // 如果RequestTab也实现了消息接收，这里就不需要手动通知了
            // 或者可以调用tab的方法来更新主题
        }
        
        System.Diagnostics.Debug.WriteLine($"MainViewModel 收到主题变化通知: {message.NewTheme}");
    }
}
