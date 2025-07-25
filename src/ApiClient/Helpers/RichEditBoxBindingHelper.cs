using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;

namespace ApiClient.Helpers;

public class RichEditBoxBindingHelper : DependencyObject
{
    public static string GetBoundText(RichEditBox obj) => (string)obj.GetValue(BoundTextProperty);

    public static void SetBoundText(RichEditBox obj, string value) => obj.SetValue(BoundTextProperty, value);

    public static readonly DependencyProperty BoundTextProperty =
        DependencyProperty.RegisterAttached("BoundText", typeof(string), typeof(RichEditBoxBindingHelper),
            new PropertyMetadata(string.Empty, OnBoundTextChanged));

    private const int DEBOUNCE_DELAY = 300; // 防抖延迟

    private static void OnBoundTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not RichEditBox box)
            return;

        var newText = e.NewValue as string ?? string.Empty;

        void ApplyTextAndHighlight()
        {
            try
            {
                box.Document.GetText(TextGetOptions.None, out var currentText);
                
                // 比较时忽略末尾的换行符差异
                if (currentText.TrimEnd() == newText.TrimEnd())
                    return;

                var originalReadOnly = box.IsReadOnly;
                if (originalReadOnly)
                    box.IsReadOnly = false;

                var stopwatch = Stopwatch.StartNew();
                
                // 设置文本
                box.Document.SetText(TextSetOptions.None, newText);
                
                // 进行语法高亮
                try
                {
                    // 异步进行语法高亮，避免阻塞UI
                    _ = box.DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () =>
                    {
                        try
                        {
                            JsonHighlighter.RenderHighlights(box.Document, JsonHighlighter.GetHighlightSegments(newText));
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"异步JSON高亮失败: {ex.Message}");
                        }
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"JSON高亮调度失败: {ex.Message}");
                }
                
                stopwatch.Stop();
                if (stopwatch.ElapsedMilliseconds > 50) // 只记录较慢的操作
                {
                    Debug.WriteLine($"RichEditBox文本更新耗时: {stopwatch.ElapsedMilliseconds}ms");
                }

                if (originalReadOnly)
                    box.IsReadOnly = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ApplyTextAndHighlight失败: {ex.Message}");
                
                // 备用方案：至少保证文本能正确显示
                try
                {
                    box.Document.SetText(TextSetOptions.None, newText);
                }
                catch (Exception innerEx)
                {
                    Debug.WriteLine($"备用文本设置也失败: {innerEx.Message}");
                }
            }
        }

        // 使用低优先级异步调用，避免阻塞UI线程
        _ = box.DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () =>
        {
            // 延迟一帧确保布局完成
            box.LayoutUpdated += OneTimeLayoutHandler;

            void OneTimeLayoutHandler(object sender, object args)
            {
                box.LayoutUpdated -= OneTimeLayoutHandler;
                ApplyTextAndHighlight();
            }
        });

        // 移除之前的事件处理程序，避免重复绑定
        box.TextChanged -= RichEditBox_TextChanged;
        box.TextChanged += RichEditBox_TextChanged;
    }

    private static void RichEditBox_TextChanged(object sender, RoutedEventArgs e)
    {
        if (sender is not RichEditBox box)
            return;

        // 获取或创建防抖计时器
        var timer = box.GetValue(UpdateTimerProperty) as DispatcherTimer;
        timer?.Stop();

        timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(DEBOUNCE_DELAY)
        };

        timer.Tick += (s, args) =>
        {
            timer.Stop();
            
            try
            {
                HandleTextChanged(box);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"防抖TextChanged处理失败: {ex.Message}");
            }
        };

        box.SetValue(UpdateTimerProperty, timer);
        timer.Start();
    }

    private static void HandleTextChanged(RichEditBox box)
    {
        try
        {
            // 记录光标位置
            var selection = box.Document.Selection;
            var start = selection.StartPosition;
            var end = selection.EndPosition;

            box.Document.GetText(TextGetOptions.None, out var updatedText);
            var currentBoundText = GetBoundText(box);

            // 比较时忽略末尾的换行符差异
            if (updatedText.TrimEnd() == currentBoundText.TrimEnd())
                return;

            // 更新绑定的属性
            SetBoundText(box, updatedText);

            // 进行实时语法高亮
            _ = box.DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () =>
            {
                try
                {
                    JsonHighlighter.RenderHighlights(box.Document, JsonHighlighter.GetHighlightSegments(updatedText));
                    
                    // 恢复光标位置
                    try
                    {
                        box.Document.Selection.SetRange(start, end);
                    }
                    catch
                    {
                        // 如果光标位置无效，不做处理
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"实时JSON高亮失败: {ex.Message}");
                }
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"HandleTextChanged失败: {ex.Message}");
        }
    }

    // 用于存储防抖计时器的附加属性
    private static readonly DependencyProperty UpdateTimerProperty =
        DependencyProperty.RegisterAttached("UpdateTimer", typeof(DispatcherTimer), typeof(RichEditBoxBindingHelper), null);
}
