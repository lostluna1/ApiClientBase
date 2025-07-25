using System.Diagnostics;
using System.Text;
using ApiClient.Core.Contracts.Services;

namespace ApiClient.Core.Services;

/// <summary>
/// HTTP客户端服务实现
/// </summary>
public class HttpClientService : IHttpClientService, IDisposable
{
    private readonly HttpClient _httpClient;
    private bool _disposed;

    public HttpClientService()
    {
        var handler = new HttpClientHandler()
        {
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
        };

        _httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(120)  // 增加超时时间到120秒，避免连接超时
        };
    }

    public async Task<HttpResponseResult> GetAsync(string url, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        return await SendAsync(HttpMethod.Get, url, null, headers, cancellationToken);
    }

    public async Task<HttpResponseResult> PostAsync(string url, string? content = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        return await SendAsync(HttpMethod.Post, url, content, headers, cancellationToken);
    }

    public async Task<HttpResponseResult> PutAsync(string url, string? content = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        return await SendAsync(HttpMethod.Put, url, content, headers, cancellationToken);
    }

    public async Task<HttpResponseResult> DeleteAsync(string url, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        return await SendAsync(HttpMethod.Delete, url, null, headers, cancellationToken);
    }

    public async Task<HttpResponseResult> PatchAsync(string url, string? content = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        return await SendAsync(HttpMethod.Patch, url, content, headers, cancellationToken);
    }

    public async Task<HttpResponseResult> SendAsync(HttpMethod method, string url, string? content = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        var result = new HttpResponseResult
        {
            RequestUrl = url,
            HttpMethod = method.Method,
            RequestTime = DateTime.Now
        };

        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var request = new HttpRequestMessage(method, url);

            // 添加调试日志
            System.Diagnostics.Debug.WriteLine($"[HttpClient] 发送请求: {method} {url}");
            System.Diagnostics.Debug.WriteLine($"[HttpClient] 超时设置: {_httpClient.Timeout.TotalSeconds}秒");

            // 添加请求头
            if (headers != null)
            {
                System.Diagnostics.Debug.WriteLine($"[HttpClient] 请求头数量: {headers.Count}");
                foreach (var header in headers)
                {
                    if (IsContentHeader(header.Key))
                    {
                        // 内容相关的头部需要设置到Content上
                        continue;
                    }
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            // 添加请求内容
            if (!string.IsNullOrEmpty(content) && (method == HttpMethod.Post || method == HttpMethod.Put || method == HttpMethod.Patch))
            {
                // 获取Content-Type，默认为application/json
                var contentType = "application/json";
                if (headers != null && headers.TryGetValue("Content-Type", out var headerContentType))
                {
                    contentType = headerContentType;
                }

                // 创建StringContent时直接指定Content-Type
                request.Content = new StringContent(content, Encoding.UTF8, contentType);

                // 设置其他内容相关的头部（排除Content-Type）
                if (headers != null)
                {
                    foreach (var header in headers.Where(h => IsContentHeader(h.Key) && !string.Equals(h.Key, "Content-Type", StringComparison.OrdinalIgnoreCase)))
                    {
                        request.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }
            }

            // 发送请求
            using var response = await _httpClient.SendAsync(request, cancellationToken);

            stopwatch.Stop();
            result.ResponseTime = DateTime.Now;
            result.ResponseTimeMs = stopwatch.ElapsedMilliseconds;

            // 读取响应
            result.StatusCode = (int)response.StatusCode;
            result.IsSuccess = response.IsSuccessStatusCode;
            result.Content = await response.Content.ReadAsStringAsync(cancellationToken);
            result.ContentType = response.Content.Headers.ContentType?.ToString();
            result.ContentLength = response.Content.Headers.ContentLength ?? 0;

            // 读取响应头
            foreach (var header in response.Headers.Concat(response.Content.Headers))
            {
                result.Headers[header.Key] = string.Join(", ", header.Value);
            }

            if (!result.IsSuccess)
            {
                result.ErrorMessage = $"HTTP {result.StatusCode}: {response.ReasonPhrase}";
            }
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException || !cancellationToken.IsCancellationRequested)
        {
            stopwatch.Stop();
            result.ResponseTime = DateTime.Now;
            result.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
            result.IsSuccess = false;
            result.ErrorMessage = $"请求超时 (耗时: {stopwatch.ElapsedMilliseconds}ms, 超时设置: {_httpClient.Timeout.TotalSeconds}s)";
            result.Exception = ex;
            
            System.Diagnostics.Debug.WriteLine($"[HttpClient] 请求超时: {url}, 耗时: {stopwatch.ElapsedMilliseconds}ms");
        }
        catch (TaskCanceledException ex) when (cancellationToken.IsCancellationRequested)
        {
            stopwatch.Stop();
            result.ResponseTime = DateTime.Now;
            result.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
            result.IsSuccess = false;
            result.ErrorMessage = "请求被取消";
            result.Exception = ex;
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            result.ResponseTime = DateTime.Now;
            result.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
            result.Exception = ex;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.ResponseTime = DateTime.Now;
            result.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
            result.IsSuccess = false;
            result.ErrorMessage = $"发生错误: {ex.Message}";
            result.Exception = ex;
        }

        return result;
    }

    public void SetTimeout(TimeSpan timeout)
    {
        _httpClient.Timeout = timeout;
    }

    public void SetDefaultHeaders(Dictionary<string, string> headers)
    {
        _httpClient.DefaultRequestHeaders.Clear();
        foreach (var header in headers)
        {
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
        }
    }

    public void ClearDefaultHeaders()
    {
        _httpClient.DefaultRequestHeaders.Clear();
    }

    private static bool IsContentHeader(string headerName)
    {
        var contentHeaders = new[]
        {
            "Content-Type", "Content-Length", "Content-Encoding", "Content-Language",
            "Content-Location", "Content-MD5", "Content-Range", "Content-Disposition"
        };

        return contentHeaders.Contains(headerName, StringComparer.OrdinalIgnoreCase);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _httpClient?.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}