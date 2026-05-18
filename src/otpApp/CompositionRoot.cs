using Microsoft.Extensions.DependencyInjection;
using otpApp.Services;
using otpApp.ViewModels;

namespace otpApp;

public static class CompositionRoot
{
    public static MainWindowViewModel CreateMainViewModel()
    {
        var services = new ServiceCollection();

        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "otpApp", "accounts.db");
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

        services
            .AddSingleton(new AccountRepository($"Filename={dbPath};Connection=direct"))
            .AddSingleton<TotpService>()
            .AddSingleton<IClipboardService, ClipboardService>()
            .AddSingleton<IDialogService, DialogService>()
            .AddTransient<AddAccountViewModel>()
            .AddSingleton<MainWindowViewModel>();

        var sp = services.BuildServiceProvider();
        return sp.GetRequiredService<MainWindowViewModel>();
    }
}
