using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;

namespace otpApp.Services;

public class FileDialogService : IFileDialogService
{
    private readonly LocalizationService _loc;

    public FileDialogService(LocalizationService loc)
    {
        _loc = loc;
    }

    public async Task<string?> OpenAndReadTextFileAsync()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return null;

        var window = desktop.MainWindow;
        if (window is null)
            return null;

        var files = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = _loc.ImportFileDialogTitle,
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType(_loc.TextFiles) { Patterns = ["*.txt"] },
                new FilePickerFileType(_loc.AllFiles) { Patterns = ["*.*"] },
            ]
        });

        var storageFile = files is { Count: > 0 } ? files[0] : null;
        if (storageFile is null)
            return null;

        await using var stream = await storageFile.OpenReadAsync();
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    public async Task<bool> SaveTextToFileAsync(string content)
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return false;

        var window = desktop.MainWindow;
        if (window is null)
            return false;

        var file = await window.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = _loc.ExportFileDialogTitle,
            DefaultExtension = "txt",
            FileTypeChoices =
            [
                new FilePickerFileType(_loc.TextFiles) { Patterns = ["*.txt"] },
                new FilePickerFileType(_loc.AllFiles) { Patterns = ["*.*"] },
            ]
        });

        if (file is null)
            return false;

        await using var stream = await file.OpenWriteAsync();
        using var writer = new StreamWriter(stream);
        await writer.WriteAsync(content);
        await writer.FlushAsync();
        return true;
    }
}
