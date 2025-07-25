using ApiClient.Core.Contracts.Services;
using ApiClient.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ApiClient.Core.Services;

/// <summary>
/// Swagger导入服务实现
/// </summary>
public class SwaggerImportService : ISwaggerImportService
{
    private readonly IHttpClientService _httpClientService;
    private JObject? _originalSwaggerJson;
    private readonly HashSet<string> _processingReferences = []; // 用于检测循环引用

    public SwaggerImportService(IHttpClientService httpClientService)
    {
        _httpClientService = httpClientService;
    }

    public async Task<ApiCollection> ImportFromUrlAsync(string swaggerUrl, string? collectionName = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // 获取Swagger文档
            var response = await _httpClientService.GetAsync(swaggerUrl, null, cancellationToken);

            if (!response.IsSuccess)
            {
                throw new InvalidOperationException($"无法获取Swagger文档: {response.ErrorMessage}");
            }

            if (string.IsNullOrWhiteSpace(response.Content))
            {
                throw new InvalidOperationException("Swagger文档内容为空");
            }

            var collection = await ImportFromJsonAsync(response.Content, collectionName);
            collection.SwaggerUrl = swaggerUrl;
            collection.ImportSource = ImportSource.SwaggerUrl;

            return collection;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"从URL导入Swagger文档失败: {ex.Message}", ex);
        }
    }

    public async Task<ApiCollection> ImportFromJsonAsync(string jsonContent, string? collectionName = null)
    {
        try
        {
            // 验证JSON格式
            var validationResult = await ValidateSwaggerAsync(jsonContent);
            if (!validationResult.IsValid)
            {
                throw new InvalidOperationException($"无效的Swagger文档: {validationResult.ErrorMessage}");
            }

            // 保存原始JSON以供后续使用
            _originalSwaggerJson = JObject.Parse(jsonContent);
            // 清空处理引用集合
            _processingReferences.Clear();

            // 解析Swagger文档 - 使用自定义设置处理$ref
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            var swaggerDoc = JsonConvert.DeserializeObject<SwaggerDocument>(jsonContent, settings);
            if (swaggerDoc == null)
            {
                throw new InvalidOperationException("无法解析Swagger文档");
            }

            // 创建API集合
            var collection = new ApiCollection
            {
                Name = collectionName ?? swaggerDoc.Info?.Title ?? "导入的API集合",
                Description = swaggerDoc.Info?.Description,
                Version = swaggerDoc.Info?.Version,
                ImportSource = ImportSource.JsonFile,
                BaseUrl = GetBaseUrl(swaggerDoc)
            };

            // 解析API接口
            if (swaggerDoc.Paths != null)
            {
                var folderMap = new Dictionary<string, ApiFolder>();

                foreach (var path in swaggerDoc.Paths)
                {
                    var pathOperations = GetOperationsFromPath(path.Value);

                    foreach (var operation in pathOperations)
                    {
                        try
                        {
                            var request = ConvertOperationToRequest(path.Key, operation.Key, operation.Value, swaggerDoc);

                            // 根据标签创建文件夹
                            var folderName = GetFolderName(operation.Value);
                            if (!string.IsNullOrWhiteSpace(folderName))
                            {
                                if (!folderMap.TryGetValue(folderName, out var value))
                                {
                                    var folder = new ApiFolder
                                    {
                                        Name = folderName,
                                        Description = $"来自标签: {folderName}"
                                    };
                                    value = folder;
                                    folderMap[folderName] = value;
                                    collection.Folders.Add(folder);
                                }

                                value.Requests.Add(request);
                            }
                            else
                            {
                                collection.Requests.Add(request);
                            }
                        }
                        catch (Exception ex)
                        {
                            // 记录单个操作的解析错误，但继续处理其他操作
                            System.Diagnostics.Debug.WriteLine($"解析操作 {operation.Key} {path.Key} 时出错: {ex.Message}");
                        }
                    }
                }
            }

            return collection;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"从JSON导入Swagger文档失败: {ex.Message}", ex);
        }
        finally
        {
            // 清理资源
            _processingReferences.Clear();
        }
    }

    public async Task<SwaggerValidationResult> ValidateSwaggerAsync(string jsonContent)
    {
        var result = new SwaggerValidationResult();

        try
        {
            var json = JObject.Parse(jsonContent);

            // 检查Swagger版本
            var swaggerVersion = json["swagger"]?.ToString();
            var openApiVersion = json["openapi"]?.ToString();

            if (!string.IsNullOrEmpty(openApiVersion))
            {
                result.DetectedVersion = $"OpenAPI {openApiVersion}";
                if (!openApiVersion.StartsWith("3."))
                {
                    result.Warnings.Add("建议使用OpenAPI 3.0+版本");
                }
            }
            else if (!string.IsNullOrEmpty(swaggerVersion))
            {
                result.DetectedVersion = $"Swagger {swaggerVersion}";
                if (!swaggerVersion.StartsWith("2."))
                {
                    result.Warnings.Add("检测到非标准Swagger版本");
                }
            }
            else
            {
                result.ErrorMessage = "无法识别的API文档格式，缺少版本信息";
                return result;
            }

            // 检查基本结构
            var info = json["info"];
            if (info == null)
            {
                result.ErrorMessage = "缺少必需的'info'字段";
                return result;
            }

            var title = info["title"]?.ToString();
            if (string.IsNullOrWhiteSpace(title))
            {
                result.Warnings.Add("建议提供API标题");
            }

            if (json["paths"] is not JObject paths || !paths.HasValues)
            {
                result.Warnings.Add("未找到API路径定义");
                result.ApiCount = 0;
            }
            else
            {
                result.ApiCount = CountApiOperations(paths);
            }

            result.IsValid = true;
        }
        catch (JsonException ex)
        {
            result.ErrorMessage = $"JSON格式错误: {ex.Message}";
        }
        catch (Exception ex)
        {
            result.ErrorMessage = $"验证失败: {ex.Message}";
        }

        return result;
    }

    public async Task<UrlValidationResult> ValidateUrlAsync(string url, CancellationToken cancellationToken = default)
    {
        var result = new UrlValidationResult();

        try
        {
            var response = await _httpClientService.GetAsync(url, null, cancellationToken);

            result.IsAccessible = response.IsSuccess;
            result.StatusCode = response.StatusCode;
            result.ResponseTimeMs = response.ResponseTimeMs;
            result.ContentType = response.ContentType;
            result.ContentLength = response.ContentLength;

            if (!response.IsSuccess)
            {
                result.ErrorMessage = response.ErrorMessage;
            }
            else if (string.IsNullOrWhiteSpace(response.Content))
            {
                result.ErrorMessage = "响应内容为空";
                result.IsAccessible = false;
            }
            else
            {
                // 检查内容是否为有效的JSON
                try
                {
                    JObject.Parse(response.Content);
                }
                catch
                {
                    result.ErrorMessage = "响应内容不是有效的JSON格式";
                    result.IsAccessible = false;
                }
            }
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.Message;
            result.IsAccessible = false;
        }

        return result;
    }

    public string GenerateExampleFromSchema(Schema schema)
    {
        try
        {
            // 清空处理引用集合
            _processingReferences.Clear();
            var example = GenerateExampleObject(schema);
            return JsonConvert.SerializeObject(example, Formatting.Indented);
        }
        catch
        {
            return "{}";
        }
        finally
        {
            _processingReferences.Clear();
        }
    }

    public (Dictionary<string, string> headers, Dictionary<string, string> queryParams) ParseParameters(List<Parameter> parameters)
    {
        var headers = new Dictionary<string, string>();
        var queryParams = new Dictionary<string, string>();

        foreach (var param in parameters)
        {
            var exampleValue = GetParameterExample(param);

            switch (param.In?.ToLower())
            {
                case "header":
                    if (!string.IsNullOrWhiteSpace(param.Name))
                    {
                        headers[param.Name] = exampleValue;
                    }
                    break;
                case "query":
                    if (!string.IsNullOrWhiteSpace(param.Name))
                    {
                        queryParams[param.Name] = exampleValue;
                    }
                    break;
            }
        }

        return (headers, queryParams);
    }

    #region Private Methods

    private string GetBaseUrl(SwaggerDocument swaggerDoc)
    {
        // OpenAPI 3.0
        if (swaggerDoc.Servers?.Count > 0)
        {
            return swaggerDoc.Servers.First().Url ?? string.Empty;
        }

        // Swagger 2.0
        if (!string.IsNullOrWhiteSpace(swaggerDoc.Host))
        {
            var scheme = swaggerDoc.Schemes?.FirstOrDefault() ?? "https";
            var basePath = swaggerDoc.BasePath ?? string.Empty;
            return $"{scheme}://{swaggerDoc.Host}{basePath}";
        }

        return string.Empty;
    }

    private Dictionary<string, Operation> GetOperationsFromPath(PathItem pathItem)
    {
        // 使用反射和元组来简化HTTP方法映射
        var httpMethods = new[]
        {
            ("GET", pathItem.Get),
            ("POST", pathItem.Post),
            ("PUT", pathItem.Put),
            ("DELETE", pathItem.Delete),
            ("PATCH", pathItem.Patch),
            ("HEAD", pathItem.Head),
            ("OPTIONS", pathItem.Options)
        };

        return httpMethods
            .Where(method => method.Item2 != null)
            .ToDictionary(method => method.Item1, method => method.Item2!);
    }

    private static string GetFolderName(Operation operation) => operation.Tags?.FirstOrDefault() ?? string.Empty;

    private RequestRecord ConvertOperationToRequest(string path, string method, Operation operation, SwaggerDocument swaggerDoc)
    {
        var request = new RequestRecord
        {
            Name = operation.Summary ?? $"{method} {path}",
            Method = method.ToUpper(),
            Url = $"{GetBaseUrl(swaggerDoc)}{path}",
            Description = operation.Description,
            ContentType = GetContentType(operation, method)
        };

        // 处理参数
        if (operation.Parameters?.Count > 0)
        {
            var (headers, queryParams) = ParseParameters(operation.Parameters);
            request.Headers = headers;

            // 将查询参数添加到URL
            if (queryParams.Count != 0)
            {
                var queryString = string.Join("&", queryParams.Select(kv => $"{kv.Key}={kv.Value}"));
                request.Url += $"?{queryString}";
            }
        }

        // 处理请求体和Schema
        if (operation.RequestBody != null)
        {
            try
            {
                // 清空处理引用集合，避免跨请求的循环引用检测
                _processingReferences.Clear();
                var exampleBody = GenerateRequestBodyExample(operation.RequestBody, path, method.ToLower());
                if (!string.IsNullOrWhiteSpace(exampleBody))
                {
                    request.Body = exampleBody;
                }

                // 保存Schema信息以便后续生成示例数据
                if (operation.RequestBody.Content != null)
                {
                    var jsonContent = operation.RequestBody.Content.FirstOrDefault(c =>
                        c.Key.Contains("json", StringComparison.OrdinalIgnoreCase));

                    if (!string.IsNullOrEmpty(jsonContent.Key) && jsonContent.Value?.Schema != null)
                    {
                        request.RequestBodySchema = jsonContent.Value.Schema;
                    }
                }
            }
            catch (Exception ex)
            {
                // 如果生成示例失败，记录错误但不影响其他处理
                System.Diagnostics.Debug.WriteLine($"生成请求体示例失败: {ex.Message}");
                request.Body = "{}";
            }
            finally
            {
                _processingReferences.Clear();
            }
        }

        // 添加标签
        if (operation.Tags?.Count > 0)
        {
            request.Tags = [.. operation.Tags];
        }

        return request;
    }

    private static string GetContentType(Operation operation, string method)
    {
        // OpenAPI 3.0
        if (operation.RequestBody?.Content != null)
        {
            return operation.RequestBody.Content.Keys.FirstOrDefault() ?? "application/json";
        }

        // Swagger 2.0
        if (operation.Consumes?.Count > 0)
        {
            return operation.Consumes.First();
        }

        // 默认值
        return method.Equals("GET", StringComparison.CurrentCultureIgnoreCase) ? "application/json" : "application/json";
    }

    /// <summary>
    /// 生成请求体示例 - 重写版本，直接从原始JSON解析Schema引用，防止循环引用
    /// </summary>
    private string GenerateRequestBodyExample(RequestBody requestBody, string path, string method)
    {
        if (requestBody.Content == null || _originalSwaggerJson == null) return "{}";

        try
        {
            // 从原始JSON中获取对应路径的schema信息
            var pathsJson = _originalSwaggerJson["paths"] as JObject;
            var pathJson = pathsJson?[path] as JObject;
            var operationJson = pathJson?[method] as JObject;
            var requestBodyJson = operationJson?["requestBody"] as JObject;

            if (requestBodyJson?["content"] is not JObject contentJson) return "{}";

            // 查找JSON类型的内容
            var jsonContentKey = contentJson.Properties()
                .FirstOrDefault(p => p.Name.Contains("json", StringComparison.OrdinalIgnoreCase))?.Name;

            if (string.IsNullOrEmpty(jsonContentKey)) return "{}";

            var mediaTypeJson = contentJson[jsonContentKey] as JObject;

            if (mediaTypeJson?["schema"] is not JObject schemaJson) return "{}";

            // 从schema JSON生成示例，增加递归深度限制
            var example = GenerateExampleFromSchemaJson(schemaJson, 0, 10);
            return JsonConvert.SerializeObject(example, Formatting.Indented);
        }
        catch (Exception)
        {
            return "{}";
        }
    }

    /// <summary>
    /// 从Schema JSON对象生成示例数据，防止循环引用和堆栈溢出
    /// </summary>
    private object GenerateExampleFromSchemaJson(JObject schemaJson, int depth = 0, int maxDepth = 10)
    {
        // 限制递归深度，防止堆栈溢出
        if (depth > maxDepth)
        {
            return new Dictionary<string, object> { ["_depth_limit_reached"] = maxDepth };
        }

        // 检查是否有引用
        if (schemaJson["$ref"]?.ToString() is string refValue)
        {
            return ResolveSchemaReferenceFromJson(refValue, depth + 1, maxDepth);
        }

        // 检查类型
        var type = schemaJson["type"]?.ToString();

        switch (type?.ToLower())
        {
            case "object":
                return GenerateObjectExample(schemaJson, depth, maxDepth);

            case "array":
                return GenerateArrayExample(schemaJson, depth, maxDepth);

            case "string":
                return GenerateStringExample(schemaJson);

            case "integer":
                return GenerateIntegerExample(schemaJson);

            case "number":
                return GenerateNumberExample(schemaJson);

            case "boolean":
                return true;

            case null:
                // 如果没有type但有properties，当作object处理
                if (schemaJson["properties"] != null)
                {
                    return GenerateObjectExample(schemaJson, depth, maxDepth);
                }
                return "example";

            default:
                return "example";
        }
    }

    /// <summary>
    /// 生成对象示例，防止循环引用
    /// </summary>
    private object GenerateObjectExample(JObject schemaJson, int depth, int maxDepth)
    {
        var result = new Dictionary<string, object>();

        if (schemaJson["properties"] is JObject propertiesJson && depth < maxDepth)
        {
            foreach (var property in propertiesJson.Properties())
            {
                if (property.Value is JObject propSchemaJson)
                {
                    result[property.Name] = GenerateExampleFromSchemaJson(propSchemaJson, depth + 1, maxDepth);
                }
            }
        }
        else if (depth >= maxDepth)
        {
            return new Dictionary<string, object> { ["_depth_limit_reached"] = maxDepth };
        }

        return result.Count > 0 ? result : new Dictionary<string, object> { ["example"] = "value" };
    }

    /// <summary>
    /// 生成数组示例，防止循环引用
    /// </summary>
    private object GenerateArrayExample(JObject schemaJson, int depth, int maxDepth)
    {
        if (schemaJson["items"] is JObject itemsJson && depth < maxDepth)
        {
            var itemExample = GenerateExampleFromSchemaJson(itemsJson, depth + 1, maxDepth);
            return new[] { itemExample };
        }
        else if (depth >= maxDepth)
        {
            return new object[] { new Dictionary<string, object> { ["_depth_limit_reached"] = maxDepth } };
        }

        return new object[] { new Dictionary<string, object> { ["example"] = "value" } };
    }

    /// <summary>
    /// 生成字符串示例
    /// </summary>
    private object GenerateStringExample(JObject schemaJson)
    {
        var format = schemaJson["format"]?.ToString()?.ToLower();
        return format switch
        {
            "email" => "user@example.com",
            "date" => DateTime.Now.ToString("yyyy-MM-dd"),
            "date-time" => DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            "uuid" => Guid.NewGuid().ToString(),
            "uri" => "https://example.com",
            "password" => "password123",
            _ => "string"
        };
    }

    /// <summary>
    /// 生成整数示例
    /// </summary>
    private object GenerateIntegerExample(JObject schemaJson)
    {
        var format = schemaJson["format"]?.ToString()?.ToLower();
        return format switch
        {
            "int64" => 1234567890L,
            _ => 123
        };
    }

    /// <summary>
    /// 生成数字示例
    /// </summary>
    private object GenerateNumberExample(JObject schemaJson)
    {
        var format = schemaJson["format"]?.ToString()?.ToLower();
        return format switch
        {
            "float" => 123.45f,
            "double" => 123.45,
            _ => 123.45
        };
    }

    /// <summary>
    /// 从原始JSON解析Schema引用，防止循环引用
    /// </summary>
    private object ResolveSchemaReferenceFromJson(string reference, int depth = 0, int maxDepth = 10)
    {
        if (_originalSwaggerJson == null || !reference.StartsWith("#/") || depth > maxDepth)
        {
            return depth > maxDepth
                ? new Dictionary<string, object> { ["_depth_limit_reached"] = maxDepth }
                : new Dictionary<string, object> { ["error"] = $"无法解析引用: {reference}" };
        }

        // 检查循环引用
        if (_processingReferences.Contains(reference))
        {
            return new Dictionary<string, object>
            {
                ["_circular_reference"] = reference.Split('/').LastOrDefault() ?? "unknown"
            };
        }

        try
        {
            // 标记当前正在处理的引用
            _processingReferences.Add(reference);

            var parts = reference.Substring(2).Split('/');

            if (parts.Length >= 3 && parts[0] == "components" && parts[1] == "schemas")
            {
                var schemaName = parts[2];
                var componentsJson = _originalSwaggerJson["components"] as JObject;
                var schemasJson = componentsJson?["schemas"] as JObject;

                if (schemasJson?[schemaName] is JObject schemaJson)
                {
                    return GenerateExampleFromSchemaJson(schemaJson, depth + 1, maxDepth);
                }
            }
            else if (parts.Length >= 2 && parts[0] == "definitions")
            {
                // Swagger 2.0格式
                var schemaName = parts[1];
                var definitionsJson = _originalSwaggerJson["definitions"] as JObject;

                if (definitionsJson?[schemaName] is JObject schemaJson)
                {
                    return GenerateExampleFromSchemaJson(schemaJson, depth + 1, maxDepth);
                }
            }

            return new Dictionary<string, object> { ["error"] = $"无法找到引用: {reference}" };
        }
        catch (Exception ex)
        {
            return new Dictionary<string, object> { ["error"] = $"解析引用失败: {ex.Message}" };
        }
        finally
        {
            // 移除当前处理的引用标记
            _processingReferences.Remove(reference);
        }
    }

    /// <summary>
    /// 从Schema对象生成示例 - 保留原有方法供其他地方使用，防止循环引用
    /// </summary>
    private object GenerateExampleObject(Schema schema, int depth = 0, int maxDepth = 10)
    {
        if (schema == null)
        {
            return "null";
        }

        if (depth > maxDepth)
        {
            return new Dictionary<string, object> { ["_depth_limit_reached"] = maxDepth };
        }

        if (schema.Example != null)
        {
            return schema.Example;
        }

        // 处理引用类型
        if (!string.IsNullOrEmpty(schema.Ref))
        {
            return ResolveSchemaReferenceFromJson(schema.Ref, depth + 1, maxDepth);
        }

        switch (schema.Type?.ToLower())
        {
            case "object":
                var obj = new Dictionary<string, object>();
                if (schema.Properties != null && depth < maxDepth)
                {
                    foreach (var prop in schema.Properties)
                    {
                        if (prop.Value != null)
                        {
                            obj[prop.Key] = GenerateExampleObject(prop.Value, depth + 1, maxDepth);
                        }
                    }
                }
                else if (depth >= maxDepth)
                {
                    return new Dictionary<string, object> { ["_depth_limit_reached"] = maxDepth };
                }
                return obj.Count > 0 ? obj : new Dictionary<string, object> { ["example"] = "value" };

            case "array":
                if (schema.Items != null && depth < maxDepth)
                {
                    var itemExample = GenerateExampleObject(schema.Items, depth + 1, maxDepth);
                    return new[] { itemExample };
                }
                else if (depth >= maxDepth)
                {
                    return new object[] { new Dictionary<string, object> { ["_depth_limit_reached"] = maxDepth } };
                }
                return new object[] { new Dictionary<string, object> { ["example"] = "value" } };

            case "string":
                return schema.Format?.ToLower() switch
                {
                    "email" => "user@example.com",
                    "date" => DateTime.Now.ToString("yyyy-MM-dd"),
                    "date-time" => DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    "uuid" => Guid.NewGuid().ToString(),
                    "uri" => "https://example.com",
                    "password" => "password123",
                    _ => "string"
                };

            case "integer":
                return schema.Format?.ToLower() switch
                {
                    "int64" => 1234567890L,
                    _ => 123
                };

            case "number":
                return schema.Format?.ToLower() switch
                {
                    "float" => 123.45f,
                    "double" => 123.45,
                    _ => 123.45
                };

            case "boolean":
                return true;

            case null when schema.Properties != null:
                var objWithoutType = new Dictionary<string, object>();
                if (depth < maxDepth)
                {
                    foreach (var prop in schema.Properties)
                    {
                        if (prop.Value != null)
                        {
                            objWithoutType[prop.Key] = GenerateExampleObject(prop.Value, depth + 1, maxDepth);
                        }
                    }
                }
                else if (depth >= maxDepth)
                {
                    return new Dictionary<string, object> { ["_depth_limit_reached"] = maxDepth };
                }
                return objWithoutType;

            default:
                return "example";
        }
    }

    private string GetParameterExample(Parameter parameter)
    {
        if (parameter.Example != null)
        {
            return parameter.Example.ToString() ?? string.Empty;
        }

        if (parameter.Schema != null)
        {
            try
            {
                _processingReferences.Clear();
                var example = GenerateExampleObject(parameter.Schema, 0, 5); // 参数示例限制较小的深度
                return example.ToString() ?? string.Empty;
            }
            catch
            {
                return "example";
            }
            finally
            {
                _processingReferences.Clear();
            }
        }

        return parameter.Type?.ToLower() switch
        {
            "string" => "example",
            "integer" => "123",
            "number" => "123.45",
            "boolean" => "true",
            _ => "example"
        };
    }

    private int CountApiOperations(JObject paths)
    {
        var count = 0;
        foreach (var path in paths)
        {
            if (path.Value is JObject pathObject)
            {
                var httpMethods = new[] { "get", "post", "put", "delete", "patch", "head", "options" };
                count += httpMethods.Count(method => pathObject[method] != null);
            }
        }
        return count;
    }

    #endregion
}