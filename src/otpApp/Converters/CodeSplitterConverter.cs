using System.Globalization;

using Avalonia.Data.Converters;

namespace otpApp.Converters;

public class CodeSplitterConverter : IValueConverter
{
    public object? Convert( object? value, Type targetType, object? parameter, CultureInfo culture )
    {
        if ( value is not string code )
            return null;

        var parts = new List<string>();
        for ( var i = 0; i < code.Length; i += 3 )
        {
            parts.Add( code[i..Math.Min( i + 3, code.Length )] );
        }
        return parts;
    }

    public object? ConvertBack( object? value, Type targetType, object? parameter, CultureInfo culture )
    {
        throw new NotSupportedException();
    }
}