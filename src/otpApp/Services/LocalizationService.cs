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
            ["AppTitle"] = "OTP App",
            ["AppSubtitle"] = "Time-based One-Time Passwords",
            ["AddAccountButton"] = "+ Add Account",
            ["AddAccountDialogTitle"] = "Add Account",
            ["AddAccountDialogSubtitle"] = "Enter your TOTP account details below.",
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
            ["CmdAddAccount"] = "Add Account",
            ["Seconds"] = "s",
            ["RemainingSeconds"] = "s",
        },
        ["de"] = new()
        {
            ["AppTitle"] = "OTP App",
            ["AppSubtitle"] = "Zeitbasierte Einmalpasswörter",
            ["AddAccountButton"] = "+ Konto hinzufügen",
            ["AddAccountDialogTitle"] = "Konto hinzufügen",
            ["AddAccountDialogSubtitle"] = "Geben Sie unten Ihre TOTP-Kontodaten ein.",
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
            ["CmdAddAccount"] = "Konto hinzufügen",
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
            if (_currentCulture != value && Strings.ContainsKey(value))
            {
                _currentCulture = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
            }
        }
    }

    public string[] SupportedCultures => Strings.Keys.ToArray();

    public string AppTitle => GetString("AppTitle");
    public string AppSubtitle => GetString("AppSubtitle");
    public string AddAccountButton => GetString("AddAccountButton");
    public string AddAccountDialogTitle => GetString("AddAccountDialogTitle");
    public string AddAccountDialogSubtitle => GetString("AddAccountDialogSubtitle");
    public string IssuerPlaceholder => GetString("IssuerPlaceholder");
    public string LabelPlaceholder => GetString("LabelPlaceholder");
    public string SecretPlaceholder => GetString("SecretPlaceholder");
    public string DigitsPlaceholder => GetString("DigitsPlaceholder");
    public string PeriodPlaceholder => GetString("PeriodPlaceholder");
    public string Save => GetString("Save");
    public string Cancel => GetString("Cancel");
    public string CopyTooltip => GetString("CopyTooltip");
    public string EditTooltip => GetString("EditTooltip");
    public string DeleteTooltip => GetString("DeleteTooltip");
    public string Language => GetString("Language");
    public string English => GetString("English");
    public string German => GetString("German");
    public string CmdCopy => GetString("CmdCopy");
    public string CmdDelete => GetString("CmdDelete");
    public string CmdEdit => GetString("CmdEdit");
    public string CmdSave => GetString("CmdSave");
    public string CmdCancel => GetString("CmdCancel");
    public string CmdAddAccount => GetString("CmdAddAccount");

    private string GetString(string key)
    {
        return Strings.TryGetValue(_currentCulture, out var lang) && lang.TryGetValue(key, out var value)
            ? value
            : (Strings["en"].TryGetValue(key, out var fallback) ? fallback : key);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
