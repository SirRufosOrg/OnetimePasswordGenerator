#nullable disable
using System.Globalization;
using FluentAssertions;
using otpApp.Converters;

namespace otpApp.Tests.Converters;

public class CodeSplitterConverterTests
{
    private readonly CodeSplitterConverter _sut = new();

    [Fact]
    public void Convert_SixDigits_ReturnsTwoParts()
    {
        var result = _sut.Convert("123456", null, null, CultureInfo.InvariantCulture);

        result.Should().BeOfType<List<string>>()
            .Which.Should().BeEquivalentTo(["123", "456"], o => o.WithStrictOrdering());
    }

    [Fact]
    public void Convert_EightDigits_ReturnsThreeParts()
    {
        var result = _sut.Convert("12345678", null, null, CultureInfo.InvariantCulture);

        result.Should().BeOfType<List<string>>()
            .Which.Should().BeEquivalentTo(["123", "456", "78"], o => o.WithStrictOrdering());
    }

    [Fact]
    public void Convert_Null_ReturnsNull()
    {
        var result = _sut.Convert(null, null, null, CultureInfo.InvariantCulture);

        result.Should().BeNull();
    }

    [Fact]
    public void Convert_NonString_ReturnsNull()
    {
        var result = _sut.Convert(42, null, null, CultureInfo.InvariantCulture);

        result.Should().BeNull();
    }
}
