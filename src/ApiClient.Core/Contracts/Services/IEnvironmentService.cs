using ApiClient.Core.Models;

namespace ApiClient.Core.Contracts.Services;

/// <summary>
/// API环境管理服务接口
/// </summary>
public interface IEnvironmentService
{
    /// <summary>
    /// 获取所有环境
    /// </summary>
    /// <returns>环境列表</returns>
    Task<List<ApiEnvironment>> GetEnvironmentsAsync();

    /// <summary>
    /// 获取当前环境
    /// </summary>
    /// <returns>当前环境</returns>
    Task<ApiEnvironment?> GetCurrentEnvironmentAsync();

    /// <summary>
    /// 设置当前环境
    /// </summary>
    /// <param name="environmentId">环境ID</param>
    Task SetCurrentEnvironmentAsync(string environmentId);

    /// <summary>
    /// 添加环境
    /// </summary>
    /// <param name="environment">环境信息</param>
    Task AddEnvironmentAsync(ApiEnvironment environment);

    /// <summary>
    /// 更新环境
    /// </summary>
    /// <param name="environment">环境信息</param>
    Task UpdateEnvironmentAsync(ApiEnvironment environment);

    /// <summary>
    /// 删除环境
    /// </summary>
    /// <param name="environmentId">环境ID</param>
    Task DeleteEnvironmentAsync(string environmentId);

    /// <summary>
    /// 解析变量值
    /// </summary>
    /// <param name="value">包含变量的值</param>
    /// <returns>解析后的值</returns>
    Task<string> ResolveVariablesAsync(string value);

    /// <summary>
    /// 获取环境变量
    /// </summary>
    /// <param name="key">变量键</param>
    /// <returns>变量值</returns>
    Task<string?> GetVariableAsync(string key);

    /// <summary>
    /// 设置环境变量
    /// </summary>
    /// <param name="key">变量键</param>
    /// <param name="value">变量值</param>
    Task SetVariableAsync(string key, string value);
}