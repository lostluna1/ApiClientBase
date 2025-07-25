using ApiClient.Core.Contracts.Services;
using ApiClient.Core.Models;

namespace ApiClient.Core.Services;

/// <summary>
/// 请求历史服务实现
/// </summary>
public class RequestHistoryService : IRequestHistoryService
{
    private readonly IFileService _fileService;
    private readonly string _historyFolderPath;
    private readonly string _historyFileName = "request_history.json";
    private List<RequestRecord>? _cachedHistory;

    public RequestHistoryService(IFileService fileService)
    {
        _fileService = fileService;
        _historyFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ApiClient", "History");
    }

    public async Task SaveRequestAsync(RequestRecord request)
    {
        var history = await GetRequestHistoryAsync();

        // 查找是否已存在相同ID的请求
        var existingRequest = history.FirstOrDefault(r => r.Id == request.Id);
        if (existingRequest != null)
        {
            // 更新现有请求
            var index = history.IndexOf(existingRequest);
            request.LastModified = DateTime.Now;
            history[index] = request;
        }
        else
        {
            // 添加新请求
            request.CreatedAt = DateTime.Now;
            request.LastModified = DateTime.Now;
            history.Insert(0, request); // 插入到开头，最新的在前面
        }

        // 限制历史记录数量，保留最近的1000条
        if (history.Count > 1000)
        {
            history = [.. history.Take(1000)];
        }

        await SaveHistoryToFileAsync(history);
        _cachedHistory = history;
    }

    public async Task<List<RequestRecord>> GetRequestHistoryAsync()
    {
        if (_cachedHistory != null)
        {
            return _cachedHistory;
        }

        try
        {
            var history = _fileService.Read<List<RequestRecord>>(_historyFolderPath, _historyFileName);
            _cachedHistory = history ?? [];
        }
        catch
        {
            _cachedHistory = [];
        }

        return _cachedHistory;
    }

    public async Task<RequestRecord?> GetRequestByIdAsync(string id)
    {
        var history = await GetRequestHistoryAsync();
        return history.FirstOrDefault(r => r.Id == id);
    }

    public async Task DeleteRequestAsync(string id)
    {
        var history = await GetRequestHistoryAsync();
        var request = history.FirstOrDefault(r => r.Id == id);
        if (request != null)
        {
            history.Remove(request);
            await SaveHistoryToFileAsync(history);
            _cachedHistory = history;
        }
    }

    public async Task ClearHistoryAsync()
    {
        var emptyHistory = new List<RequestRecord>();
        await SaveHistoryToFileAsync(emptyHistory);
        _cachedHistory = emptyHistory;
    }

    public async Task<List<RequestRecord>> SearchRequestsAsync(string keyword)
    {
        var history = await GetRequestHistoryAsync();

        if (string.IsNullOrWhiteSpace(keyword))
        {
            return history;
        }

        var lowerKeyword = keyword.ToLower();
        return [.. history.Where(r =>
            r.Name.ToLower().Contains(lowerKeyword) ||
            r.Url.ToLower().Contains(lowerKeyword) ||
            r.Method.ToLower().Contains(lowerKeyword) ||
            (r.Description?.ToLower().Contains(lowerKeyword) ?? false) ||
            r.Tags.Any(tag => tag.ToLower().Contains(lowerKeyword)) ||
            (r.Group?.ToLower().Contains(lowerKeyword) ?? false)
        )];
    }

    private async Task SaveHistoryToFileAsync(List<RequestRecord> history)
    {
        await Task.Run(() =>
        {
            _fileService.Save(_historyFolderPath, _historyFileName, history);
        });
    }
}