using Newtonsoft.Json;

namespace ApiClient.Core.Models;

/// <summary>
/// Swagger文档信息
/// </summary>
public class SwaggerDocument
{
    /// <summary>
    /// OpenAPI版本
    /// </summary>
    [JsonProperty("openapi")]
    public string? OpenApi
    {
        get; set;
    }

    /// <summary>
    /// Swagger版本
    /// </summary>
    [JsonProperty("swagger")]
    public string? Swagger
    {
        get; set;
    }

    /// <summary>
    /// API信息
    /// </summary>
    [JsonProperty("info")]
    public ApiInfo? Info
    {
        get; set;
    }

    /// <summary>
    /// 服务器配置
    /// </summary>
    [JsonProperty("servers")]
    public List<ServerInfo>? Servers
    {
        get; set;
    }

    /// <summary>
    /// 基础路径（Swagger 2.0）
    /// </summary>
    [JsonProperty("basePath")]
    public string? BasePath
    {
        get; set;
    }

    /// <summary>
    /// 主机地址（Swagger 2.0）
    /// </summary>
    [JsonProperty("host")]
    public string? Host
    {
        get; set;
    }

    /// <summary>
    /// 协议（Swagger 2.0）
    /// </summary>
    [JsonProperty("schemes")]
    public List<string>? Schemes
    {
        get; set;
    }

    /// <summary>
    /// 路径信息
    /// </summary>
    [JsonProperty("paths")]
    public Dictionary<string, PathItem>? Paths
    {
        get; set;
    }

    /// <summary>
    /// 组件定义（OpenAPI 3.0）
    /// </summary>
    [JsonProperty("components")]
    public Components? Components
    {
        get; set;
    }

    /// <summary>
    /// 定义（Swagger 2.0）
    /// </summary>
    [JsonProperty("definitions")]
    public Dictionary<string, object>? Definitions
    {
        get; set;
    }
}

/// <summary>
/// API基础信息
/// </summary>
public class ApiInfo
{
    /// <summary>
    /// API标题
    /// </summary>
    [JsonProperty("title")]
    public string? Title
    {
        get; set;
    }

    /// <summary>
    /// API描述
    /// </summary>
    [JsonProperty("description")]
    public string? Description
    {
        get; set;
    }

    /// <summary>
    /// API版本
    /// </summary>
    [JsonProperty("version")]
    public string? Version
    {
        get; set;
    }
}

/// <summary>
/// 服务器信息
/// </summary>
public class ServerInfo
{
    /// <summary>
    /// 服务器URL
    /// </summary>
    [JsonProperty("url")]
    public string? Url
    {
        get; set;
    }

    /// <summary>
    /// 服务器描述
    /// </summary>
    [JsonProperty("description")]
    public string? Description
    {
        get; set;
    }
}

/// <summary>
/// 路径项
/// </summary>
public class PathItem
{
    /// <summary>
    /// GET操作
    /// </summary>
    [JsonProperty("get")]
    public Operation? Get
    {
        get; set;
    }

    /// <summary>
    /// POST操作
    /// </summary>
    [JsonProperty("post")]
    public Operation? Post
    {
        get; set;
    }

    /// <summary>
    /// PUT操作
    /// </summary>
    [JsonProperty("put")]
    public Operation? Put
    {
        get; set;
    }

    /// <summary>
    /// DELETE操作
    /// </summary>
    [JsonProperty("delete")]
    public Operation? Delete
    {
        get; set;
    }

    /// <summary>
    /// PATCH操作
    /// </summary>
    [JsonProperty("patch")]
    public Operation? Patch
    {
        get; set;
    }

    /// <summary>
    /// HEAD操作
    /// </summary>
    [JsonProperty("head")]
    public Operation? Head
    {
        get; set;
    }

    /// <summary>
    /// OPTIONS操作
    /// </summary>
    [JsonProperty("options")]
    public Operation? Options
    {
        get; set;
    }

    /// <summary>
    /// 参数
    /// </summary>
    [JsonProperty("parameters")]
    public List<Parameter>? Parameters
    {
        get; set;
    }
}

/// <summary>
/// 操作定义
/// </summary>
public class Operation
{
    /// <summary>
    /// 操作标签
    /// </summary>
    [JsonProperty("tags")]
    public List<string>? Tags
    {
        get; set;
    }

    /// <summary>
    /// 操作摘要
    /// </summary>
    [JsonProperty("summary")]
    public string? Summary
    {
        get; set;
    }

    /// <summary>
    /// 操作描述
    /// </summary>
    [JsonProperty("description")]
    public string? Description
    {
        get; set;
    }

    /// <summary>
    /// 操作ID
    /// </summary>
    [JsonProperty("operationId")]
    public string? OperationId
    {
        get; set;
    }

    /// <summary>
    /// 参数
    /// </summary>
    [JsonProperty("parameters")]
    public List<Parameter>? Parameters
    {
        get; set;
    }

    /// <summary>
    /// 请求体（OpenAPI 3.0）
    /// </summary>
    [JsonProperty("requestBody")]
    public RequestBody? RequestBody
    {
        get; set;
    }

    /// <summary>
    /// 响应
    /// </summary>
    [JsonProperty("responses")]
    public Dictionary<string, Response>? Responses
    {
        get; set;
    }

    /// <summary>
    /// 消费的媒体类型（Swagger 2.0）
    /// </summary>
    [JsonProperty("consumes")]
    public List<string>? Consumes
    {
        get; set;
    }

    /// <summary>
    /// 生产的媒体类型（Swagger 2.0）
    /// </summary>
    [JsonProperty("produces")]
    public List<string>? Produces
    {
        get; set;
    }
}

/// <summary>
/// 参数定义
/// </summary>
public class Parameter
{
    /// <summary>
    /// 参数名称
    /// </summary>
    [JsonProperty("name")]
    public string? Name
    {
        get; set;
    }

    /// <summary>
    /// 参数位置
    /// </summary>
    [JsonProperty("in")]
    public string? In
    {
        get; set;
    }

    /// <summary>
    /// 参数描述
    /// </summary>
    [JsonProperty("description")]
    public string? Description
    {
        get; set;
    }

    /// <summary>
    /// 是否必需
    /// </summary>
    [JsonProperty("required")]
    public bool Required
    {
        get; set;
    }

    /// <summary>
    /// 参数类型（Swagger 2.0）
    /// </summary>
    [JsonProperty("type")]
    public string? Type
    {
        get; set;
    }

    /// <summary>
    /// 参数格式
    /// </summary>
    [JsonProperty("format")]
    public string? Format
    {
        get; set;
    }

    /// <summary>
    /// 架构（OpenAPI 3.0）
    /// </summary>
    [JsonProperty("schema")]
    public Schema? Schema
    {
        get; set;
    }

    /// <summary>
    /// 示例值
    /// </summary>
    [JsonProperty("example")]
    public object? Example
    {
        get; set;
    }
}

/// <summary>
/// 请求体定义
/// </summary>
public class RequestBody
{
    /// <summary>
    /// 请求体描述
    /// </summary>
    [JsonProperty("description")]
    public string? Description
    {
        get; set;
    }

    /// <summary>
    /// 内容
    /// </summary>
    [JsonProperty("content")]
    public Dictionary<string, MediaType>? Content
    {
        get; set;
    }

    /// <summary>
    /// 是否必需
    /// </summary>
    [JsonProperty("required")]
    public bool Required
    {
        get; set;
    }
}

/// <summary>
/// 媒体类型
/// </summary>
public class MediaType
{
    /// <summary>
    /// 架构
    /// </summary>
    [JsonProperty("schema")]
    public Schema? Schema
    {
        get; set;
    }

    /// <summary>
    /// 示例
    /// </summary>
    [JsonProperty("example")]
    public object? Example
    {
        get; set;
    }

    /// <summary>
    /// 示例集合
    /// </summary>
    [JsonProperty("examples")]
    public Dictionary<string, object>? Examples
    {
        get; set;
    }
}

/// <summary>
/// 架构定义
/// </summary>
public class Schema
{
    /// <summary>
    /// 类型
    /// </summary>
    [JsonProperty("type")]
    public string? Type
    {
        get; set;
    }

    /// <summary>
    /// 格式
    /// </summary>
    [JsonProperty("format")]
    public string? Format
    {
        get; set;
    }

    /// <summary>
    /// 引用
    /// </summary>
    [JsonProperty("$ref")]
    public string? Ref
    {
        get; set;
    }

    /// <summary>
    /// 属性
    /// </summary>
    [JsonProperty("properties")]
    public Dictionary<string, Schema>? Properties
    {
        get; set;
    }

    /// <summary>
    /// 数组项
    /// </summary>
    [JsonProperty("items")]
    public Schema? Items
    {
        get; set;
    }

    /// <summary>
    /// 必需字段
    /// </summary>
    [JsonProperty("required")]
    public List<string>? Required
    {
        get; set;
    }

    /// <summary>
    /// 示例
    /// </summary>
    [JsonProperty("example")]
    public object? Example
    {
        get; set;
    }
}

/// <summary>
/// 响应定义
/// </summary>
public class Response
{
    /// <summary>
    /// 响应描述
    /// </summary>
    [JsonProperty("description")]
    public string? Description
    {
        get; set;
    }

    /// <summary>
    /// 内容
    /// </summary>
    [JsonProperty("content")]
    public Dictionary<string, MediaType>? Content
    {
        get; set;
    }

    /// <summary>
    /// 架构（Swagger 2.0）
    /// </summary>
    [JsonProperty("schema")]
    public Schema? Schema
    {
        get; set;
    }
}

/// <summary>
/// 组件定义
/// </summary>
public class Components
{
    /// <summary>
    /// 架构
    /// </summary>
    [JsonProperty("schemas")]
    public Dictionary<string, Schema>? Schemas
    {
        get; set;
    }

    /// <summary>
    /// 安全架构
    /// </summary>
    [JsonProperty("securitySchemes")]
    public Dictionary<string, object>? SecuritySchemes
    {
        get; set;
    }
}