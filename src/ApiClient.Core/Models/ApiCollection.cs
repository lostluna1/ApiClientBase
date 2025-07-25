namespace ApiClient.Core.Models;

/// <summary>
/// API集合
/// </summary>
public class ApiCollection
{
    /// <summary>
    /// 集合ID
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 集合名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 集合描述
    /// </summary>
    public string? Description
    {
        get; set;
    }

    /// <summary>
    /// 集合版本
    /// </summary>
    public string? Version
    {
        get; set;
    }

    /// <summary>
    /// 基础URL
    /// </summary>
    public string? BaseUrl
    {
        get; set;
    }

    /// <summary>
    /// 请求记录列表
    /// </summary>
    public List<RequestRecord> Requests { get; set; } = [];

    /// <summary>
    /// 文件夹
    /// </summary>
    public List<ApiFolder> Folders { get; set; } = [];

    /// <summary>
    /// 变量
    /// </summary>
    public Dictionary<string, string> Variables { get; set; } = [];

    /// <summary>
    /// 认证信息
    /// </summary>
    public AuthenticationInfo? Authentication
    {
        get; set;
    }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 最后修改时间
    /// </summary>
    public DateTime LastModified { get; set; } = DateTime.Now;

    /// <summary>
    /// 导入来源
    /// </summary>
    public ImportSource ImportSource { get; set; } = ImportSource.Manual;

    /// <summary>
    /// 原始Swagger文档URL（如果从URL导入）
    /// </summary>
    public string? SwaggerUrl
    {
        get; set;
    }

    /// <summary>
    /// 标签
    /// </summary>
    public List<string> Tags { get; set; } = [];
}

/// <summary>
/// API文件夹
/// </summary>
public class ApiFolder
{
    /// <summary>
    /// 文件夹ID
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 文件夹名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 文件夹描述
    /// </summary>
    public string? Description
    {
        get; set;
    }

    /// <summary>
    /// 父文件夹ID
    /// </summary>
    public string? ParentId
    {
        get; set;
    }

    /// <summary>
    /// 请求记录列表
    /// </summary>
    public List<RequestRecord> Requests { get; set; } = [];

    /// <summary>
    /// 子文件夹列表
    /// </summary>
    public List<ApiFolder> SubFolders { get; set; } = [];

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// 认证信息
/// </summary>
public class AuthenticationInfo
{
    /// <summary>
    /// 认证类型
    /// </summary>
    public AuthenticationType Type { get; set; } = AuthenticationType.None;

    /// <summary>
    /// API Key配置
    /// </summary>
    public ApiKeyAuth? ApiKey
    {
        get; set;
    }

    /// <summary>
    /// Bearer Token配置
    /// </summary>
    public BearerTokenAuth? BearerToken
    {
        get; set;
    }

    /// <summary>
    /// Basic Auth配置
    /// </summary>
    public BasicAuth? Basic
    {
        get; set;
    }

    /// <summary>
    /// OAuth2配置
    /// </summary>
    public OAuth2Auth? OAuth2
    {
        get; set;
    }
}

/// <summary>
/// 认证类型
/// </summary>
public enum AuthenticationType
{
    None,
    ApiKey,
    BearerToken,
    Basic,
    OAuth2
}

/// <summary>
/// API Key认证
/// </summary>
public class ApiKeyAuth
{
    /// <summary>
    /// 键名
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// 键值
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// 位置（header, query, cookie）
    /// </summary>
    public string In { get; set; } = "header";
}

/// <summary>
/// Bearer Token认证
/// </summary>
public class BearerTokenAuth
{
    /// <summary>
    /// Token值
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Token前缀
    /// </summary>
    public string Prefix { get; set; } = "Bearer";
}

/// <summary>
/// Basic认证
/// </summary>
public class BasicAuth
{
    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 密码
    /// </summary>
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// OAuth2认证
/// </summary>
public class OAuth2Auth
{
    /// <summary>
    /// 访问令牌
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// 刷新令牌
    /// </summary>
    public string? RefreshToken
    {
        get; set;
    }

    /// <summary>
    /// 令牌类型
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime? ExpiresAt
    {
        get; set;
    }
}

/// <summary>
/// 导入来源
/// </summary>
public enum ImportSource
{
    /// <summary>
    /// 手动创建
    /// </summary>
    Manual,

    /// <summary>
    /// 从Swagger URL导入
    /// </summary>
    SwaggerUrl,

    /// <summary>
    /// 从JSON文件导入
    /// </summary>
    JsonFile,

    /// <summary>
    /// 从Postman导入
    /// </summary>
    Postman
}