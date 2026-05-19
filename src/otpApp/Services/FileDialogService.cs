using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;

namespace otpApp.Services;

public class FileDialogService : IFileDialogService
{
    public async Task<string?> OpenAndReadTextFileAsync()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return null;

        var window = desktop.MainWindow;
        if (window is null)
            return null;

        var files = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Import Accounts",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("Text Files") { Patterns = ["*.txt"] },
                new FilePickerFileType("All Files") { Patterns = ["*.*"] },
            ]
        });

        var storageFile = files is { Count: > 0 } ? files[0] : null;
        if (storageFile is null)
            return null;

        await using var stream = await storageFile.OpenReadAsync();
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }
}