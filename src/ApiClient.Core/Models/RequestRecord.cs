using ApiClient.Core.Contracts.Services;
using Newtonsoft.Json;

namespace ApiClient.Core.Models;

/// <summary>
/// 请求记录
/// </summary>
public class RequestRecord
{
    /// <summary>
    /// 请求ID
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 请求名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// HTTP方法
    /// </summary>
    public string Method { get; set; } = "GET";

    /// <summary>
    /// 请求URL
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// 请求头
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = [];

    /// <summary>
    /// 请求体
    /// </summary>
    public string? Body
    {
        get; set;
    }

    /// <summary>
    /// 内容类型
    /// </summary>
    public string ContentType { get; set; } = "application/json";

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 最后修改时间
    /// </summary>
    public DateTime LastModified { get; set; } = DateTime.Now;

    /// <summary>
    /// 标签
    /// </summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description
    {
        get; set;
    }

    /// <summary>
    /// 收藏状态
    /// </summary>
    public bool IsFavorite
    {
        get; set;
    }

    /// <summary>
    /// 分组
    /// </summary>
    public string? Group
    {
        get; set;
    }

    /// <summary>
    /// 响应结果（用于保存最后一次请求的结果）
    /// </summary>
    public HttpResponseResult? LastResponse
    {
        get; set;
    }

    /// <summary>
    /// 请求体Schema信息（用于生成示例数据）
    /// </summary>
    public string? RequestBodySchemaJson
    {
        get; set;
    }

    /// <summary>
    /// 获取请求体Schema对象
    /// </summary>
    [JsonIgnore]
    public Schema? RequestBodySchema
    {
        get
        {
            if (string.IsNullOrWhiteSpace(RequestBodySchemaJson))
                return null;

            try
            {
                return JsonConvert.DeserializeObject<Schema>(RequestBodySchemaJson);
            }
            catch
            {
                return null;
            }
        }

        set => RequestBodySchemaJson = value != null ? JsonConvert.SerializeObject(value) : null;
    }
}