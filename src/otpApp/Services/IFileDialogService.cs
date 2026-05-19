namespace otpApp.Services;

public interface IFileDialogService
{
    Task<string?> OpenAndReadTextFileAsync();
    Task<bool> SaveTextToFileAsync(string content);
}