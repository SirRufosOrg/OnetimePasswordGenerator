namespace otpApp.Services;

public class OtpUriSerializer : IOtpUriSerializer
{
    public string ToUri(OtpAccount account)
    {
        var type = account.Type == OtpType.Hotp ? "hotp" : "totp";
        var path = string.IsNullOrEmpty(account.Issuer) ? Uri.EscapeDataString(account.Label) : $"{Uri.EscapeDataString(account.Issuer)}:{Uri.EscapeDataString(account.Label)}";

        var query = $"secret={account.SecretBase32}&issuer={Uri.EscapeDataString(account.Issuer)}";

        if (account.Algorithm != OtpAlgorithm.SHA1)
        {
            var algo = account.Algorithm switch
            {
                OtpAlgorithm.SHA224 => "SHA224",
                OtpAlgorithm.SHA256 => "SHA256",
                OtpAlgorithm.SHA384 => "SHA384",
                OtpAlgorithm.SHA512 => "SHA512",
                OtpAlgorithm.SHA3_224 => "SHA3-224",
                OtpAlgorithm.SHA3_256 => "SHA3-256",
                OtpAlgorithm.SHA3_384 => "SHA3-384",
                OtpAlgorithm.SHA3_512 => "SHA3-512",
                OtpAlgorithm.MD5 => "MD5",
                _ => "SHA1"
            };
            query += $"&algorithm={algo}";
        }

        if (account.Digits != 6)
            query += $"&digits={account.Digits}";

        if (account.Type == OtpType.Totp)
        {
            if (account.Period != 30)
                query += $"&period={account.Period}";
        }
        else
        {
            query += $"&counter={account.HotpCounter}";
        }

        return $"otpauth://{type}/{path}?{query}";
    }
}