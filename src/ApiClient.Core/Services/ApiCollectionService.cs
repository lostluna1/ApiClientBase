using ApiClient.Core.Contracts.Services;
using ApiClient.Core.Models;

namespace ApiClient.Core.Services;

/// <summary>
/// API集合管理服务实现
/// </summary>
public class ApiCollectionService : IApiCollectionService
{
    private readonly IFileService _fileService;
    private readonly string _collectionFolderPath;
    private readonly string _collectionsFileName = "api_collections.json";
    private List<ApiCollection>? _cachedCollections;

    public ApiCollectionService(IFileService fileService)
    {
        _fileService = fileService;
        _collectionFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ApiClient", "Collections");
    }

    public async Task<List<ApiCollection>> GetCollectionsAsync()
    {
        if (_cachedCollections != null)
        {
            return _cachedCollections;
        }

        try
        {
            var collections = _fileService.Read<List<ApiCollection>>(_collectionFolderPath, _collectionsFileName);
            _cachedCollections = collections ?? [];
        }
        catch
        {
            _cachedCollections = [];
        }

        return _cachedCollections;
    }

    public async Task<ApiCollection?> GetCollectionByIdAsync(string collectionId)
    {
        var collections = await GetCollectionsAsync();
        return collections.FirstOrDefault(c => c.Id == collectionId);
    }

    public async Task SaveCollectionAsync(ApiCollection collection)
    {
        var collections = await GetCollectionsAsync();

        collection.CreatedAt = DateTime.Now;
        collection.LastModified = DateTime.Now;

        collections.Add(collection);
        await SaveCollectionsAsync(collections);
    }

    public async Task DeleteCollectionAsync(string collectionId)
    {
        var collections = await GetCollectionsAsync();
        var collection = collections.FirstOrDefault(c => c.Id == collectionId);

        if (collection != null)
        {
            collections.Remove(collection);
            await SaveCollectionsAsync(collections);
        }
    }

    public async Task UpdateCollectionAsync(ApiCollection collection)
    {
        var collections = await GetCollectionsAsync();
        var existingCollection = collections.FirstOrDefault(c => c.Id == collection.Id);

        if (existingCollection != null)
        {
            var index = collections.IndexOf(existingCollection);
            collection.LastModified = DateTime.Now;
            collections[index] = collection;
            await SaveCollectionsAsync(collections);
        }
    }

    public async Task<ApiCollection> DuplicateCollectionAsync(string collectionId, string newName)
    {
        var sourceCollection = await GetCollectionByIdAsync(collectionId);
        if (sourceCollection == null)
        {
            throw new ArgumentException($"集合ID '{collectionId}' 不存在");
        }

        var duplicatedCollection = new ApiCollection
        {
            Id = Guid.NewGuid().ToString(),
            Name = newName,
            Description = sourceCollection.Description,
            Version = sourceCollection.Version,
            BaseUrl = sourceCollection.BaseUrl,
            Variables = new Dictionary<string, string>(sourceCollection.Variables),
            Authentication = CloneAuthentication(sourceCollection.Authentication),
            ImportSource = ImportSource.Manual,
            Tags = [.. sourceCollection.Tags],
            Requests = CloneRequests(sourceCollection.Requests),
            Folders = CloneFolders(sourceCollection.Folders)
        };

        await SaveCollectionAsync(duplicatedCollection);
        return duplicatedCollection;
    }

    public async Task<List<ApiCollection>> SearchCollectionsAsync(string keyword)
    {
        var collections = await GetCollectionsAsync();

        if (string.IsNullOrWhiteSpace(keyword))
        {
            return collections;
        }

        var lowerKeyword = keyword.ToLower();
        return collections.Where(c =>
            c.Name.ToLower().Contains(lowerKeyword) ||
            (c.Description?.ToLower().Contains(lowerKeyword) ?? false) ||
            c.Tags.Any(tag => tag.ToLower().Contains(lowerKeyword)) ||
            c.Requests.Any(r => r.Name.ToLower().Contains(lowerKeyword) || r.Url.ToLower().Contains(lowerKeyword))
        ).ToList();
    }

    public async Task AddRequestToCollectionAsync(string collectionId, RequestRecord request, string? folderId = null)
    {
        var collection = await GetCollectionByIdAsync(collectionId);
        if (collection == null)
        {
            throw new ArgumentException($"集合ID '{collectionId}' 不存在");
        }

        if (!string.IsNullOrEmpty(folderId))
        {
            var folder = FindFolder(collection.Folders, folderId);
            if (folder == null)
            {
                throw new ArgumentException($"文件夹ID '{folderId}' 不存在");
            }
            folder.Requests.Add(request);
        }
        else
        {
            collection.Requests.Add(request);
        }

        await UpdateCollectionAsync(collection);
    }

    public async Task RemoveRequestFromCollectionAsync(string collectionId, string requestId)
    {
        var collection = await GetCollectionByIdAsync(collectionId);
        if (collection == null)
        {
            throw new ArgumentException($"集合ID '{collectionId}' 不存在");
        }

        // 从根级别移除
        var request = collection.Requests.FirstOrDefault(r => r.Id == requestId);
        if (request != null)
        {
            collection.Requests.Remove(request);
        }
        else
        {
            // 从文件夹中移除
            RemoveRequestFromFolders(collection.Folders, requestId);
        }

        await UpdateCollectionAsync(collection);
    }

    public async Task<ApiFolder> CreateFolderAsync(string collectionId, string folderName, string? parentFolderId = null)
    {
        var collection = await GetCollectionByIdAsync(collectionId);
        if (collection == null)
        {
            throw new ArgumentException($"集合ID '{collectionId}' 不存在");
        }

        var newFolder = new ApiFolder
        {
            Name = folderName,
            ParentId = parentFolderId
        };

        if (string.IsNullOrEmpty(parentFolderId))
        {
            collection.Folders.Add(newFolder);
        }
        else
        {
            var parentFolder = FindFolder(collection.Folders, parentFolderId);
            if (parentFolder == null)
            {
                throw new ArgumentException($"父文件夹ID '{parentFolderId}' 不存在");
            }
            parentFolder.SubFolders.Add(newFolder);
        }

        await UpdateCollectionAsync(collection);
        return newFolder;
    }

    public async Task DeleteFolderAsync(string collectionId, string folderId)
    {
        var collection = await GetCollectionByIdAsync(collectionId);
        if (collection == null)
        {
            throw new ArgumentException($"集合ID '{collectionId}' 不存在");
        }

        RemoveFolder(collection.Folders, folderId);
        await UpdateCollectionAsync(collection);
    }

    public async Task RenameFolderAsync(string collectionId, string folderId, string newName)
    {
        var collection = await GetCollectionByIdAsync(collectionId);
        if (collection == null)
        {
            throw new ArgumentException($"集合ID '{collectionId}' 不存在");
        }

        var folder = FindFolder(collection.Folders, folderId);
        if (folder == null)
        {
            throw new ArgumentException($"文件夹ID '{folderId}' 不存在");
        }

        folder.Name = newName;
        await UpdateCollectionAsync(collection);
    }

    public async Task<string> ExportCollectionAsync(string collectionId)
    {
        var collection = await GetCollectionByIdAsync(collectionId);
        return collection == null
            ? throw new ArgumentException($"集合ID '{collectionId}' 不存在")
            : Newtonsoft.Json.JsonConvert.SerializeObject(collection, Newtonsoft.Json.Formatting.Indented);
    }

    public async Task<CollectionStatistics> GetCollectionStatisticsAsync(string collectionId)
    {
        var collection = await GetCollectionByIdAsync(collectionId);
        if (collection == null)
        {
            throw new ArgumentException($"集合ID '{collectionId}' 不存在");
        }

        var allRequests = GetAllRequestsFromCollection(collection);

        var statistics = new CollectionStatistics
        {
            TotalRequests = allRequests.Count,
            TotalFolders = CountFolders(collection.Folders),
            LastUpdated = collection.LastModified,
            Created = collection.CreatedAt,
            // 按HTTP方法统计
            RequestsByMethod = allRequests
                .GroupBy(r => r.Method.ToUpper())
                .ToDictionary(g => g.Key, g => g.Count())
        };

        // 按标签统计
        var allTags = allRequests.SelectMany(r => r.Tags).Where(tag => !string.IsNullOrWhiteSpace(tag));
        statistics.RequestsByTag = allTags
            .GroupBy(tag => tag)
            .ToDictionary(g => g.Key, g => g.Count());

        return statistics;
    }

    #region Private Methods

    private async Task SaveCollectionsAsync(List<ApiCollection> collections)
    {
        await Task.Run(() =>
        {
            _fileService.Save(_collectionFolderPath, _collectionsFileName, collections);
        });
        _cachedCollections = collections;
    }

    private AuthenticationInfo? CloneAuthentication(AuthenticationInfo? auth)
    {
        return auth == null
            ? null
            : new AuthenticationInfo
            {
                Type = auth.Type,
                ApiKey = auth.ApiKey != null ? new ApiKeyAuth
                {
                    Key = auth.ApiKey.Key,
                    Value = auth.ApiKey.Value,
                    In = auth.ApiKey.In
                } : null,
                BearerToken = auth.BearerToken != null ? new BearerTokenAuth
                {
                    Token = auth.BearerToken.Token,
                    Prefix = auth.BearerToken.Prefix
                } : null,
                Basic = auth.Basic != null ? new BasicAuth
                {
                    Username = auth.Basic.Username,
                    Password = auth.Basic.Password
                } : null,
                OAuth2 = auth.OAuth2 != null ? new OAuth2Auth
                {
                    AccessToken = auth.OAuth2.AccessToken,
                    RefreshToken = auth.OAuth2.RefreshToken,
                    TokenType = auth.OAuth2.TokenType,
                    ExpiresAt = auth.OAuth2.ExpiresAt
                } : null
            };
    }

    private List<RequestRecord> CloneRequests(List<RequestRecord> requests)
    {
        return requests.Select(r => new RequestRecord
        {
            Id = Guid.NewGuid().ToString(),
            Name = r.Name,
            Method = r.Method,
            Url = r.Url,
            Headers = new Dictionary<string, string>(r.Headers),
            Body = r.Body,
            ContentType = r.ContentType,
            Tags = [.. r.Tags],
            Description = r.Description,
            IsFavorite = r.IsFavorite,
            Group = r.Group
        }).ToList();
    }

    private List<ApiFolder> CloneFolders(List<ApiFolder> folders)
    {
        return folders.Select(f => CloneFolder(f)).ToList();
    }

    private ApiFolder CloneFolder(ApiFolder folder)
    {
        return new ApiFolder
        {
            Id = Guid.NewGuid().ToString(),
            Name = folder.Name,
            Description = folder.Description,
            ParentId = folder.ParentId,
            Requests = CloneRequests(folder.Requests),
            SubFolders = CloneFolders(folder.SubFolders)
        };
    }

    private ApiFolder? FindFolder(List<ApiFolder> folders, string folderId)
    {
        foreach (var folder in folders)
        {
            if (folder.Id == folderId)
            {
                return folder;
            }

            var found = FindFolder(folder.SubFolders, folderId);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private void RemoveFolder(List<ApiFolder> folders, string folderId)
    {
        var folder = folders.FirstOrDefault(f => f.Id == folderId);
        if (folder != null)
        {
            folders.Remove(folder);
            return;
        }

        foreach (var parentFolder in folders)
        {
            RemoveFolder(parentFolder.SubFolders, folderId);
        }
    }

    private void RemoveRequestFromFolders(List<ApiFolder> folders, string requestId)
    {
        foreach (var folder in folders)
        {
            var request = folder.Requests.FirstOrDefault(r => r.Id == requestId);
            if (request != null)
            {
                folder.Requests.Remove(request);
                return;
            }

            RemoveRequestFromFolders(folder.SubFolders, requestId);
        }
    }

    private List<RequestRecord> GetAllRequestsFromCollection(ApiCollection collection)
    {
        var allRequests = new List<RequestRecord>(collection.Requests);
        AddRequestsFromFolders(collection.Folders, allRequests);
        return allRequests;
    }

    private void AddRequestsFromFolders(List<ApiFolder> folders, List<RequestRecord> allRequests)
    {
        foreach (var folder in folders)
        {
            allRequests.AddRange(folder.Requests);
            AddRequestsFromFolders(folder.SubFolders, allRequests);
        }
    }

    private int CountFolders(List<ApiFolder> folders)
    {
        var count = folders.Count;
        foreach (var folder in folders)
        {
            count += CountFolders(folder.SubFolders);
        }
        return count;
    }

    #endregion
}