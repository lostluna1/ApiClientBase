using Newtonsoft.Json;

namespace ApiClient.Core.Contracts.Services;

/// <summary>
/// HTTP响应结果
/// </summary>
public class HttpResponseResult
{
    /// <summary>
    /// 响应状态码
    /// </summary>
    public int StatusCode
    {
        get; set;
    }

    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccess
    {
        get; set;
    }

    /// <summary>
    /// 响应内容
    /// </summary>
    public string? Content
    {
        get; set;
    }

    /// <summary>
    /// 响应头
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = [];

    /// <summary>
    /// 错误消息
    /// </summary>
    public string? ErrorMessage
    {
        get; set;
    }

    /// <summary>
    /// 响应时间（毫秒）
    /// </summary>
    public long ResponseTimeMs
    {
        get; set;
    }

    /// <summary>
    /// 请求URL
    /// </summary>
    public string? RequestUrl
    {
        get; set;
    }

    /// <summary>
    /// HTTP方法
    /// </summary>
    public string? HttpMethod
    {
        get; set;
    }

    /// <summary>
    /// 请求时间
    /// </summary>
    public DateTime RequestTime
    {
        get; set;
    }

    /// <summary>
    /// 响应时间
    /// </summary>
    public DateTime ResponseTime
    {
        get; set;
    }

    /// <summary>
    /// 内容类型
    /// </summary>
    public string? ContentType
    {
        get; set;
    }

    /// <summary>
    /// 内容长度
    /// </summary>
    public long ContentLength
    {
        get; set;
    }

    /// <summary>
    /// 异常信息
    /// </summary>
    [JsonIgnore]
    public Exception? Exception
    {
        get; set;
    }
}