// src\ApiClient\Messages\ThemeChangedMessage.cs
using Microsoft.UI.Xaml;

namespace ApiClient.Messages;

/// <summary>
/// 主题变化消息
/// </summary>
public record ThemeChangedMessage(ElementTheme NewTheme);