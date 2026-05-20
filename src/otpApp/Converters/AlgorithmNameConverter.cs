using System.Globalization;
using Avalonia.Data.Converters;
using otpApp.Models;

namespace otpApp.Converters;

public class AlgorithmNameConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not OtpAlgorithm algorithm)
            return null;

        return algorithm switch
        {
            OtpAlgorithm.SHA1 => "SHA1",
            OtpAlgorithm.SHA224 => "SHA224",
            OtpAlgorithm.SHA256 => "SHA256",
            OtpAlgorithm.SHA384 => "SHA384",
            OtpAlgorithm.SHA512 => "SHA512",
            OtpAlgorithm.SHA3_224 => "SHA3-224",
            OtpAlgorithm.SHA3_256 => "SHA3-256",
            OtpAlgorithm.SHA3_384 => "SHA3-384",
            OtpAlgorithm.SHA3_512 => "SHA3-512",
            OtpAlgorithm.MD5 => "MD5",
            _ => algorithm.ToString()
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
