using System.Security.Cryptography;

namespace otpApp.Models;

public class TotpService : ITotpService
{
    public string GenerateCode(OtpAccount account, DateTime? timestamp = null)
    {
        var time = timestamp ?? DateTime.UtcNow;
        var counter = GetTimeCounter(time, account.Period);
        return GenerateOtp(account, counter);
    }

    public string GenerateCode(OtpAccount account, long counter)
    {
        return GenerateOtp(account, GetCounterBytes(counter));
    }

    private string GenerateOtp(OtpAccount account, byte[] counter)
    {
        var secret = Base32Decode(account.SecretBase32);
        var hash = ComputeHmac(secret, counter, account.Algorithm);
        return Truncate(hash, account.Digits);
    }

    public int RemainingSeconds(OtpAccount account, DateTime? timestamp = null)
    {
        var time = timestamp ?? DateTime.UtcNow;
        var unix = (long)(time - DateTime.UnixEpoch).TotalSeconds;
        return account.Period - (int)(unix % account.Period);
    }

    private static byte[] GetTimeCounter(DateTime time, int period)
    {
        var unix = (long)(time - DateTime.UnixEpoch).TotalSeconds;
        var counter = unix / period;
        return GetCounterBytes(counter);
    }

    private static byte[] GetCounterBytes(long counter)
    {
        return BitConverter.GetBytes(counter).Reverse().ToArray();
    }

    private static byte[] ComputeHmac(byte[] secret, byte[] counter, OtpAlgorithm algorithm)
    {
        using var hmac = algorithm switch
        {
            OtpAlgorithm.SHA256 => (HMAC)new HMACSHA256(secret),
            OtpAlgorithm.SHA512 => new HMACSHA512(secret),
            _ => new HMACSHA1(secret)
        };
        return hmac.ComputeHash(counter);
    }

    private static string Truncate(byte[] hash, int digits)
    {
        var offset = hash[^1] & 0x0F;

        var binary = ((hash[offset] & 0x7F) << 24)
                     | ((hash[offset + 1] & 0xFF) << 16)
                     | ((hash[offset + 2] & 0xFF) << 8)
                     | (hash[offset + 3] & 0xFF);

        var otp = binary % (int)Math.Pow(10, digits);
        return otp.ToString(new string('0', digits));
    }

    private static byte[] Base32Decode(string base32)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        base32 = base32.Trim().ToUpperInvariant().Replace(" ", "").Replace("-", "");

        var bits = base32.Select(c => alphabet.IndexOf(c))
                         .Where(i => i >= 0)
                         .SelectMany(EnumerateBits)
                         .ToList();

        var bytes = new byte[bits.Count / 8];
        for (var i = 0; i < bytes.Length; i++)
        {
            bytes[i] = 0;
            for (var j = 0; j < 8; j++)
            {
                bytes[i] = (byte)((bytes[i] << 1) | (bits[i * 8 + j]));
            }
        }

        return bytes;
    }

    private static IEnumerable<int> EnumerateBits(int value)
    {
        for (var i = 4; i > 0; i--)
        {
            yield return (value >> (i - 1)) & 1;
        }
    }
}
