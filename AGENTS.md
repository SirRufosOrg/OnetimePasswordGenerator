# AGENTS.md

## Summary

TOTP/HOTP-Authenticator-App mit Avalonia/ReactiveUI/Zafiro – Code-Generierung, Bearbeitung, i18n, About-Dialog und macOS-Menüanbindung.

## Constraints & Preferences

- macOS: About im systemeigenen Anwendungsmenü via `NativeMenu.Menu` auf `<Application>` in `App.axaml`
- `Application.DataContext` wird in `App.axaml.cs` auf `MainWindowViewModel` gesetzt (für Bindings im NativeMenu)
- Compiled Bindings (`AvaloniaUseCompiledBindingsByDefault=true`)
- `[Reactive]`-Attribute + `IEnhancedCommand` + `CompositeDisposable`-Patterns
- ViewModels dürfen keine Avalonia-Typen referenzieren (Services übernehmen Plattform-Logik)
- Zusätzlich zu TOTP: HOTP (Counter-basiert) in Modell, Service und UI

## Progress

### Done

- **ProgressBar**: `Minimum="0" Maximum="1"` gesetzt
- **Code sofort beim Start**: `CurrentCode` im Konstruktor von `AccountItemViewModel`
- **Edit-Funktion**: ✎-Button pro Karte, umschaltbarer Edit-Modus (Issuer/Label/Secret/Algorithm/Digits/Period + Type/Counter für HOTP)
- **Digits auf 6/8 beschränkt**: NumericUpDown `Min=6 Max=8 Inc=2`, SaveEditCommand validiert via `canSave`
- **i18n**: `LocalizationService` EN/DE, `Loc`-Property auf `ViewModelBase`, alle XAML-Strings via Binding
- **Sprachauswahl**: ComboBox im Header, schaltet `CurrentCulture` um
- **Systemsprache**: `CultureInfo.CurrentUICulture.TwoLetterISOLanguageName` beim Start
- **Globale Usings**: `ImplicitUsings` + 10 `<Using>`-Items in csproj
- **Zafiro-Review-Fixes**: DI, `ThrownExceptions`, `WhenAnyValue`, Lokalisierung für CmdAbout
- **AboutWindow**: erstellt mit Versionsanzeige, `ShowAboutCommand` öffnet modalen Dialog
- **About-Button**: ?-Button im Header, auf macOS via `OperatingSystem.IsMacOS()` ausgeblendet
- **macOS About im Anwendungsmenü**: `NativeMenu.Menu` auf `<Application>` in `App.axaml`, `x:DataType="vm:MainWindowViewModel"`, `this.DataContext = vm`. `MacAppMenuLocalizer` (in `Platform/`) lokalisiert Services/Hide/Quit-Einträge live.
- **MVVM-Fixes**: Avalonia-Typen aus ViewModels entfernt (`IClipboardService`/`ClipboardService` + `IDialogService`/`DialogService`). Speicherleck behoben (`Dispose()` vor Clear in `LoadAccounts()`). Edit ohne Full-Reload (`RefreshAfterEdit()`). AddDialog `Reset()` statt Replace. `AccountRepository`: single `LiteDatabase`-Instanz.
- **Display-Update**: `{Binding Account.Issuer}` (Zwei-Ebenen) durch reactive Flat-Properties (`DisplayIssuer`/`DisplayLabel`) ersetzt, `RefreshAfterEdit()` aktualisiert alle.
- **TOTP + HOTP**:
  - `OtpType`-Enum (Totp, Hotp), `OtpAccount.Type` + `HotpCounter`
  - `TotpService.GenerateCode(OtpAccount, long counter)`-Overload für HOTP
  - Add-Dialog: Type-Auswahl, bedingte Period (TOTP) / Counter (HOTP)
  - Karte: TOTP mit ProgressBar + Sekunden, HOTP mit Counter + „Next"-Button
  - Edit-Modus: Typ umschaltbar, Period/Counter bedingt sichtbar via `IsEditTotp`
  - Timer nur für TOTP, separater `_timerDisposables` für Start/Stopp
  - `RefreshAfterEdit()`: aktualisiert Display-Properties + Timer + Code

### Blocked

- *(none)*

## Key Decisions

- `LocalizationService` als statischer Singleton (`Default`) statt DI – Zugriff über `ViewModelBase.Loc`
- Inline-Edit auf der Karte statt separatem Dialog
- About per `ShowDialog()` statt Zafiro-Interaktion
- `NativeMenu.Menu` auf `<Application>` (nicht Window) – macOS systemweite Menüleiste; `NativeMenu.AppMenu` existiert nicht
- `MacAppMenuLocalizer` in `Platform/` (nicht Services/ oder Views/) – macOS-spezifische Systemintegration
- `RefreshAfterEdit()` statt `NotifyAccountUpdated()` – stellt gesamten Karten-Zustand neu dar (IsTotp, Timer, Code)
- HOTP-Counter wird lokal im ViewModel geführt und via `AdvanceCounter`-Callback persistiert

## Relevant Files

- `src/otpApp/App.axaml` – `NativeMenu.Menu` (Zeile 29–33), `x:DataType` (Zeile 5)
- `src/otpApp/App.axaml.cs` – `this.DataContext = vm` (Zeile 24), `MacAppMenuLocalizer`-Instantiierung (Zeile 27)
- `src/otpApp/Platform/MacAppMenuLocalizer.cs` – macOS-Menü-Lokalisierung
- `src/otpApp/Views/MainWindow.axaml` – Add-Dialog, Karten-Template (TOTP/HOTP), Edit-Modus
- `src/otpApp/ViewModels/MainWindowViewModel.cs` – `ShowAboutCommand`, `EditAccount`, `AdvanceCounter`
- `src/otpApp/ViewModels/AccountItemViewModel.cs` – TOTP/HOTP-Display, Timer, `RefreshAfterEdit`, Counter-Advance, `canSave`-Validierung
- `src/otpApp/ViewModels/AddAccountViewModel.cs` – Type/Counter-Felder, bedingte Validierung, `Reset()`
- `src/otpApp/Models/OtpAccount.cs` – `OtpType`-Enum, `Type`, `HotpCounter`
- `src/otpApp/Models/TotpService.cs` – `GenerateCode(OtpAccount, long counter)`-Overload
- `src/otpApp/Services/IClipboardService.cs` + `ClipboardService.cs`
- `src/otpApp/Services/IDialogService.cs` + `DialogService.cs`
- `src/otpApp/Services/AccountRepository.cs` – single `LiteDatabase`-Instanz
- `src/otpApp/Services/LocalizationService.cs` – EN/DE-Strings inkl. AppCmdAbout, OtpType, Counter, NextCode
- `src/otpApp/CompositionRoot.cs` – DI-Setup (Services + ViewModels)
