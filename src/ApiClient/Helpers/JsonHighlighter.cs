using System.Text.Json;
using ApiClient.Models;
using Microsoft.UI;
using Microsoft.UI.Text;
using Windows.UI;
using System.Diagnostics;

namespace ApiClient.Helpers;

public static class JsonHighlighter
{
    // 限制高亮段数量，避免过多的UI操作
    private const int MAX_SEGMENTS = 1500;
    
    public static List<(int start, int length, Color color)> GetHighlightSegments(string jsonText)
    {
        var segments = new List<(int, int, Color)>();

        if (string.IsNullOrWhiteSpace(jsonText))
            return segments;

        var stopwatch = Stopwatch.StartNew();

        try
        {
            // 快速检查是否可能是JSON格式
            var trimmed = jsonText.TrimStart();
            if (!trimmed.StartsWith("{") && !trimmed.StartsWith("["))
            {
                // 不是JSON格式，只高亮基本符号
                HighlightBasicSymbols(jsonText, segments);
                return segments;
            }

            // 尝试解析JSON
            using var doc = JsonDocument.Parse(jsonText);
            TraverseOptimized(doc.RootElement, jsonText, segments);
        }
        catch (JsonException)
        {
            // JSON无效，提供基本的符号高亮
            Debug.WriteLine("JSON格式无效，使用基本符号高亮");
            HighlightBasicSymbols(jsonText, segments);
            return segments;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"JSON解析异常: {ex.Message}");
            return segments;
        }

        try
        {
            HighlightBrackets(jsonText, segments);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"括号高亮处理异常: {ex.Message}");
        }

        // 限制段数量以提高性能
        if (segments.Count > MAX_SEGMENTS)
        {
            Debug.WriteLine($"高亮段过多({segments.Count})，截取前{MAX_SEGMENTS}个");
            segments = segments.Take(MAX_SEGMENTS).ToList();
        }

        stopwatch.Stop();
        if (stopwatch.ElapsedMilliseconds > 10) // 只记录较慢的操作
        {
            Debug.WriteLine($"JSON语法高亮耗时: {stopwatch.ElapsedMilliseconds}ms，处理长度: {jsonText.Length}，段数: {segments.Count}");
        }

        return segments;
    }

    private static void TraverseOptimized(JsonElement element, string rawText, List<(int, int, Color)> list)
    {
        // 如果already有太多段，停止处理
        if (list.Count > MAX_SEGMENTS)
            return;

        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var prop in element.EnumerateObject())
                {
                    if (list.Count > MAX_SEGMENTS) break;
                    
                    // 添加所有匹配的属性名
                    AddAllOccurrences(rawText, $"\"{prop.Name}\"", JsonHighlightColors.PropertyName, list);
                    TraverseOptimized(prop.Value, rawText, list);
                }
                break;

            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                {
                    if (list.Count > MAX_SEGMENTS) break;
                    TraverseOptimized(item, rawText, list);
                }
                break;

            case JsonValueKind.String:
                var stringValue = element.GetString();
                if (!string.IsNullOrEmpty(stringValue))
                {
                    // 添加所有匹配的字符串值
                    AddAllOccurrences(rawText, $"\"{stringValue}\"", JsonHighlightColors.StringValue, list);
                }
                break;

            case JsonValueKind.Number:
                var numberStr = element.ToString();
                // 添加所有匹配的数字
                AddAllOccurrences(rawText, numberStr, JsonHighlightColors.NumberValue, list);
                break;

            case JsonValueKind.True:
                AddAllOccurrences(rawText, "true", JsonHighlightColors.BooleanValue, list);
                break;

            case JsonValueKind.False:
                AddAllOccurrences(rawText, "false", JsonHighlightColors.BooleanValue, list);
                break;

            case JsonValueKind.Null:
                AddAllOccurrences(rawText, "null", JsonHighlightColors.NullValue, list);
                break;
        }
    }

    /// <summary>
    /// 添加所有匹配的词项，解决渲染不完整的问题
    /// </summary>
    private static void AddAllOccurrences(string source, string token, Color color, List<(int, int, Color)> list)
    {
        try
        {
            if (string.IsNullOrEmpty(token)) return;

            var start = 0;
            while ((start = source.IndexOf(token, start, StringComparison.Ordinal)) != -1)
            {
                if (list.Count >= MAX_SEGMENTS) break;

                // 检查是否与现有段重叠
                var overlapping = list.Any(seg => 
                    (start >= seg.Item1 && start < seg.Item1 + seg.Item2) ||
                    (start + token.Length > seg.Item1 && start < seg.Item1 + seg.Item2));
                
                if (!overlapping)
                {
                    list.Add((start, token.Length, color));
                }
                
                start += token.Length; // 移动到下一个可能的位置
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"添加高亮段失败: {ex.Message}");
        }
    }

    private static void HighlightBrackets(string source, List<(int, int, Color)> list)
    {
        for (var i = 0; i < source.Length && list.Count < MAX_SEGMENTS; i++)
        {
            var c = source[i];
            Color? color = c switch
            {
                '{' or '}' => JsonHighlightColors.BraceColor,
                '[' or ']' => JsonHighlightColors.BracketColor,
                ':' or ',' => JsonHighlightColors.SymbolColor,
                _ => null
            };

            if (color.HasValue)
            {
                // 快速检查重叠（简化版本）
                var overlapping = list.Any(seg => i >= seg.Item1 && i < seg.Item1 + seg.Item2);
                if (!overlapping)
                {
                    list.Add((i, 1, color.Value));
                }
            }
        }
    }

    /// <summary>
    /// 基本符号高亮，用于无效JSON
    /// </summary>
    private static void HighlightBasicSymbols(string source, List<(int, int, Color)> list)
    {
        for (var i = 0; i < source.Length && list.Count < MAX_SEGMENTS; i++)
        {
            var c = source[i];
            Color? color = c switch
            {
                '{' or '}' => JsonHighlightColors.BraceColor,
                '[' or ']' => JsonHighlightColors.BracketColor,
                ':' => JsonHighlightColors.SymbolColor,
                ',' => JsonHighlightColors.SymbolColor,
                '"' => JsonHighlightColors.StringValue,
                _ => null
            };

            if (color.HasValue)
            {
                list.Add((i, 1, color.Value));
            }
        }
    }

    public static void RenderHighlights(RichEditTextDocument doc, List<(int start, int length, Color color)> segments)
    {
        if (segments == null || segments.Count == 0)
            return;

        var stopwatch = Stopwatch.StartNew();

        try
        {
            // 批量处理：首先重置所有格式
            doc.Selection.SetRange(0, int.MaxValue);
            doc.Selection.CharacterFormat.ForegroundColor = Colors.Black;

            // 按位置排序并去重，优化渲染性能
            var sortedSegments = segments
                .Where(seg => seg.Item1 >= 0 && seg.Item2 > 0)
                .OrderBy(seg => seg.Item1)
                .Take(MAX_SEGMENTS) // 再次限制数量
                .ToList();

            // 批量应用格式
            foreach (var seg in sortedSegments)
            {
                try
                {
                    doc.Selection.SetRange(seg.Item1, seg.Item1 + seg.Item2);
                    doc.Selection.CharacterFormat.ForegroundColor = seg.Item3;
                }
                catch (ArgumentException)
                {
                    // 位置超出范围，跳过这个段
                    Debug.WriteLine($"跳过超出范围的高亮段: start={seg.Item1}, length={seg.Item2}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"设置高亮格式失败 at {seg.Item1}: {ex.Message}");
                }
            }

            // 重置选择到开始位置
            doc.Selection.SetRange(0, 0);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"渲染高亮失败: {ex.Message}");
        }

        stopwatch.Stop();
        if (stopwatch.ElapsedMilliseconds > 20) // 只记录较慢的操作
        {
            Debug.WriteLine($"高亮渲染耗时: {stopwatch.ElapsedMilliseconds}ms，段数: {segments.Count}");
        }
    }
}
