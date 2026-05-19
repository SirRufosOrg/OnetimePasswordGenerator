namespace otpApp.Models;

public enum OtpType
{
    Totp,
    Hotp
}

public enum OtpAlgorithm
{
    SHA1,
    SHA256,
    SHA512
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

    public string ToUri()
    {
        var type = Type == OtpType.Hotp ? "hotp" : "totp";
        var path = string.IsNullOrEmpty(Issuer) ? Uri.EscapeDataString(Label) : $"{Uri.EscapeDataString(Issuer)}:{Uri.EscapeDataString(Label)}";
        var algo = Algorithm switch
        {
            OtpAlgorithm.SHA256 => "SHA256",
            OtpAlgorithm.SHA512 => "SHA512",
            _ => "SHA1"
        };
        var query = $"secret={SecretBase32}&issuer={Uri.EscapeDataString(Issuer)}&algorithm={algo}&digits={Digits}";
        if (Type == OtpType.Totp)
            query += $"&period={Period}";
        else
            query += $"&counter={HotpCounter}";
        return $"otpauth://{type}/{path}?{query}";
    }
}
