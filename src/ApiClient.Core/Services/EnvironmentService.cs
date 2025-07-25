using System.Text.RegularExpressions;
using ApiClient.Core.Contracts.Services;
using ApiClient.Core.Models;

namespace ApiClient.Core.Services;

/// <summary>
/// 环境管理服务实现
/// </summary>
public class EnvironmentService : IEnvironmentService
{
    private readonly IFileService _fileService;
    private readonly string _environmentFolderPath;
    private readonly string _environmentFileName = "environments.json";
    private readonly string _currentEnvironmentFileName = "current_environment.json";
    private List<ApiEnvironment>? _cachedEnvironments;
    private ApiEnvironment? _cachedCurrentEnvironment;

    public EnvironmentService(IFileService fileService)
    {
        _fileService = fileService;
        _environmentFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ApiClient", "Environments");
    }

    public async Task<List<ApiEnvironment>> GetEnvironmentsAsync()
    {
        if (_cachedEnvironments != null)
        {
            return _cachedEnvironments;
        }

        try
        {
            var environments = _fileService.Read<List<ApiEnvironment>>(_environmentFolderPath, _environmentFileName);
            _cachedEnvironments = environments ?? await CreateDefaultEnvironmentsAsync();
        }
        catch
        {
            _cachedEnvironments = await CreateDefaultEnvironmentsAsync();
        }

        return _cachedEnvironments;
    }

    public async Task<ApiEnvironment?> GetCurrentEnvironmentAsync()
    {
        if (_cachedCurrentEnvironment != null)
        {
            return _cachedCurrentEnvironment;
        }

        try
        {
            var currentEnvData = _fileService.Read<CurrentEnvironmentData>(_environmentFolderPath, _currentEnvironmentFileName);
            if (currentEnvData?.EnvironmentId != null)
            {
                var environments = await GetEnvironmentsAsync();
                _cachedCurrentEnvironment = environments.FirstOrDefault(e => e.Id == currentEnvData.EnvironmentId);
            }
        }
        catch
        {
            // 忽略错误，返回默认环境
        }

        // 如果没有找到当前环境，返回第一个环境
        if (_cachedCurrentEnvironment == null)
        {
            var environments = await GetEnvironmentsAsync();
            _cachedCurrentEnvironment = environments.FirstOrDefault();
        }

        return _cachedCurrentEnvironment;
    }

    public async Task SetCurrentEnvironmentAsync(string environmentId)
    {
        var environments = await GetEnvironmentsAsync();
        var environment = environments.FirstOrDefault(e => e.Id == environmentId);

        if (environment != null)
        {
            var currentEnvData = new CurrentEnvironmentData { EnvironmentId = environmentId };
            _fileService.Save(_environmentFolderPath, _currentEnvironmentFileName, currentEnvData);

            // 更新所有环境的激活状态
            foreach (var env in environments)
            {
                env.IsActive = env.Id == environmentId;
            }

            await SaveEnvironmentsAsync(environments);
            _cachedCurrentEnvironment = environment;
        }
    }

    public async Task AddEnvironmentAsync(ApiEnvironment environment)
    {
        var environments = await GetEnvironmentsAsync();
        environment.Id = Guid.NewGuid().ToString();
        environment.CreatedAt = DateTime.Now;
        environment.LastModified = DateTime.Now;

        environments.Add(environment);
        await SaveEnvironmentsAsync(environments);
    }

    public async Task UpdateEnvironmentAsync(ApiEnvironment environment)
    {
        var environments = await GetEnvironmentsAsync();
        var existingEnvironment = environments.FirstOrDefault(e => e.Id == environment.Id);

        if (existingEnvironment != null)
        {
            var index = environments.IndexOf(existingEnvironment);
            environment.LastModified = DateTime.Now;
            environments[index] = environment;

            await SaveEnvironmentsAsync(environments);

            // 如果更新的是当前环境，更新缓存
            if (_cachedCurrentEnvironment?.Id == environment.Id)
            {
                _cachedCurrentEnvironment = environment;
            }
        }
    }

    public async Task DeleteEnvironmentAsync(string environmentId)
    {
        var environments = await GetEnvironmentsAsync();
        var environment = environments.FirstOrDefault(e => e.Id == environmentId);

        if (environment != null)
        {
            environments.Remove(environment);
            await SaveEnvironmentsAsync(environments);

            // 如果删除的是当前环境，切换到第一个可用环境
            if (_cachedCurrentEnvironment?.Id == environmentId)
            {
                var firstEnvironment = environments.FirstOrDefault();
                if (firstEnvironment != null)
                {
                    await SetCurrentEnvironmentAsync(firstEnvironment.Id);
                }
                else
                {
                    _cachedCurrentEnvironment = null;
                }
            }
        }
    }

    public async Task<string> ResolveVariablesAsync(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        var currentEnvironment = await GetCurrentEnvironmentAsync();
        if (currentEnvironment == null)
        {
            return value;
        }

        var result = value;

        // 匹配 {{variable}} 格式的变量
        var regex = new Regex(@"\{\{(\w+)\}\}", RegexOptions.IgnoreCase);
        var matches = regex.Matches(value);

        foreach (Match match in matches)
        {
            var variableName = match.Groups[1].Value;
            var variableValue = GetVariableFromEnvironment(currentEnvironment, variableName);

            if (variableValue != null)
            {
                result = result.Replace(match.Value, variableValue);
            }
        }

        return result;
    }

    public async Task<string?> GetVariableAsync(string key)
    {
        var currentEnvironment = await GetCurrentEnvironmentAsync();
        return GetVariableFromEnvironment(currentEnvironment, key);
    }

    public async Task SetVariableAsync(string key, string value)
    {
        var currentEnvironment = await GetCurrentEnvironmentAsync();
        if (currentEnvironment != null)
        {
            currentEnvironment.Variables[key] = value;
            await UpdateEnvironmentAsync(currentEnvironment);
        }
    }

    private string? GetVariableFromEnvironment(ApiEnvironment? environment, string key)
    {
        if (environment == null)
        {
            return null;
        }

        // 首先查找环境变量
        if (environment.Variables.TryGetValue(key, out var value))
        {
            return value;
        }

        // 查找一些内置变量
        return key.ToLower() switch
        {
            "baseurl" => environment.BaseUrl,
            "timestamp" => DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
            "datetime" => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            "date" => DateTime.Now.ToString("yyyy-MM-dd"),
            "time" => DateTime.Now.ToString("HH:mm:ss"),
            "guid" => Guid.NewGuid().ToString(),
            "uuid" => Guid.NewGuid().ToString(),
            _ => null
        };
    }

    private async Task SaveEnvironmentsAsync(List<ApiEnvironment> environments)
    {
        await Task.Run(() =>
        {
            _fileService.Save(_environmentFolderPath, _environmentFileName, environments);
        });
        _cachedEnvironments = environments;
    }

    private async Task<List<ApiEnvironment>> CreateDefaultEnvironmentsAsync()
    {
        var environments = new List<ApiEnvironment>
        {
            new() {
                Id = Guid.NewGuid().ToString(),
                Name = "Development",
                BaseUrl = "http://localhost:3000",
                Description = "开发环境",
                IsActive = true,
                Variables = new Dictionary<string, string>
                {
                    ["host"] = "localhost",
                    ["port"] = "3000"
                }
            },
            new() {
                Id = Guid.NewGuid().ToString(),
                Name = "Production",
                BaseUrl = "https://api.example.com",
                Description = "生产环境",
                IsActive = false,
                Variables = new Dictionary<string, string>
                {
                    ["host"] = "api.example.com",
                    ["port"] = "443"
                }
            }
        };

        await SaveEnvironmentsAsync(environments);

        // 设置第一个环境为当前环境
        if (environments.Count != 0)
        {
            await SetCurrentEnvironmentAsync(environments.First().Id);
        }

        return environments;
    }

    private class CurrentEnvironmentData
    {
        public string? EnvironmentId
        {
            get; set;
        }
    }
}