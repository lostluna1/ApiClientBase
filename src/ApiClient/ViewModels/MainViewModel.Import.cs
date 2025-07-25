using System.Collections.ObjectModel;
using ApiClient.Core.Contracts.Services;
using ApiClient.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ApiClient.ViewModels;

public partial class MainViewModel : ObservableRecipient
{

    [ObservableProperty]
    public partial string SwaggerUrl { get; set; } = "https://petstore.swagger.io/v2/swagger.json";

    [ObservableProperty]
    public partial string JsonContent { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CollectionName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsImporting
    {
        get;
        set;
    }

    [ObservableProperty]
    public partial string ImportStatus { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsUrlTabSelected { get; set; } = true;

    [ObservableProperty]
    public partial bool IsJsonTabSelected
    {
        get;
        set;
    }

    [ObservableProperty]
    public partial string ValidationMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsValidationSuccessful
    {
        get;
        set;
    }

    [ObservableProperty]
    public partial SwaggerValidationResult? ValidationResult
    {
        get;
        set;
    }

    [ObservableProperty]
    public partial UrlValidationResult? UrlValidationResult
    {
        get;
        set;
    }
    public ObservableCollection<ApiCollection> ImportedCollections { get; } = [];


    [RelayCommand]
    private async Task ImportFromUrlAsync()
    {
        if (string.IsNullOrWhiteSpace(SwaggerUrl))
        {
            ImportStatus = "请输入有效的Swagger文档URL";
            return;
        }

        try
        {
            IsImporting = true;
            ImportStatus = "正在从URL导入...";

            var collection = await _swaggerImportService.ImportFromUrlAsync(
                SwaggerUrl,
                string.IsNullOrWhiteSpace(CollectionName) ? null : CollectionName);

            // 检查是否存在同名集合
            await HandleCollectionConflictAsync(collection);

            ImportStatus = $"成功导入 '{collection.Name}'，包含 {GetTotalRequestCount(collection)} 个API接口";
            CollectionName = string.Empty;

            await LoadCollectionsAsync();

            // 刷新TreeView数据
            await LoadTreeDataAsync();
        }
        catch (Exception ex)
        {
            ImportStatus = $"导入失败: {ex.Message}";
        }
        finally
        {
            IsImporting = false;
        }
    }

    [RelayCommand]
    private async Task ImportFromJsonAsync()
    {
        if (string.IsNullOrWhiteSpace(JsonContent))
        {
            ImportStatus = "请输入有效的Swagger JSON内容";
            return;
        }

        try
        {
            IsImporting = true;
            ImportStatus = "正在从JSON导入...";

            var collection = await _swaggerImportService.ImportFromJsonAsync(
                JsonContent,
                string.IsNullOrWhiteSpace(CollectionName) ? null : CollectionName);

            // 检查是否存在同名集合
            await HandleCollectionConflictAsync(collection);

            ImportStatus = $"成功导入 '{collection.Name}'，包含 {GetTotalRequestCount(collection)} 个API接口";
            CollectionName = string.Empty;
            JsonContent = string.Empty;

            await LoadCollectionsAsync();

            // 刷新TreeView数据
            await LoadTreeDataAsync();
        }
        catch (Exception ex)
        {
            ImportStatus = $"导入失败: {ex.Message}";
        }
        finally
        {
            IsImporting = false;
        }
    }

    private async Task HandleCollectionConflictAsync(ApiCollection newCollection)
    {
        var existingCollections = await _collectionService.GetCollectionsAsync();
        var existingCollection = existingCollections.FirstOrDefault(c =>
            string.Equals(c.Name, newCollection.Name, StringComparison.OrdinalIgnoreCase));

        if (existingCollection != null)
        {
            // 如果存在同名集合，覆盖它
            ImportStatus = $"检测到同名集合 '{existingCollection.Name}'，正在覆盖...";

            // 保留原有的ID和创建时间
            newCollection.Id = existingCollection.Id;
            newCollection.CreatedAt = existingCollection.CreatedAt;
            newCollection.LastModified = DateTime.Now;

            await _collectionService.UpdateCollectionAsync(newCollection);
            ImportStatus = $"已覆盖集合 '{newCollection.Name}'";
        }
        else
        {
            // 新增集合
            await _collectionService.SaveCollectionAsync(newCollection);
        }
    }

    private async Task ValidateUrlAsync()
    {
        if (string.IsNullOrWhiteSpace(SwaggerUrl))
        {
            ValidationMessage = "请输入URL";
            IsValidationSuccessful = false;
            return;
        }

        try
        {
            ValidationMessage = "正在验证URL...";

            UrlValidationResult = await _swaggerImportService.ValidateUrlAsync(SwaggerUrl);

            if (UrlValidationResult.IsAccessible)
            {
                ValidationMessage = $"URL可访问 (状态码: {UrlValidationResult.StatusCode}, 响应时间: {UrlValidationResult.ResponseTimeMs}ms)";
                IsValidationSuccessful = true;
            }
            else
            {
                ValidationMessage = $"URL不可访问: {UrlValidationResult.ErrorMessage}";
                IsValidationSuccessful = false;
            }
        }
        catch (Exception ex)
        {
            ValidationMessage = $"验证失败: {ex.Message}";
            IsValidationSuccessful = false;
        }
    }

    private async Task ValidateJsonAsync()
    {
        if (string.IsNullOrWhiteSpace(JsonContent))
        {
            ValidationMessage = "请输入JSON内容";
            IsValidationSuccessful = false;
            return;
        }

        try
        {
            ValidationMessage = "正在验证JSON...";

            ValidationResult = await _swaggerImportService.ValidateSwaggerAsync(JsonContent);

            if (ValidationResult.IsValid)
            {
                var warnings = ValidationResult.Warnings.Count != 0
                    ? $" (警告: {ValidationResult.Warnings.Count})"
                    : string.Empty;

                ValidationMessage = $"JSON格式有效 - {ValidationResult.DetectedVersion}, {ValidationResult.ApiCount} 个API{warnings}";
                IsValidationSuccessful = true;
            }
            else
            {
                ValidationMessage = $"JSON格式无效: {ValidationResult.ErrorMessage}";
                IsValidationSuccessful = false;
            }
        }
        catch (Exception ex)
        {
            ValidationMessage = $"验证失败: {ex.Message}";
            IsValidationSuccessful = false;
        }
    }

    private void SelectTab(bool isUrlTab)
    {
        IsUrlTabSelected = isUrlTab;
        IsJsonTabSelected = !isUrlTab;
        ValidationMessage = string.Empty;
        ImportStatus = string.Empty;
    }

    [RelayCommand]
    private async Task LoadCollectionsAsync()
    {
        try
        {
            var collections = await _collectionService.GetCollectionsAsync();

            ImportedCollections.Clear();
            foreach (var collection in collections.OrderByDescending(c => c.LastModified))
            {
                ImportedCollections.Add(collection);
            }
        }
        catch (Exception ex)
        {
            ImportStatus = $"加载集合失败: {ex.Message}";
        }
    }

    private static int GetTotalRequestCount(ApiCollection collection)
    {
        var count = collection.Requests.Count;
        count += GetRequestCountFromFolders(collection.Folders);
        return count;
    }

    private static int GetRequestCountFromFolders(List<ApiFolder> folders)
    {
        var count = 0;
        foreach (var folder in folders)
        {
            count += folder.Requests.Count;
            count += GetRequestCountFromFolders(folder.SubFolders);
        }
        return count;
    }
}
