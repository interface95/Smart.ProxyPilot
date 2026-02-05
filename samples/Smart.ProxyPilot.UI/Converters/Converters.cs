using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Smart.ProxyPilot.UI.Models;

namespace Smart.ProxyPilot.UI.Converters;

/// <summary>
/// 代理状态转颜色转换器
/// </summary>
public class ProxyStateToColorConverter : IValueConverter
{
    public static ProxyStateToColorConverter Instance { get; } = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ProxyState state)
        {
            return state switch
            {
                ProxyState.Available => Brushes.Green,
                ProxyState.Validating => Brushes.Orange,
                ProxyState.Cooldown => Brushes.DodgerBlue,
                ProxyState.Disabled => Brushes.Gray,
                _ => Brushes.Gray
            };
        }
        return Brushes.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// 日志级别转颜色转换器
/// </summary>
public class LogLevelToColorConverter : IValueConverter
{
    public static LogLevelToColorConverter Instance { get; } = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is LogLevel level)
        {
            return level switch
            {
                LogLevel.Debug => Brushes.Gray,
                LogLevel.Info => Brushes.DodgerBlue,
                LogLevel.Warning => Brushes.Orange,
                LogLevel.Error => Brushes.Red,
                _ => Brushes.Gray
            };
        }
        return Brushes.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// 布尔值取反转换器
/// </summary>
public class InverseBoolConverter : IValueConverter
{
    public static InverseBoolConverter Instance { get; } = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
            return !b;
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
            return !b;
        return false;
    }
}

/// <summary>
/// 布尔值转颜色转换器
/// </summary>
public class BoolToColorConverter : IValueConverter
{
    public static BoolToColorConverter Instance { get; } = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
            return b ? Brushes.Green : Brushes.Gray;
        return Brushes.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// 布尔值转文本转换器
/// </summary>
public class BoolToTextConverter : IValueConverter
{
    public static BoolToTextConverter Instance { get; } = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
            return b ? "运行中" : "已停止";
        return "未知";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
