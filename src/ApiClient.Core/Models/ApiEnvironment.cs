namespace ApiClient.Core.Models;

/// <summary>
/// API环境
/// </summary>
public class ApiEnvironment
{
    /// <summary>
    /// 环境ID
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 环境名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 基础URL
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// 环境变量
    /// </summary>
    public Dictionary<string, string> Variables { get; set; } = [];

    /// <summary>
    /// 默认请求头
    /// </summary>
    public Dictionary<string, string> DefaultHeaders { get; set; } = [];

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description
    {
        get; set;
    }

    /// <summary>
    /// 是否激活
    /// </summary>
    public bool IsActive
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
    /// 超时设置（秒）
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// 是否启用SSL验证
    /// </summary>
    public bool EnableSslVerification { get; set; } = true;

    /// <summary>
    /// 代理设置
    /// </summary>
    public ProxySettings? ProxySettings
    {
        get; set;
    }
}

/// <summary>
/// 代理设置
/// </summary>
public class ProxySettings
{
    /// <summary>
    /// 是否启用代理
    /// </summary>
    public bool Enabled
    {
        get; set;
    }

    /// <summary>
    /// 代理服务器地址
    /// </summary>
    public string? Host
    {
        get; set;
    }

    /// <summary>
    /// 代理服务器端口
    /// </summary>
    public int Port
    {
        get; set;
    }

    /// <summary>
    /// 用户名
    /// </summary>
    public string? Username
    {
        get; set;
    }

    /// <summary>
    /// 密码
    /// </summary>
    public string? Password
    {
        get; set;
    }
}