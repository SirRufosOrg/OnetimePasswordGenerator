#nullable disable
using System.Globalization;
using FluentAssertions;
using otpApp.Converters;

namespace otpApp.Tests.Converters;

public class InvertBoolConverterTests
{
    private readonly InvertBoolConverter _sut = new();

    [Fact]
    public void Convert_True_ReturnsFalse()
    {
        var result = _sut.Convert(true, null, null, CultureInfo.InvariantCulture);

        result.Should().Be(false);
    }

    [Fact]
    public void Convert_False_ReturnsTrue()
    {
        var result = _sut.Convert(false, null, null, CultureInfo.InvariantCulture);

        result.Should().Be(true);
    }

    [Fact]
    public void Convert_Null_ReturnsNull()
    {
        var result = _sut.Convert(null, null, null, CultureInfo.InvariantCulture);

        result.Should().BeNull();
    }

    [Fact]
    public void ConvertBack_True_ReturnsFalse()
    {
        var result = _sut.ConvertBack(true, null, null, CultureInfo.InvariantCulture);

        result.Should().Be(false);
    }
}
