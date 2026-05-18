using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

namespace otpApp.Services;

public class DialogService : IDialogService
{
    public void ShowAbout()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var window = new Views.AboutWindow();
            window.ShowDialog(desktop.MainWindow!);
        }
    }
}
