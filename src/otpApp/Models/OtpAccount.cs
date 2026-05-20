namespace otpApp.Models;

public enum OtpType
{
    Totp,
    Hotp
}

public enum OtpAlgorithm
{
    SHA1,
    SHA224,
    SHA256,
    SHA384,
    SHA512,
    SHA3_224,
    SHA3_256,
    SHA3_384,
    SHA3_512,
    MD5
}

public class OtpAccount
{
    public int Id { get; set; }
    public OtpType Type { get; set; } = OtpType.Totp;
    public string Issuer { get; set; } = "";
    public string Label { get; set; } = "";
    public string SecretBase32 { get; set; } = "";
    public OtpAlgorithm Algorithm { get; set; } = OtpAlgorithm.SHA1;
    public int Digits { get; set; } = 6;
    public int Period { get; set; } = 30;
    public long HotpCounter { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public static string NormalizeSecret(string secret) =>
        secret.Trim()
            .Replace(" ", "")
            .Replace("-", "")
            .Replace("=", "")
            .ToUpperInvariant();
}
