using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace otpApp.Services;

public class LocalizationService : INotifyPropertyChanged
{
    private string _currentCulture = DetectSystemCulture();

    private static string DetectSystemCulture()
    {
        var uiCulture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        return uiCulture == "de" ? "de" : "en";
    }

    private static readonly Dictionary<string, Dictionary<string, string>> Strings = new()
    {
        ["en"] = new()
        {
            ["AppTitle"] = "otpApp",
            ["AppSubtitle"] = "Time-based One-Time Passwords",
            ["AddAccountButton"] = "+ Add Account",
            ["AddAccountDialogTitle"] = "Add Account",
            ["AddAccountDialogSubtitle"] = "Enter your account details below.",
            ["OtpType"] = "Type",
            ["Totp"] = "TOTP",
            ["Hotp"] = "HOTP",
            ["Counter"] = "Counter",
            ["NextCode"] = "Next",
            ["CounterPlaceholder"] = "Initial Counter",
            ["IssuerPlaceholder"] = "Issuer (e.g. GitHub)",
            ["LabelPlaceholder"] = "Label (e.g. user@email.com)",
            ["SecretPlaceholder"] = "Secret Key (Base32)",
            ["DigitsPlaceholder"] = "Digits",
            ["PeriodPlaceholder"] = "Period (s)",
            ["Save"] = "Save",
            ["Cancel"] = "Cancel",
            ["CopyTooltip"] = "Copy code",
            ["EditTooltip"] = "Edit account",
            ["DeleteTooltip"] = "Delete account",
            ["Language"] = "Language",
            ["English"] = "English",
            ["German"] = "German",
            ["CmdCopy"] = "Copy",
            ["CmdDelete"] = "Delete",
            ["CmdEdit"] = "Edit",
            ["CmdSave"] = "Save",
            ["CmdCancel"] = "Cancel",
            ["AppCmdAbout"] = "About otpApp",
            ["AppCmdHide"] = "Hidet otpApp",
            ["AppCmdHideOthers"] = "Hide Others",
            ["AppCmdShowAll"] = "Show All",
            ["AppCmdQuit"] = "Quit otpApp",
            ["CmdAddAccount"] = "Add Account",
            ["CmdAbout"] = "About",
            ["ImportFromClipboard"] = "New from Clipboard",
            ["ImportParseError"] = "No valid authentication data found in clipboard",
            ["AboutTooltip"] = "About",
            ["MenuFile"] = "File",
            ["MenuServices"] = "Services",
            ["MenuHelp"] = "Help",
            ["Seconds"] = "s",
            ["RemainingSeconds"] = "s",
        },
        ["de"] = new()
        {
            ["AppTitle"] = "otpApp",
            ["AppSubtitle"] = "Zeitbasierte Einmalpasswörter",
            ["AddAccountButton"] = "+ Konto hinzufügen",
            ["AddAccountDialogTitle"] = "Konto hinzufügen",
            ["AddAccountDialogSubtitle"] = "Geben Sie unten Ihre Kontodaten ein.",
            ["OtpType"] = "Typ",
            ["Totp"] = "TOTP",
            ["Hotp"] = "HOTP",
            ["Counter"] = "Zähler",
            ["NextCode"] = "Nächster",
            ["CounterPlaceholder"] = "Startzähler",
            ["IssuerPlaceholder"] = "Anbieter (z.B. GitHub)",
            ["LabelPlaceholder"] = "Bezeichnung (z.B. benutzer@email.de)",
            ["SecretPlaceholder"] = "Geheimschlüssel (Base32)",
            ["DigitsPlaceholder"] = "Stellen",
            ["PeriodPlaceholder"] = "Zeitraum (s)",
            ["Save"] = "Speichern",
            ["Cancel"] = "Abbrechen",
            ["CopyTooltip"] = "Code kopieren",
            ["EditTooltip"] = "Konto bearbeiten",
            ["DeleteTooltip"] = "Konto löschen",
            ["Language"] = "Sprache",
            ["English"] = "Englisch",
            ["German"] = "Deutsch",
            ["CmdCopy"] = "Kopieren",
            ["CmdDelete"] = "Löschen",
            ["CmdEdit"] = "Bearbeiten",
            ["CmdSave"] = "Speichern",
            ["CmdCancel"] = "Abbrechen",
            ["AppCmdAbout"] = "Über otpApp",
            ["AppCmdHide"] = "otpApp ausblenden",
            ["AppCmdHideOthers"] = "Andere ausblenden",
            ["AppCmdShowAll"] = "Alle anzeigen",
            ["AppCmdQuit"] = "otpApp beenden",
            ["CmdAddAccount"] = "Konto hinzufügen",
            ["CmdAbout"] = "Über",
            ["ImportFromClipboard"] = "Neu aus Zwischenablage",
            ["ImportParseError"] = "Keine gültigen Authentifizierungsdaten in der Zwischenablage",
            ["AboutTooltip"] = "Über",
            ["MenuFile"] = "Datei",
            ["MenuServices"] = "Dienste",
            ["MenuHelp"] = "Hilfe",
            ["Seconds"] = "s",
            ["RemainingSeconds"] = "s",
        }
    };

    public static LocalizationService Default { get; } = new();

    public string CurrentCulture
    {
        get => _currentCulture;
        set
        {
            if ( _currentCulture != value && Strings.ContainsKey( value ) )
            {
                _currentCulture = value;
                PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( string.Empty ) );
            }
        }
    }

    public string[] SupportedCultures => Strings.Keys.ToArray();

    public string AppTitle => GetString( "AppTitle" );
    public string AppSubtitle => GetString( "AppSubtitle" );
    public string AddAccountButton => GetString( "AddAccountButton" );
    public string AddAccountDialogTitle => GetString( "AddAccountDialogTitle" );
    public string AddAccountDialogSubtitle => GetString( "AddAccountDialogSubtitle" );
    public string IssuerPlaceholder => GetString( "IssuerPlaceholder" );
    public string LabelPlaceholder => GetString( "LabelPlaceholder" );
    public string SecretPlaceholder => GetString( "SecretPlaceholder" );
    public string DigitsPlaceholder => GetString( "DigitsPlaceholder" );
    public string PeriodPlaceholder => GetString( "PeriodPlaceholder" );
    public string Save => GetString( "Save" );
    public string Cancel => GetString( "Cancel" );
    public string CopyTooltip => GetString( "CopyTooltip" );
    public string EditTooltip => GetString( "EditTooltip" );
    public string DeleteTooltip => GetString( "DeleteTooltip" );
    public string Language => GetString( "Language" );
    public string English => GetString( "English" );
    public string German => GetString( "German" );
    public string CmdCopy => GetString( "CmdCopy" );
    public string CmdDelete => GetString( "CmdDelete" );
    public string CmdEdit => GetString( "CmdEdit" );
    public string CmdSave => GetString( "CmdSave" );
    public string CmdCancel => GetString( "CmdCancel" );
    public string AppCmdAbout => GetString();
    public string AppCmdHide => GetString();
    public string AppCmdHideOthers => GetString();
    public string AppCmdShowAll => GetString();
    public string AppCmdQuit => GetString();
    public string CmdAddAccount => GetString( "CmdAddAccount" );
    public string CmdAbout => GetString( "CmdAbout" );
    public string MenuFile => GetString( "MenuFile" );
    public string MenuServices => GetString( "MenuServices" );
    public string MenuHelp => GetString( "MenuHelp" );
    public string OtpType => GetString();
    public string Totp => GetString();
    public string Hotp => GetString();
    public string Counter => GetString();
    public string NextCode => GetString();
    public string CounterPlaceholder => GetString();
    public string ImportFromClipboard => GetString();
    public string ImportParseError => GetString();
    public string AboutTooltip => GetString( "AboutTooltip" );

    private string GetString( [CallerMemberName] string key = "" )
    {
        return Strings.TryGetValue( _currentCulture, out var lang ) && lang.TryGetValue( key, out var value )
            ? value
            : ( Strings["en"].TryGetValue( key, out var fallback ) ? fallback : key );
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
