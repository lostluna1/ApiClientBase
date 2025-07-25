using ApiClient.Core.Models;

namespace ApiClient.Core.Contracts.Services;

/// <summary>
/// API集合管理服务接口
/// </summary>
public interface IApiCollectionService
{
    /// <summary>
    /// 获取所有API集合
    /// </summary>
    /// <returns>API集合列表</returns>
    Task<List<ApiCollection>> GetCollectionsAsync();

    /// <summary>
    /// 根据ID获取API集合
    /// </summary>
    /// <param name="collectionId">集合ID</param>
    /// <returns>API集合</returns>
    Task<ApiCollection?> GetCollectionByIdAsync(string collectionId);

    /// <summary>
    /// 保存API集合
    /// </summary>
    /// <param name="collection">API集合</param>
    Task SaveCollectionAsync(ApiCollection collection);

    /// <summary>
    /// 删除API集合
    /// </summary>
    /// <param name="collectionId">集合ID</param>
    Task DeleteCollectionAsync(string collectionId);

    /// <summary>
    /// 更新API集合
    /// </summary>
    /// <param name="collection">API集合</param>
    Task UpdateCollectionAsync(ApiCollection collection);

    /// <summary>
    /// 复制API集合
    /// </summary>
    /// <param name="collectionId">源集合ID</param>
    /// <param name="newName">新集合名称</param>
    /// <returns>复制的集合</returns>
    Task<ApiCollection> DuplicateCollectionAsync(string collectionId, string newName);

    /// <summary>
    /// 搜索API集合
    /// </summary>
    /// <param name="keyword">关键词</param>
    /// <returns>匹配的集合列表</returns>
    Task<List<ApiCollection>> SearchCollectionsAsync(string keyword);

    /// <summary>
    /// 添加请求到集合
    /// </summary>
    /// <param name="collectionId">集合ID</param>
    /// <param name="request">请求记录</param>
    /// <param name="folderId">文件夹ID（可选）</param>
    Task AddRequestToCollectionAsync(string collectionId, RequestRecord request, string? folderId = null);

    /// <summary>
    /// 从集合中移除请求
    /// </summary>
    /// <param name="collectionId">集合ID</param>
    /// <param name="requestId">请求ID</param>
    Task RemoveRequestFromCollectionAsync(string collectionId, string requestId);

    /// <summary>
    /// 创建文件夹
    /// </summary>
    /// <param name="collectionId">集合ID</param>
    /// <param name="folderName">文件夹名称</param>
    /// <param name="parentFolderId">父文件夹ID（可选）</param>
    /// <returns>创建的文件夹</returns>
    Task<ApiFolder> CreateFolderAsync(string collectionId, string folderName, string? parentFolderId = null);

    /// <summary>
    /// 删除文件夹
    /// </summary>
    /// <param name="collectionId">集合ID</param>
    /// <param name="folderId">文件夹ID</param>
    Task DeleteFolderAsync(string collectionId, string folderId);

    /// <summary>
    /// 重命名文件夹
    /// </summary>
    /// <param name="collectionId">集合ID</param>
    /// <param name="folderId">文件夹ID</param>
    /// <param name="newName">新名称</param>
    Task RenameFolderAsync(string collectionId, string folderId, string newName);

    /// <summary>
    /// 导出集合为JSON
    /// </summary>
    /// <param name="collectionId">集合ID</param>
    /// <returns>JSON字符串</returns>
    Task<string> ExportCollectionAsync(string collectionId);

    /// <summary>
    /// 获取集合统计信息
    /// </summary>
    /// <param name="collectionId">集合ID</param>
    /// <returns>统计信息</returns>
    Task<CollectionStatistics> GetCollectionStatisticsAsync(string collectionId);
}

/// <summary>
/// 集合统计信息
/// </summary>
public class CollectionStatistics
{
    /// <summary>
    /// 总请求数
    /// </summary>
    public int TotalRequests
    {
        get; set;
    }

    /// <summary>
    /// 文件夹数
    /// </summary>
    public int TotalFolders
    {
        get; set;
    }

    /// <summary>
    /// 按HTTP方法分组的统计
    /// </summary>
    public Dictionary<string, int> RequestsByMethod { get; set; } = [];

    /// <summary>
    /// 按标签分组的统计
    /// </summary>
    public Dictionary<string, int> RequestsByTag { get; set; } = [];

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTime LastUpdated
    {
        get; set;
    }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime Created
    {
        get; set;
    }
}