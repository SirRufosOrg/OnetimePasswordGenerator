#nullable disable
using System.Globalization;
using FluentAssertions;
using otpApp.Converters;

namespace otpApp.Tests.Converters;

public class StringNotEmptyConverterTests
{
    private readonly StringNotEmptyConverter _sut = new();

    [Fact]
    public void Convert_NonEmptyString_ReturnsTrue()
    {
        var result = _sut.Convert("hello", null, null, CultureInfo.InvariantCulture);

        result.Should().Be(true);
    }

    [Fact]
    public void Convert_EmptyString_ReturnsFalse()
    {
        var result = _sut.Convert("", null, null, CultureInfo.InvariantCulture);

        result.Should().Be(false);
    }

    [Fact]
    public void Convert_Null_ReturnsFalse()
    {
        var result = _sut.Convert(null, null, null, CultureInfo.InvariantCulture);

        result.Should().Be(false);
    }
}
