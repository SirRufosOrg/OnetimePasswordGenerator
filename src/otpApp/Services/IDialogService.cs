namespace otpApp.Services;

public interface IDialogService
{
    void ShowAbout();
    Task<bool> ConfirmAsync(string message, string confirmText, string cancelText);
}
