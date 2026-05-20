using otpApp.Models;

namespace otpApp.Tests;

public class TotpServiceTests
{
    private static readonly string RfcSecret = "GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ";
    private readonly TotpService _sut = new();

    public static TheoryData<long, string> HotpTestVectors => new()
    {
        { 0, "755224" },
        { 1, "287082" },
        { 2, "359152" },
        { 3, "969429" },
        { 4, "338314" },
        { 5, "254676" },
        { 6, "287922" },
        { 7, "162583" },
        { 8, "399871" },
        { 9, "520489" },
    };

    [Theory]
    [MemberData( nameof( HotpTestVectors ) )]
    public void GenerateCode_Hotp_Sha1_KnownValues( long counter, string expected )
    {
        var account = new OtpAccount
        {
            Type = OtpType.Hotp,
            SecretBase32 = RfcSecret,
            Algorithm = OtpAlgorithm.SHA1,
            Digits = 6,
        };

        var result = _sut.GenerateCode( account, counter );

        result.Should().Be( expected );
    }

    [Fact]
    public void GenerateCode_Totp_Sha1_AtUnixEpoch_MatchesHotpCounter0()
    {
        var account = new OtpAccount
        {
            Type = OtpType.Totp,
            SecretBase32 = RfcSecret,
            Algorithm = OtpAlgorithm.SHA1,
            Digits = 6,
            Period = 30,
        };
        var epoch = DateTime.UnixEpoch;

        var result = _sut.GenerateCode( account, epoch );

        result.Should().Be( "755224" );
    }

    [Fact]
    public void GenerateCode_Totp_Sha1_At30s_MatchesHotpCounter1()
    {
        var account = new OtpAccount
        {
            Type = OtpType.Totp,
            SecretBase32 = RfcSecret,
            Algorithm = OtpAlgorithm.SHA1,
            Digits = 6,
            Period = 30,
        };
        var time = DateTime.UnixEpoch.AddSeconds( 30 );

        var result = _sut.GenerateCode( account, time );

        result.Should().Be( "287082" );
    }

    [Fact]
    public void GenerateCode_Hotp_Sha256()
    {
        var counter = 0L;
        var account = new OtpAccount
        {
            Type = OtpType.Hotp,
            SecretBase32 = RfcSecret,
            Algorithm = OtpAlgorithm.SHA256,
            Digits = 6,
        };

        var result = _sut.GenerateCode( account, counter );

        result.Should().HaveLength( 6 );
    }

    [Fact]
    public void GenerateCode_Hotp_Sha512()
    {
        var counter = 0L;
        var account = new OtpAccount
        {
            Type = OtpType.Hotp,
            SecretBase32 = RfcSecret,
            Algorithm = OtpAlgorithm.SHA512,
            Digits = 6,
        };

        var result = _sut.GenerateCode( account, counter );

        result.Should().HaveLength( 6 );
    }

    [Fact]
    public void GenerateCode_Totp_Sha256_AtUnixEpoch()
    {
        var account = new OtpAccount
        {
            Type = OtpType.Totp,
            SecretBase32 = RfcSecret,
            Algorithm = OtpAlgorithm.SHA256,
            Digits = 6,
            Period = 30,
        };

        var result = _sut.GenerateCode( account, DateTime.UnixEpoch );

        result.Should().HaveLength( 6 );
    }

    [Fact]
    public void GenerateCode_Totp_Sha512_AtUnixEpoch()
    {
        var account = new OtpAccount
        {
            Type = OtpType.Totp,
            SecretBase32 = RfcSecret,
            Algorithm = OtpAlgorithm.SHA512,
            Digits = 6,
            Period = 30,
        };

        var result = _sut.GenerateCode( account, DateTime.UnixEpoch );

        result.Should().HaveLength( 6 );
    }

    [Fact]
    public void GenerateCode_EightDigits()
    {
        var counter = 0L;
        var account = new OtpAccount
        {
            Type = OtpType.Hotp,
            SecretBase32 = RfcSecret,
            Algorithm = OtpAlgorithm.SHA1,
            Digits = 8,
        };

        var result = _sut.GenerateCode( account, counter );

        result.Should().HaveLength( 8 );
    }

    [Fact]
    public void RemainingSeconds_AtPeriodStart_ReturnsFullPeriod()
    {
        var account = new OtpAccount { Period = 30 };
        var timestamp = DateTime.UnixEpoch;

        var result = _sut.RemainingSeconds( account, timestamp );

        result.Should().Be( 30 );
    }

    [Fact]
    public void RemainingSeconds_MidPeriod()
    {
        var account = new OtpAccount { Period = 30 };
        var timestamp = DateTime.UnixEpoch.AddSeconds( 10 );

        var result = _sut.RemainingSeconds( account, timestamp );

        result.Should().Be( 20 );
    }
}
