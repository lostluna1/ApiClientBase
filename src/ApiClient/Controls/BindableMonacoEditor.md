# BindableMonacoEditor 用户控件使用说明

## 概述
`BindableMonacoEditor` 是一个封装了 Monaco Editor 的 WinUI 3 用户控件，支持数据绑定，让你可以在 XAML 中直接使用 `x:Bind` 设置和获取编辑器文本内容。

## 功能特性
- **双向数据绑定**: 支持 `Text` 属性的双向绑定
- **语言设置**: 支持通过 `EditorLanguage` 属性设置编辑器语言
- **主题设置**: 支持通过 `EditorTheme` 属性设置编辑器主题
- **只读模式**: 支持通过 `IsReadOnly` 属性设置只读模式（待实现）
- **异步操作**: 内部处理异步内容更新
- **事件处理**: 自动处理内容变化并更新绑定属性

## 属性说明

### Text (string)
编辑器的文本内容，支持双向绑定。
```xml
Text="{x:Bind ViewModel.EditorText, Mode=TwoWay}"
```

### EditorLanguage (string)
编辑器的语言模式，默认为 "plaintext"。
支持的语言包括：json, xml, csharp, javascript, typescript, html, css 等。
```xml
EditorLanguage="{x:Bind ViewModel.EditorLang, Mode=TwoWay}"
```

### EditorTheme (Monaco.EditorThemes)
编辑器的主题，默认为 `Monaco.EditorThemes.VisualStudioLight`。
支持的主题包括：
- `Monaco.EditorThemes.VisualStudioLight`: Visual Studio 浅色主题
- `Monaco.EditorThemes.VisualStudioDark`: Visual Studio 深色主题  
- `Monaco.EditorThemes.HighContrastDark`: 高对比度深色主题

```xml
EditorTheme="{x:Bind ViewModel.EditorTheme, Mode=TwoWay}"
```

### IsReadOnly (bool)
设置编辑器是否为只读模式，默认为 false。
```xml
IsReadOnly="{x:Bind ViewModel.IsReadOnly, Mode=OneWay}"
```

## 使用示例

### XAML 中的使用
```xml
<Page xmlns:controls="using:EditorTest.Controls">
    <Grid>
        <controls:BindableMonacoEditor
            Text="{x:Bind ViewModel.EditorText, Mode=TwoWay}"
            EditorLanguage="{x:Bind ViewModel.EditorLang, Mode=TwoWay}"
            EditorTheme="{x:Bind ViewModel.EditorTheme, Mode=TwoWay}" />
    </Grid>
</Page>
```

### ViewModel 中的配置
```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public partial class MainViewModel : ObservableRecipient
{
    [ObservableProperty]
    public partial string EditorText { get; set; } = string.Empty;
    
    [ObservableProperty]
    public partial string EditorLang { get; set; } = "json";
    
    [ObservableProperty]
    public partial Monaco.EditorThemes EditorTheme { get; set; } = Monaco.EditorThemes.VisualStudioLight;
    
    [RelayCommand]
    public void LoadJsonContent()
    {
        EditorText = """
        {
            "name": "示例",
            "version": "1.0.0",
            "description": "这是一个示例 JSON"
        }
        """;
        EditorLang = "json";
    }
    
    [RelayCommand]
    public void ToggleTheme()
    {
        // 循环切换 Monaco Editor 主题
        if (EditorTheme.Equals(Monaco.EditorThemes.VisualStudioLight))
        {
            EditorTheme = Monaco.EditorThemes.VisualStudioDark;
        }
        else if (EditorTheme.Equals(Monaco.EditorThemes.VisualStudioDark))
        {
            EditorTheme = Monaco.EditorThemes.HighContrastDark;
        }
        else
        {
            EditorTheme = Monaco.EditorThemes.VisualStudioLight;
        }
    }
}
```

### 直接在 XAML 中设置主题
你也可以在 XAML 中直接设置主题值：
```xml
<controls:BindableMonacoEditor EditorTheme="VisualStudioDark" />
```

或在代码隐藏中设置：
```csharp
private void BindableMonacoEditor_Loaded(object sender, RoutedEventArgs e)
{
    _monaco.EditorTheme = Monaco.EditorThemes.VisualStudioDark;
}
```

## Monaco.EditorThemes 枚举

Monaco Editor 提供的官方主题枚举：
- `Monaco.EditorThemes.VisualStudioLight` - Visual Studio 浅色主题（默认）
- `Monaco.EditorThemes.VisualStudioDark` - Visual Studio 深色主题
- `Monaco.EditorThemes.HighContrastDark` - 高对比度深色主题

## 公开方法
- `GetEditorContentAsync()`: 异步获取编辑器内容
- `SetEditorContent(string)`: 设置编辑器内容（等同于设置 Text 属性）
- `SetEditorTheme(Monaco.EditorThemes)`: 设置编辑器主题（等同于设置 EditorTheme 属性）
- `MonacoEditor`: 提供对内部 Monaco Editor 实例的访问

## 注意事项
1. 控件内部使用异步操作处理内容更新，确保 UI 响应性
2. 内容变化时会自动更新绑定的 Text 属性
3. 主题变化会立即应用到编辑器
4. 建议在 ViewModel 中使用 ObservableProperty 确保属性变化通知
5. 使用 Monaco 官方的 EditorThemes 枚举确保主题兼容性
6. 主题比较时使用 `.Equals()` 方法而不是 `==` 操作符

## 完整示例
参考 MainPage.xaml 和 MainViewModel.cs 中的实现，展示了如何使用按钮命令来生成内容、切换主题等功能。

## 升级说明
本控件现在直接使用 Monaco Editor 包提供的 `Monaco.EditorThemes` 类型，确保与原生 Monaco Editor 的完全兼容性。如果你之前使用了自定义的枚举类型，请更新代码以使用 `Monaco.EditorThemes`。