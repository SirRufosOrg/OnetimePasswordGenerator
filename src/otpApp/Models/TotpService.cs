using System.Security.Cryptography;
using otpApp.Models.HashAlgorithms;

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
        return algorithm switch
        {
            OtpAlgorithm.SHA224 => HmacSha224(secret, counter),
            OtpAlgorithm.SHA256 => HmacBuiltIn(secret, counter, new HMACSHA256(secret)),
            OtpAlgorithm.SHA384 => HmacBuiltIn(secret, counter, new HMACSHA384(secret)),
            OtpAlgorithm.SHA512 => HmacBuiltIn(secret, counter, new HMACSHA512(secret)),
            OtpAlgorithm.SHA3_224 => HmacSha3(secret, counter, 224),
            OtpAlgorithm.SHA3_256 => HmacSha3(secret, counter, 256),
            OtpAlgorithm.SHA3_384 => HmacSha3(secret, counter, 384),
            OtpAlgorithm.SHA3_512 => HmacSha3(secret, counter, 512),
            OtpAlgorithm.MD5 => HmacBuiltIn(secret, counter, new HMACMD5(secret)),
            _ => HmacBuiltIn(secret, counter, new HMACSHA1(secret))
        };
    }

    private static byte[] HmacBuiltIn(byte[] secret, byte[] counter, HMAC hmac)
    {
        using (hmac)
            return hmac.ComputeHash(counter);
    }

    private static byte[] HmacSha224(byte[] key, byte[] data)
    {
        const int blockSize = 64;
        key = HashKeyIfNeeded(key, blockSize, () => new Sha224().ComputeHash(key));

        var ipad = new byte[blockSize];
        var opad = new byte[blockSize];
        for (var i = 0; i < blockSize; i++)
        {
            ipad[i] = (byte)(key[i] ^ 0x36);
            opad[i] = (byte)(key[i] ^ 0x5C);
        }

        using var innerHash = new Sha224();
        innerHash.TransformBlock(ipad, 0, blockSize, null, 0);
        innerHash.TransformFinalBlock(data, 0, data.Length);
        var innerResult = innerHash.Hash!;

        using var outerHash = new Sha224();
        outerHash.TransformBlock(opad, 0, blockSize, null, 0);
        outerHash.TransformFinalBlock(innerResult, 0, innerResult.Length);
        return outerHash.Hash!;
    }

    private static byte[] HmacSha3(byte[] key, byte[] data, int hashBits)
    {
        var rate = (1600 - hashBits * 2) / 8;
        key = HashKeyIfNeeded(key, rate, () => new Sha3(hashBits).ComputeHash(key));

        var ipad = new byte[rate];
        var opad = new byte[rate];
        for (var i = 0; i < rate; i++)
        {
            ipad[i] = (byte)(key[i] ^ 0x36);
            opad[i] = (byte)(key[i] ^ 0x5C);
        }

        using var innerHash = new Sha3(hashBits);
        innerHash.TransformBlock(ipad, 0, rate, null, 0);
        innerHash.TransformFinalBlock(data, 0, data.Length);
        var innerResult = innerHash.Hash!;

        using var outerHash = new Sha3(hashBits);
        outerHash.TransformBlock(opad, 0, rate, null, 0);
        outerHash.TransformFinalBlock(innerResult, 0, innerResult.Length);
        return outerHash.Hash!;
    }

    private static byte[] HashKeyIfNeeded(byte[] key, int blockSize, Func<byte[]> hashKey)
    {
        if (key.Length > blockSize)
            return hashKey();
        if (key.Length < blockSize)
        {
            var padded = new byte[blockSize];
            Array.Copy(key, padded, key.Length);
            return padded;
        }
        return key;
    }

    private static string Truncate(byte[] hash, int digits)
    {
        var offset = hash[^1] & 0x0F;
        if (offset + 4 > hash.Length)
            offset = hash.Length - 4;

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
        for (var i = 5; i > 0; i--)
        {
            yield return (value >> (i - 1)) & 1;
        }
    }
}
