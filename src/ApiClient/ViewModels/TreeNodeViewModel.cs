using System.Collections.ObjectModel;
using ApiClient.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ApiClient.ViewModels;

/// <summary>
/// TreeView节点数据模型
/// </summary>
public partial class TreeNodeViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Icon { get; set; } = "📁";

    [ObservableProperty]
    public partial bool IsExpanded { get; set; } = false;

    [ObservableProperty]
    public partial TreeNodeType NodeType { get; set; } = TreeNodeType.Folder;

    /// <summary>
    /// 关联的请求记录（当节点类型为Request时）
    /// </summary>
    public RequestRecord? RequestRecord
    {
        get; set;
    }

    /// <summary>
    /// 关联的API集合（当节点类型为Collection时）
    /// </summary>
    public ApiCollection? ApiCollection
    {
        get; set;
    }

    /// <summary>
    /// 关联的文件夹（当节点类型为Folder时）
    /// </summary>
    public ApiFolder? ApiFolder
    {
        get; set;
    }

    /// <summary>
    /// 子节点
    /// </summary>
    public ObservableCollection<TreeNodeViewModel> Children { get; } = [];

    /// <summary>
    /// 父节点
    /// </summary>
    public TreeNodeViewModel? Parent
    {
        get; set;
    }

    /// <summary>
    /// 创建集合节点
    /// </summary>
    public static TreeNodeViewModel CreateCollectionNode(ApiCollection collection)
    {
        return new TreeNodeViewModel
        {
            Name = collection.Name,
            Icon = "📚",
            NodeType = TreeNodeType.Collection,
            ApiCollection = collection,
            IsExpanded = true
        };
    }

    /// <summary>
    /// 创建文件夹节点
    /// </summary>
    public static TreeNodeViewModel CreateFolderNode(ApiFolder folder)
    {
        var icon = folder.Name.ToLower() switch
        {
            var name when name.Contains("auth") => "🔐",
            var name when name.Contains("user") => "👤",
            var name when name.Contains("order") => "🛒",
            var name when name.Contains("product") => "📦",
            var name when name.Contains("payment") => "💳",
            _ => "📁"
        };

        return new TreeNodeViewModel
        {
            Name = folder.Name,
            Icon = icon,
            NodeType = TreeNodeType.Folder,
            ApiFolder = folder,
            IsExpanded = false
        };
    }

    /// <summary>
    /// 创建请求节点
    /// </summary>
    public static TreeNodeViewModel CreateRequestNode(RequestRecord request)
    {
        return new TreeNodeViewModel
        {
            Name = GetRequestDisplayName(request), // 移除HTTP方法前缀
            Icon = string.Empty, // 移除圆点图标
            NodeType = TreeNodeType.Request,
            RequestRecord = request
        };
    }

    private static string GetRequestDisplayName(RequestRecord request)
    {
        // 如果有自定义名称且不包含HTTP方法，直接使用
        if (!string.IsNullOrWhiteSpace(request.Name) &&
            request.Name != $"{request.Method} {request.Url}" &&
            !request.Name.StartsWith($"{request.Method} "))
        {
            return request.Name;
        }

        // 从URL中提取路径作为显示名称
        if (Uri.TryCreate(request.Url, UriKind.Absolute, out var uri))
        {
            var path = uri.PathAndQuery;
            // 如果路径太长，只显示最后几段
            if (path.Length > 50)
            {
                var segments = path.Split('/');
                if (segments.Length > 3)
                {
                    return $".../{string.Join("/", segments.TakeLast(2))}";
                }
            }
            return path;
        }

        // 如果URL无法解析，返回去掉HTTP方法的名称
        var displayName = request.Name ?? request.Url;
        if (displayName.StartsWith($"{request.Method} "))
        {
            displayName = displayName[(request.Method.Length + 1)..];
        }

        return displayName;
    }
}

/// <summary>
/// TreeView节点类型
/// </summary>
public enum TreeNodeType
{
    Collection,
    Folder,
    Request
}