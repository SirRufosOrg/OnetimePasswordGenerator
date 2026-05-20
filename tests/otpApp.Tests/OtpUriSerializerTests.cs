using otpApp.Models;
using otpApp.Services;

namespace otpApp.Tests;

public class OtpUriSerializerTests
{
    private readonly OtpUriSerializer _sut = new();

    [Fact]
    public void ToUri_Totp_OmitDefaults()
    {
        var account = new OtpAccount
        {
            Type = OtpType.Totp,
            Issuer = "GitHub",
            Label = "user",
            SecretBase32 = "JBSWY3DPEHPK3PXP",
            Algorithm = OtpAlgorithm.SHA1,
            Digits = 6,
            Period = 30,
        };

        var result = _sut.ToUri( account );

        result.Should().Be( "otpauth://totp/GitHub:user?secret=JBSWY3DPEHPK3PXP&issuer=GitHub" );
    }

    [Fact]
    public void ToUri_Totp_WithCustomValues()
    {
        var account = new OtpAccount
        {
            Type = OtpType.Totp,
            Issuer = "GitHub",
            Label = "user",
            SecretBase32 = "JBSWY3DPEHPK3PXP",
            Algorithm = OtpAlgorithm.SHA256,
            Digits = 8,
            Period = 60,
        };

        var result = _sut.ToUri( account );

        result.Should().Be( "otpauth://totp/GitHub:user?secret=JBSWY3DPEHPK3PXP&issuer=GitHub&algorithm=SHA256&digits=8&period=60" );
    }

    [Fact]
    public void ToUri_Hotp_IncludesCounter()
    {
        var account = new OtpAccount
        {
            Type = OtpType.Hotp,
            Issuer = "ACME",
            Label = "user",
            SecretBase32 = "JBSWY3DPEHPK3PXP",
            HotpCounter = 5,
        };

        var result = _sut.ToUri( account );

        result.Should().Be( "otpauth://hotp/ACME:user?secret=JBSWY3DPEHPK3PXP&issuer=ACME&counter=5" );
    }

    [Theory]
    [InlineData( 7, "7" )]
    [InlineData( 8, "8" )]
    [InlineData( 9, "9" )]
    public void ToUri_WithCustomDigits( int digits, string expectedDigits )
    {
        var account = new OtpAccount
        {
            Type = OtpType.Totp,
            Issuer = "Test",
            Label = "user",
            SecretBase32 = "JBSWY3DPEHPK3PXP",
            Digits = digits,
        };

        var result = _sut.ToUri( account );

        result.Should().Be( $"otpauth://totp/Test:user?secret=JBSWY3DPEHPK3PXP&issuer=Test&digits={expectedDigits}" );
    }

    [Fact]
    public void ToUri_WithoutIssuer()
    {
        var account = new OtpAccount
        {
            Type = OtpType.Totp,
            Label = "user@email.com",
            SecretBase32 = "JBSWY3DPEHPK3PXP",
        };

        var result = _sut.ToUri( account );

        result.Should().Be( "otpauth://totp/user%40email.com?secret=JBSWY3DPEHPK3PXP&issuer=" );
    }

    [Theory]
    [InlineData( OtpAlgorithm.SHA224, "SHA224" )]
    [InlineData( OtpAlgorithm.SHA256, "SHA256" )]
    [InlineData( OtpAlgorithm.SHA384, "SHA384" )]
    [InlineData( OtpAlgorithm.SHA512, "SHA512" )]
    [InlineData( OtpAlgorithm.SHA3_224, "SHA3-224" )]
    [InlineData( OtpAlgorithm.SHA3_256, "SHA3-256" )]
    [InlineData( OtpAlgorithm.SHA3_384, "SHA3-384" )]
    [InlineData( OtpAlgorithm.SHA3_512, "SHA3-512" )]
    [InlineData( OtpAlgorithm.MD5, "MD5" )]
    public void ToUri_WithAlgorithm( OtpAlgorithm algorithm, string expectedAlgorithm )
    {
        var account = new OtpAccount
        {
            Type = OtpType.Totp,
            Issuer = "Test",
            Label = "user",
            SecretBase32 = "JBSWY3DPEHPK3PXP",
            Algorithm = algorithm,
        };

        var result = _sut.ToUri( account );

        result.Should().Be( $"otpauth://totp/Test:user?secret=JBSWY3DPEHPK3PXP&issuer=Test&algorithm={expectedAlgorithm}" );
    }
}
