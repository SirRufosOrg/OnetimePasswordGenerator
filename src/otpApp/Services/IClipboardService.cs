namespace otpApp.Services;

public interface IClipboardService
{
    Task CopyToClipboardAsync(string text);
}
