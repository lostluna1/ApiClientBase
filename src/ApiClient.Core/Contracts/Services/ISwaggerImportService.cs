using ApiClient.Core.Models;

namespace ApiClient.Core.Contracts.Services;

/// <summary>
/// Swagger导入服务接口
/// </summary>
public interface ISwaggerImportService
{
    /// <summary>
    /// 从Swagger文档URL导入API集合
    /// </summary>
    /// <param name="swaggerUrl">Swagger文档URL</param>
    /// <param name="collectionName">集合名称（可选）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>导入的API集合</returns>
    Task<ApiCollection> ImportFromUrlAsync(string swaggerUrl, string? collectionName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 从JSON字符串导入API集合
    /// </summary>
    /// <param name="jsonContent">Swagger JSON内容</param>
    /// <param name="collectionName">集合名称（可选）</param>
    /// <returns>导入的API集合</returns>
    Task<ApiCollection> ImportFromJsonAsync(string jsonContent, string? collectionName = null);

    /// <summary>
    /// 验证Swagger文档格式
    /// </summary>
    /// <param name="jsonContent">JSON内容</param>
    /// <returns>验证结果</returns>
    Task<SwaggerValidationResult> ValidateSwaggerAsync(string jsonContent);

    /// <summary>
    /// 检查Swagger URL是否可访问
    /// </summary>
    /// <param name="url">Swagger文档URL</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>检查结果</returns>
    Task<UrlValidationResult> ValidateUrlAsync(string url, CancellationToken cancellationToken = default);

    /// <summary>
    /// 生成请求体示例
    /// </summary>
    /// <param name="schema">架构定义</param>
    /// <returns>生成的示例JSON</returns>
    string GenerateExampleFromSchema(Schema schema);

    /// <summary>
    /// 解析参数为请求头和查询参数
    /// </summary>
    /// <param name="parameters">参数列表</param>
    /// <returns>解析结果</returns>
    (Dictionary<string, string> headers, Dictionary<string, string> queryParams) ParseParameters(List<Parameter> parameters);
}

/// <summary>
/// Swagger验证结果
/// </summary>
public class SwaggerValidationResult
{
    /// <summary>
    /// 是否有效
    /// </summary>
    public bool IsValid
    {
        get; set;
    }

    /// <summary>
    /// 错误消息
    /// </summary>
    public string? ErrorMessage
    {
        get; set;
    }

    /// <summary>
    /// 警告消息列表
    /// </summary>
    public List<string> Warnings { get; set; } = [];

    /// <summary>
    /// 检测到的Swagger版本
    /// </summary>
    public string? DetectedVersion
    {
        get; set;
    }

    /// <summary>
    /// 检测到的API数量
    /// </summary>
    public int ApiCount
    {
        get; set;
    }
}

/// <summary>
/// URL验证结果
/// </summary>
public class UrlValidationResult
{
    /// <summary>
    /// 是否可访问
    /// </summary>
    public bool IsAccessible
    {
        get; set;
    }

    /// <summary>
    /// HTTP状态码
    /// </summary>
    public int StatusCode
    {
        get; set;
    }

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
    /// 内容类型
    /// </summary>
    public string? ContentType
    {
        get; set;
    }

    /// <summary>
    /// 内容大小
    /// </summary>
    public long ContentLength
    {
        get; set;
    }
}