using System;
using LiteDB;

namespace otpApp.Models;

public enum OtpAlgorithm
{
    SHA1,
    SHA256,
    SHA512
}

public class OtpAccount
{
    public int Id { get; set; }
    public string Issuer { get; set; } = "";
    public string Label { get; set; } = "";
    public string SecretBase32 { get; set; } = "";
    public OtpAlgorithm Algorithm { get; set; } = OtpAlgorithm.SHA1;
    public int Digits { get; set; } = 6;
    public int Period { get; set; } = 30;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
