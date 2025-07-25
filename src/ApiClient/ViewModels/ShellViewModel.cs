using ApiClient.Contracts.Services;
using ApiClient.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;
using CommunityToolkit.Mvvm.Messaging;
using ApiClient.Messages;
namespace ApiClient.ViewModels;

public partial class ShellViewModel : ObservableRecipient
{
    private readonly IThemeSelectorService _themeSelectorService;

    [ObservableProperty]
    public partial bool IsBackEnabled
    {
        get;
        set;
    }

    [ObservableProperty]
    public partial object? Selected
    {
        get;
        set;
    }

    [ObservableProperty]
    public partial ElementTheme CurrentTheme
    {
        get;
        set;
    }

    public INavigationService NavigationService
    {
        get;
    }

    public INavigationViewService NavigationViewService
    {
        get;
    }

    // 主题图标属性
    public string ThemeIcon => CurrentTheme switch
    {
        ElementTheme.Light => "\uE706", // 太阳图标
        ElementTheme.Dark => "\uE708",  // 月亮图标
        _ => "\uE706" // 默认为太阳图标
    };

    // 主题提示信息属性
    public string ThemeToolTip => CurrentTheme switch
    {
        ElementTheme.Light => "切换到深色主题",
        ElementTheme.Dark => "切换到浅色主题",
        _ => "切换主题"
    };

    public ShellViewModel(INavigationService navigationService, INavigationViewService navigationViewService, IThemeSelectorService themeSelectorService)
    {
        NavigationService = navigationService;
        NavigationService.Navigated += OnNavigated;
        NavigationViewService = navigationViewService;
        _themeSelectorService = themeSelectorService;
        
        // 初始化当前主题，如果是自动主题则转为浅色主题
        CurrentTheme = _themeSelectorService.Theme == ElementTheme.Default ? ElementTheme.Light : _themeSelectorService.Theme;
    }

    [RelayCommand]
    private async Task SwitchThemeAsync()
    {
        // 只在浅色和深色主题之间切换
        var newTheme = CurrentTheme switch
        {
            ElementTheme.Light => ElementTheme.Dark,
            ElementTheme.Dark => ElementTheme.Light,
            _ => ElementTheme.Light // 如果是其他值，默认切换到浅色
        };

        CurrentTheme = newTheme;
        await _themeSelectorService.SetThemeAsync(newTheme);
        
        // 通知属性更新
        OnPropertyChanged(nameof(ThemeIcon));
        OnPropertyChanged(nameof(ThemeToolTip));
        
        // 发送主题变化消息给其他ViewModel
        Messenger.Send(new ThemeChangedMessage(newTheme));
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        IsBackEnabled = NavigationService.CanGoBack;

        if (e.SourcePageType == typeof(SettingsPage))
        {
            Selected = NavigationViewService.SettingsItem;
            return;
        }

        var selectedItem = NavigationViewService.GetSelectedItem(e.SourcePageType);
        if (selectedItem != null)
        {
            Selected = selectedItem;
        }
    }
}
