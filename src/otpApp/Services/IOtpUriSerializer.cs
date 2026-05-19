namespace otpApp.Services;

public interface IOtpUriSerializer
{
    string ToUri(OtpAccount account);
}