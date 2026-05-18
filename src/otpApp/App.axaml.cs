using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using otpApp.Views;

namespace otpApp;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var vm = CompositionRoot.CreateMainViewModel();
            var mainWindow = new MainWindow { DataContext = vm };

            if (OperatingSystem.IsMacOS())
            {
                var aboutItem = new NativeMenuItem("About OTP App")
                {
                    Command = vm.ShowAboutCommand,
                };
                var appMenu = new NativeMenu();
                appMenu.Items.Add(aboutItem);

                var appMenuItem = new NativeMenuItem("");
                appMenuItem.Menu = appMenu;

                var menuBar = new NativeMenu();
                menuBar.Items.Add(appMenuItem);
                NativeMenu.SetMenu(mainWindow, menuBar);
            }

            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
