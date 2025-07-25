namespace ApiClient.Core.Contracts.Services;

public interface IHttpClientService
{
    /// <summary>
    /// 发送GET请求
    /// </summary>
    /// <param name="url">请求URL</param>
    /// <param name="headers">请求头</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>HTTP响应结果</returns>
    Task<HttpResponseResult> GetAsync(string url, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送POST请求
    /// </summary>
    /// <param name="url">请求URL</param>
    /// <param name="content">请求内容</param>
    /// <param name="headers">请求头</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>HTTP响应结果</returns>
    Task<HttpResponseResult> PostAsync(string url, string? content = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送PUT请求
    /// </summary>
    /// <param name="url">请求URL</param>
    /// <param name="content">请求内容</param>
    /// <param name="headers">请求头</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>HTTP响应结果</returns>
    Task<HttpResponseResult> PutAsync(string url, string? content = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送DELETE请求
    /// </summary>
    /// <param name="url">请求URL</param>
    /// <param name="headers">请求头</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>HTTP响应结果</returns>
    Task<HttpResponseResult> DeleteAsync(string url, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送PATCH请求
    /// </summary>
    /// <param name="url">请求URL</param>
    /// <param name="content">请求内容</param>
    /// <param name="headers">请求头</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>HTTP响应结果</returns>
    Task<HttpResponseResult> PatchAsync(string url, string? content = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送自定义HTTP方法请求
    /// </summary>
    /// <param name="method">HTTP方法</param>
    /// <param name="url">请求URL</param>
    /// <param name="content">请求内容</param>
    /// <param name="headers">请求头</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>HTTP响应结果</returns>
    Task<HttpResponseResult> SendAsync(HttpMethod method, string url, string? content = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 设置超时时间
    /// </summary>
    /// <param name="timeout">超时时间</param>
    void SetTimeout(TimeSpan timeout);

    /// <summary>
    /// 设置默认请求头
    /// </summary>
    /// <param name="headers">默认请求头</param>
    void SetDefaultHeaders(Dictionary<string, string> headers);

    /// <summary>
    /// 清除默认请求头
    /// </summary>
    void ClearDefaultHeaders();
}