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
            .AddSingleton<LocalizationService>()
            .AddSingleton<IAccountRepository>(new AccountRepository($"Filename={dbPath};Connection=direct"))
            .AddSingleton<ITotpService, TotpService>()
            .AddSingleton<IClipboardService, ClipboardService>()
            .AddSingleton<IDialogService, DialogService>()
            .AddSingleton<IFileDialogService, FileDialogService>()
            .AddSingleton<IPlatformService, PlatformService>()
            .AddSingleton<IOtpUriSerializer, OtpUriSerializer>()
            .AddTransient<AddAccountViewModel>()
            .AddSingleton<MainWindowViewModel>();

        var sp = services.BuildServiceProvider();
        return sp.GetRequiredService<MainWindowViewModel>();
    }
}
