using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using otpApp.ViewModels;

namespace otpApp.Services;

public class DialogService : IDialogService
{
    private readonly LocalizationService _loc;

    public DialogService(LocalizationService loc)
    {
        _loc = loc;
    }

    public void ShowAbout()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var vm = new AboutWindowViewModel(_loc);
            var window = new Views.AboutWindow { DataContext = vm };
            window.ShowDialog(desktop.MainWindow!);
        }
    }
}
