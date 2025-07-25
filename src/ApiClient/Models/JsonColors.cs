using Microsoft.UI;
using Windows.UI;

namespace ApiClient.Models;

public static class JsonHighlightColors
{
    public static readonly Color PropertyName = Color.FromArgb(255, 172, 21, 21);   // 红色
    public static readonly Color StringValue = Colors.LightBlue;                        // 蓝色
    public static readonly Color NumberValue = Colors.Teal;                        // 蓝绿色
    public static readonly Color BooleanValue = Colors.Green;                       // 草绿色
    public static readonly Color NullValue = Colors.Green;                       // 与布尔值一致
    public static readonly Color BracketColor = Colors.Blue;                        // 方括号
    public static readonly Color BraceColor = Colors.Green;                       // 花括号
    public static readonly Color SymbolColor = Colors.Gray;                        // 分隔符
}

