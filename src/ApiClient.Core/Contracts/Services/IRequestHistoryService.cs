using ApiClient.Core.Models;

namespace ApiClient.Core.Contracts.Services;

/// <summary>
/// 请求历史服务接口
/// </summary>
public interface IRequestHistoryService
{
    /// <summary>
    /// 保存请求记录
    /// </summary>
    /// <param name="request">请求记录</param>
    Task SaveRequestAsync(RequestRecord request);

    /// <summary>
    /// 获取所有请求历史
    /// </summary>
    /// <returns>请求历史列表</returns>
    Task<List<RequestRecord>> GetRequestHistoryAsync();

    /// <summary>
    /// 根据ID获取请求记录
    /// </summary>
    /// <param name="id">请求ID</param>
    /// <returns>请求记录</returns>
    Task<RequestRecord?> GetRequestByIdAsync(string id);

    /// <summary>
    /// 删除请求记录
    /// </summary>
    /// <param name="id">请求ID</param>
    Task DeleteRequestAsync(string id);

    /// <summary>
    /// 清空请求历史
    /// </summary>
    Task ClearHistoryAsync();

    /// <summary>
    /// 搜索请求历史
    /// </summary>
    /// <param name="keyword">关键词</param>
    /// <returns>匹配的请求记录</returns>
    Task<List<RequestRecord>> SearchRequestsAsync(string keyword);
}