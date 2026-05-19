namespace otpApp.Models;

public interface ITotpService
{
    string GenerateCode(OtpAccount account, DateTime? timestamp = null);
    string GenerateCode(OtpAccount account, long counter);
    int RemainingSeconds(OtpAccount account, DateTime? timestamp = null);
}