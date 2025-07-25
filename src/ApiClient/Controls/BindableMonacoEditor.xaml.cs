using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Reflection;
using System.Threading.Tasks;

namespace ApiClient.Controls;

public sealed partial class BindableMonacoEditor : UserControl
{
    private bool _isUpdatingContent = false;
    private bool _hideScrollbars = false;

    public BindableMonacoEditor()
    {
        this.InitializeComponent();
    }

    // Text 依赖属性，支持双向绑定
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(BindableMonacoEditor),
            new PropertyMetadata(string.Empty, OnTextPropertyChanged));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    // EditorLanguage 依赖属性
    public static readonly DependencyProperty EditorLanguageProperty =
        DependencyProperty.Register(
            nameof(EditorLanguage),
            typeof(string),
            typeof(BindableMonacoEditor),
            new PropertyMetadata("plaintext", OnEditorLanguagePropertyChanged));

    public string EditorLanguage
    {
        get => (string)GetValue(EditorLanguageProperty);
        set => SetValue(EditorLanguageProperty, value);
    }

    // EditorTheme 依赖属性 - 使用 Monaco.EditorThemes 类型
    public static readonly DependencyProperty EditorThemeProperty =
        DependencyProperty.Register(
            nameof(EditorTheme),
            typeof(Monaco.EditorThemes),
            typeof(BindableMonacoEditor),
            new PropertyMetadata(Monaco.EditorThemes.VisualStudioLight, OnEditorThemePropertyChanged));

    public Monaco.EditorThemes EditorTheme
    {
        get => (Monaco.EditorThemes)GetValue(EditorThemeProperty);
        set => SetValue(EditorThemeProperty, value);
    }

    // HideScrollbars 依赖属性 - 控制是否隐藏滚动条
    public static readonly DependencyProperty HideScrollbarsProperty =
        DependencyProperty.Register(
            nameof(HideScrollbars),
            typeof(bool),
            typeof(BindableMonacoEditor),
            new PropertyMetadata(false, OnHideScrollbarsPropertyChanged));

    public bool HideScrollbars
    {
        get => (bool)GetValue(HideScrollbarsProperty);
        set => SetValue(HideScrollbarsProperty, value);
    }

    // IsReadOnly 依赖属性
    public static readonly DependencyProperty IsReadOnlyProperty =
        DependencyProperty.Register(
            nameof(IsReadOnly),
            typeof(bool),
            typeof(BindableMonacoEditor),
            new PropertyMetadata(false, OnIsReadOnlyPropertyChanged));

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    // Text 属性变化处理
    private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is BindableMonacoEditor control && !control._isUpdatingContent)
        {
            control.UpdateEditorContent((string)e.NewValue);
        }
    }

    // EditorLanguage 属性变化处理
    private static void OnEditorLanguagePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is BindableMonacoEditor control && control.InternalMonacoEditor != null)
        {
            control.InternalMonacoEditor.EditorLanguage = (string)e.NewValue;
        }
    }

    // EditorTheme 属性变化处理
    private static void OnEditorThemePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is BindableMonacoEditor control && control.InternalMonacoEditor != null && e.NewValue != null)
        {
            control.InternalMonacoEditor.EditorTheme = (Monaco.EditorThemes)e.NewValue;
        }
    }

    // HideScrollbars 属性变化处理
    private static async void OnHideScrollbarsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is BindableMonacoEditor control)
        {
            control._hideScrollbars = (bool)e.NewValue;
            await control.ApplyScrollbarVisibilityAsync();
        }
    }

    // IsReadOnly 属性变化处理
    private static void OnIsReadOnlyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is BindableMonacoEditor control)
        {
            // Monaco Editor 可能没有直接的 IsReadOnly 属性，这里可以根据实际情况调整
            // control.InternalMonacoEditor.IsReadOnly = (bool)e.NewValue;
        }
    }

    /// <summary>
    /// 获取 Monaco 编辑器的 WebView2 控件
    /// </summary>
    /// <param name="editor">Monaco 编辑器实例</param>
    /// <returns>WebView2 控件或 null</returns>
    public static object? GetMonacoWebView(Monaco.MonacoEditor editor)
    {
        var field = typeof(Monaco.MonacoEditor).GetField("MonacoEditorWebView",
            BindingFlags.NonPublic | BindingFlags.Instance);
        return field?.GetValue(editor);
    }

    // 应用滚动条可见性设置
    private async Task ApplyScrollbarVisibilityAsync()
    {
        if (InternalMonacoEditor == null) return;

        try
        {
            var webView = GetMonacoWebView(InternalMonacoEditor);
            if (webView != null)
            {
                // 尝试通过反射调用 ExecuteScriptAsync 方法
                var coreWebView2Property = webView.GetType().GetProperty("CoreWebView2");
                var coreWebView2 = coreWebView2Property?.GetValue(webView);

                if (coreWebView2 != null)
                {
                    var executeScriptMethod = coreWebView2.GetType().GetMethod("ExecuteScriptAsync");
                    if (executeScriptMethod != null)
                    {
                        var script = _hideScrollbars ? GetHideScrollbarsScript() : GetShowScrollbarsScript();
                        var task = (Task)executeScriptMethod.Invoke(coreWebView2, new object[] { script })!;
                        await task;
                    }
                }
            }
        }
        catch
        {
            // 静默处理脚本执行错误
        }
    }

    // 获取隐藏滚动条的脚本
    private string GetHideScrollbarsScript()
    {
        return @"
            (function() {
                // 创建或更新样式元素
                let styleId = 'monaco-hide-scrollbars-style';
                let existingStyle = document.getElementById(styleId);
                
                if (!existingStyle) {
                    let style = document.createElement('style');
                    style.id = styleId;
                    style.type = 'text/css';
                    document.head.appendChild(style);
                    existingStyle = style;
                }
                
                // 隐藏所有滚动条的 CSS
                existingStyle.innerHTML = `
                    /* 隐藏 body 滚动条 */
                    body {
                        overflow: hidden !important;
                    }
                    
                    /* 隐藏 WebKit 滚动条 */
                    body::-webkit-scrollbar,
                    html::-webkit-scrollbar,
                    *::-webkit-scrollbar {
                        width: 0px !important;
                        height: 0px !important;
                        background: transparent !important;
                        display: none !important;
                    }
                    
                    /* 隐藏滚动条轨道 */
                    body::-webkit-scrollbar-track,
                    html::-webkit-scrollbar-track,
                    *::-webkit-scrollbar-track {
                        background: transparent !important;
                        display: none !important;
                    }
                    
                    /* 隐藏滚动条滑块 */
                    body::-webkit-scrollbar-thumb,
                    html::-webkit-scrollbar-thumb,
                    *::-webkit-scrollbar-thumb {
                        background: transparent !important;
                        display: none !important;
                    }
                    
                    /* 隐藏滚动条角落 */
                    body::-webkit-scrollbar-corner,
                    html::-webkit-scrollbar-corner,
                    *::-webkit-scrollbar-corner {
                        background: transparent !important;
                        display: none !important;
                    }
                    
                    /* Firefox 滚动条隐藏 */
                    html {
                        scrollbar-width: none !important;
                    }
                    
                    /* 确保 Monaco Editor 容器不显示滚动条 */
                    .monaco-editor,
                    .monaco-editor .overflow-guard,
                    .monaco-editor .monaco-scrollable-element {
                        overflow: hidden !important;
                    }
                    
                    /* 隐藏外部容器的滚动条 */
                    #container {
                        overflow: hidden !important;
                    }
                `;
                
                console.log('Monaco Editor scrollbars hidden');
                return 'success';
            })();
        ";
    }

    // 获取显示滚动条的脚本
    private string GetShowScrollbarsScript()
    {
        return @"
            (function() {
                // 移除隐藏滚动条的样式
                let styleId = 'monaco-hide-scrollbars-style';
                let existingStyle = document.getElementById(styleId);
                if (existingStyle) {
                    existingStyle.remove();
                }
                
                console.log('Monaco Editor scrollbars restored');
                return 'success';
            })();
        ";
    }

    // 更新编辑器内容（同步版本）
    private void UpdateEditorContent(string content)
    {
        if (InternalMonacoEditor != null)
        {
            InternalMonacoEditor.EditorContent = content ?? string.Empty;
        }
    }

    // Monaco Editor 加载完成事件
    private async void InternalMonacoEditor_Loaded(object sender, RoutedEventArgs e)
    {
        // 初始化时设置内容
        UpdateEditorContent(Text);

        // 设置语言和主题
        if (InternalMonacoEditor != null)
        {
            InternalMonacoEditor.EditorLanguage = EditorLanguage;
            InternalMonacoEditor.EditorTheme = EditorTheme;
        }

        // 等待一段时间确保 WebView 完全加载，然后应用滚动条设置
        await Task.Delay(1500);  // 给 WebView 更多时间来完全加载
        await ApplyScrollbarVisibilityAsync();
    }

    // Monaco Editor 内容变化事件
    private async void InternalMonacoEditor_EditorContentChanged(object sender, System.EventArgs e)
    {
        if (!_isUpdatingContent && InternalMonacoEditor != null)
        {
            _isUpdatingContent = true;
            try
            {
                // 获取编辑器内容并更新绑定属性
                var content = await InternalMonacoEditor.GetEditorContentAsync();
                SetValue(TextProperty, content);
            }
            finally
            {
                _isUpdatingContent = false;
            }
        }
    }

    // 公开一些常用的 Monaco Editor 方法
    public async Task<string> GetEditorContentAsync()
    {
        if (InternalMonacoEditor != null)
        {
            return await InternalMonacoEditor.GetEditorContentAsync();
        }
        return string.Empty;
    }

    public void SetEditorContent(string content)
    {
        Text = content;
    }

    // 设置编辑器主题
    public void SetEditorTheme(Monaco.EditorThemes theme)
    {
        EditorTheme = theme;
    }

    // 设置滚动条可见性
    public async Task SetScrollbarVisibilityAsync(bool hideScrollbars)
    {
        HideScrollbars = hideScrollbars;
        await ApplyScrollbarVisibilityAsync();
    }

    // 提供对内部 Monaco Editor 的访问（如果需要）
    public Monaco.MonacoEditor? MonacoEditor => InternalMonacoEditor;
}