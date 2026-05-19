namespace otpApp.Services;

public interface IFileDialogService
{
    Task<string?> OpenAndReadTextFileAsync();
}