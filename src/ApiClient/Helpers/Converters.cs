using ApiClient.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.Text;

namespace ApiClient.Helpers;

public class BoolNegationConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return !(bool)value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return !(bool)value;
    }
}

public class StatusCodeColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is int statusCode
            ? statusCode switch
            {
                >= 200 and < 300 => new SolidColorBrush(Colors.Green),
                >= 300 and < 400 => new SolidColorBrush(Colors.Orange),
                >= 400 => new SolidColorBrush(Colors.Red),
                _ => new SolidColorBrush(Colors.Gray)
            }
            : new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class BoolToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return (bool)value ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class NodeTypeToFontWeightConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is TreeNodeType nodeType)
        {
            return nodeType switch
            {
                TreeNodeType.Collection => new FontWeight { Weight = 700 }, // Bold
                TreeNodeType.Folder => new FontWeight { Weight = 600 },     // SemiBold
                TreeNodeType.Request => new FontWeight { Weight = 400 },    // Normal
                _ => new FontWeight { Weight = 400 }                        // Normal
            };
        }
        return new FontWeight { Weight = 400 }; // Normal
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class NodeTypeToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is TreeNodeType nodeType && parameter is string param
            ? param switch
            {
                "Request" => nodeType == TreeNodeType.Request ? Visibility.Visible : Visibility.Collapsed,
                "NotRequest" => nodeType != TreeNodeType.Request ? Visibility.Visible : Visibility.Collapsed,
                _ => Visibility.Visible
            }
            : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class HttpMethodToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is string method
            ? method.ToUpper() switch
            {
                "GET" => new SolidColorBrush(Colors.Green),
                "POST" => new SolidColorBrush(Colors.Orange),
                "PUT" => new SolidColorBrush(Colors.Blue),
                "DELETE" => new SolidColorBrush(Colors.Red),
                "PATCH" => new SolidColorBrush(Colors.Purple),
                "HEAD" => new SolidColorBrush(Colors.Gray),
                "OPTIONS" => new SolidColorBrush(Colors.Brown),
                _ => new SolidColorBrush(Colors.Gray)
            }
            : new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class CountToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int count)
        {
            var param = parameter as string;
            return param?.ToLower() switch
            {
                "zero" => count == 0 ? Visibility.Visible : Visibility.Collapsed,
                _ => count > 0 ? Visibility.Visible : Visibility.Collapsed
            };
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (parameter is string param)
        {
            // 如果有参数，比较字符串是否相等
            return string.Equals(value as string, param, StringComparison.OrdinalIgnoreCase)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        // 如果没有参数，检查字符串是否非空
        return !string.IsNullOrEmpty(value as string) ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool boolValue)
        {
            // 检查是否有反转参数
            var shouldInvert = parameter is string param && 
                              (param.Equals("Inverse", StringComparison.OrdinalIgnoreCase) || 
                               param.Equals("Invert", StringComparison.OrdinalIgnoreCase));
            
            if (shouldInvert)
            {
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }
            else
            {
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value is Visibility.Visible;
    }
}

public class BoolToOpacityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is bool boolValue ? boolValue ? 1.0 : 0.3 : 0.3;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class BoolToFontWeightConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is bool boolValue
            ? boolValue ? new FontWeight { Weight = 600 } : new FontWeight { Weight = 400 }
            : new FontWeight { Weight = 400 };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class BoolToAccentColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Color.FromArgb(100, 0, 120, 215) : Colors.Transparent; // IsCurrentTab对应的颜色设置为浅蓝色
        }
        return Colors.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class StringToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (parameter is string param)
        {
            return string.Equals(value as string, param, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is bool boolValue && boolValue && parameter is string param)
        {
            return param;
        }
        return null;
    }
}

public class BooleanInverterConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is bool boolValue ? !boolValue : true;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value is bool boolValue ? !boolValue : false;
    }
}

public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value == null ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class MultiValueStringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string stringValue && parameter is string param)
        {
            // 支持多个值，用空格分隔
            var targetValues = param.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return targetValues.Any(target => string.Equals(stringValue, target, StringComparison.OrdinalIgnoreCase))
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}