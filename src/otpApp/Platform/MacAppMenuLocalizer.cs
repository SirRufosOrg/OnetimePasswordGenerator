using Avalonia;
using Avalonia.Controls;

namespace otpApp.Platform;

public class MacAppMenuLocalizer
{
    private readonly Application _app;
    private readonly LocalizationService _loc;
    private readonly NativeMenuItem? _servicesItem;
    private readonly NativeMenuItem? _hideAppItem;
    private readonly NativeMenuItem? _hideOthersItem;
    private readonly NativeMenuItem? _showAllItem;
    private readonly NativeMenuItem? _quitItem;

    public MacAppMenuLocalizer( Application app, LocalizationService loc )
    {
        _app = app;
        _loc = loc;
        var appMenu = NativeMenu.GetMenu( app ) ?? throw new InvalidOperationException( "Could not get application menu" );
        var menuItems = appMenu.Items.OfType<NativeMenuItem>().Reverse().ToList();

        _servicesItem = menuItems.FirstOrDefault( i => i.Header == "Services" );
        _hideAppItem = menuItems.FirstOrDefault( i => i.Header == "Hide " + ( app.Name ?? "Application" ) );
        _hideOthersItem = menuItems.FirstOrDefault( i => i.Header == "Hide Others" );
        _showAllItem = menuItems.FirstOrDefault( i => i.Header == "Show All" );
        _quitItem = menuItems.FirstOrDefault( i => i.Header == "Quit" );

        Localize();

        loc.PropertyChanged += ( _, _ ) => Localize();
    }

    private static NativeMenuItem? GetLastItem( List<NativeMenuItem> items, int offsetFromEnd )
    {
        var index = items.Count - 1 - offsetFromEnd;
        return index >= 0 ? items[index] : null;
    }

    private void Localize()
    {
        _app.Name = _loc.AppTitle;
        _servicesItem?.Header = _loc.MenuServices;
        _hideAppItem?.Header = _loc.AppCmdHide;
        _hideOthersItem?.Header = _loc.AppCmdHideOthers;
        _showAllItem?.Header = _loc.AppCmdShowAll;
        _quitItem?.Header = _loc.AppCmdQuit;
    }
}