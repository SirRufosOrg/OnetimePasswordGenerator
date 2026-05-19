using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using otpApp.ViewModels;

namespace otpApp.Services;

public class DialogService : IDialogService
{
    public void ShowAbout()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var vm = new AboutWindowViewModel();
            var window = new Views.AboutWindow { DataContext = vm };
            window.ShowDialog(desktop.MainWindow!);
        }
    }
}
