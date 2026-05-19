using System.Globalization;
using Avalonia.Data.Converters;

namespace otpApp.Converters;

public class CodeSplitterConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string code)
            return null;

        var parts = new List<string>();
        for (var i = code.Length; i > 0; i -= 3)
        {
            var start = Math.Max(0, i - 3);
            parts.Insert(0, code[start..i]);
        }
        return parts;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}