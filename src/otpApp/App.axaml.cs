using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using otpApp.Platform;
using otpApp.Views;

namespace otpApp;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load( this );
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if ( ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop )
        {
            var vm = CompositionRoot.CreateMainViewModel();
            var mainWindow = new MainWindow { DataContext = vm };

            this.DataContext = vm;
            desktop.MainWindow = mainWindow;
            if ( OperatingSystem.IsMacOS() )
            {
                new MacAppMenuLocalizer( this, vm.Loc );
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}