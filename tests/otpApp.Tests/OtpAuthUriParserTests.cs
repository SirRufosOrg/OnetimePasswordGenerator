using otpApp.Models;

namespace otpApp.Tests;

public class OtpAuthUriParserTests
{
    [Fact]
    public void Parse_ValidTotpUri_ReturnsAccount()
    {
        var result = OtpAuthUriParser.Parse( "otpauth://totp/GitHub:user@email.com?secret=JBSWY3DPEHPK3PXP" );

        result.Should().NotBeNull();
        result!.Type.Should().Be( OtpType.Totp );
        result.Issuer.Should().Be( "GitHub" );
        result.Label.Should().Be( "user@email.com" );
        result.SecretBase32.Should().Be( "JBSWY3DPEHPK3PXP" );
        result.Algorithm.Should().Be( OtpAlgorithm.SHA1 );
        result.Digits.Should().Be( 6 );
        result.Period.Should().Be( 30 );
    }

    [Fact]
    public void Parse_ValidHotpUri_ReturnsAccount()
    {
        var result = OtpAuthUriParser.Parse( "otpauth://hotp/ACME:user@email.com?secret=JBSWY3DPEHPK3PXP&counter=5" );

        result.Should().NotBeNull();
        result!.Type.Should().Be( OtpType.Hotp );
        result.Issuer.Should().Be( "ACME" );
        result.Label.Should().Be( "user@email.com" );
        result.HotpCounter.Should().Be( 5 );
    }

    [Fact]
    public void Parse_IssuerInQuery_OverridesPathIssuer()
    {
        var result = OtpAuthUriParser.Parse( "otpauth://totp/PathIssuer:label?secret=JBSWY3DPEHPK3PXP&issuer=QueryIssuer" );

        result.Should().NotBeNull();
        result!.Issuer.Should().Be( "QueryIssuer" );
    }

    [Fact]
    public void Parse_Defaults_WhenOmitted()
    {
        var result = OtpAuthUriParser.Parse( "otpauth://totp/label?secret=JBSWY3DPEHPK3PXP" );

        result.Should().NotBeNull();
        result!.Algorithm.Should().Be( OtpAlgorithm.SHA1 );
        result.Digits.Should().Be( 6 );
        result.Period.Should().Be( 30 );
    }

    [Fact]
    public void Parse_SHA256()
    {
        var result = OtpAuthUriParser.Parse( "otpauth://totp/label?secret=JBSWY3DPEHPK3PXP&algorithm=SHA256" );

        result.Should().NotBeNull();
        result!.Algorithm.Should().Be( OtpAlgorithm.SHA256 );
    }

    [Fact]
    public void Parse_SHA512()
    {
        var result = OtpAuthUriParser.Parse( "otpauth://totp/label?secret=JBSWY3DPEHPK3PXP&algorithm=SHA512" );

        result.Should().NotBeNull();
        result!.Algorithm.Should().Be( OtpAlgorithm.SHA512 );
    }

    [Fact]
    public void Parse_EightDigits()
    {
        var result = OtpAuthUriParser.Parse( "otpauth://totp/label?secret=JBSWY3DPEHPK3PXP&digits=8" );

        result.Should().NotBeNull();
        result!.Digits.Should().Be( 8 );
    }

    [Fact]
    public void Parse_Period60()
    {
        var result = OtpAuthUriParser.Parse( "otpauth://totp/label?secret=JBSWY3DPEHPK3PXP&period=60" );

        result.Should().NotBeNull();
        result!.Period.Should().Be( 60 );
    }

    [Fact]
    public void Parse_InvalidUri_ReturnsNull()
    {
        var result = OtpAuthUriParser.Parse( "not a uri at all" );

        result.Should().BeNull();
    }

    [Fact]
    public void Parse_NonOtpAuthScheme_ReturnsNull()
    {
        var result = OtpAuthUriParser.Parse( "https://example.com/path?secret=JBSWY3DPEHPK3PXP" );

        result.Should().BeNull();
    }

    [Fact]
    public void Parse_EmptyString_ReturnsNull()
    {
        var result = OtpAuthUriParser.Parse( "" );

        result.Should().BeNull();
    }

    [Fact]
    public void Parse_NormalizesSecret()
    {
        var result = OtpAuthUriParser.Parse( "otpauth://totp/label?secret=jbs wy-3dpe=hk3p-xp" );

        result.Should().NotBeNull();
        result!.SecretBase32.Should().Be( "JBSWY3DPEHK3PXP" );
    }
}
