# AGENTS.md

## Summary

TOTP-Authenticator-App mit Avalonia/ReactiveUI/Zafiro – Code-Generierung, Bearbeitung, i18n, About-Dialog und macOS-Menüanbindung.

## Constraints & Preferences
- macOS: About im systemeigenen Anwendungsmenü via `NativeMenu.Menu` in XAML
- Englisch als Fallback-Sprache, Systemsprache wird beim Start erkannt
- Compiled Bindings (`AvaloniaUseCompiledBindingsByDefault=true`)
- `[Reactive]`-Attribute + `IEnhancedCommand` + `CompositeDisposable`-Patterns

## Progress

### Done
- ProgressBar: Minimum="0" Maximum="1" gesetzt
- Code sofort beim Start: CurrentCode im Konstruktor von AccountItemViewModel
- Edit-Funktion: ✎-Button pro Karte, umschaltbarer Edit-Modus (Issuer/Label/Secret/Algorithm/Digits/Period)
- NumericUpDown: FormatString="0" + Increment="1"
- i18n: LocalizationService (EN/DE), Loc-Property auf ViewModelBase, alle XAML-Strings via Binding
- Sprachauswahl: ComboBox im Header, schaltet CurrentCulture um
- Systemsprache: CultureInfo.CurrentUICUICulture beim Start
- Globale Usings: ImplicitUsings + 10 Using-Items in csproj
- Zafiro-Review: DI, ThrownExceptions, WhenAnyValue, CmdAbout-Lokalisierung gefixed
- AboutWindow: erstellt, ShowAboutCommand öffnet modalen Dialog
- About-Button: ?-Button im Header, auf macOS via OperatingSystem.IsMacOS() ausgeblendet
- **macOS About im Anwendungsmenü**: via `NativeMenu.Menu` in XAML (gebunden an ShowAboutCommand)

### Blocked
- *(none)*

## Key Decisions
- LocalizationService als statischer Singleton (Default) statt DI – Zugriff über ViewModelBase.Loc
- IServiceProvider in MainWindowViewModel für AddAccountViewModel (DI)
- Inline-Edit auf der Karte statt separatem Dialog
- About per ShowDialog() statt Zafiro-Interaktion
- NativeMenu.Menu in XAML für macOS App-Menü (nicht programmatisch)

## Relevant Files
- `src/otpApp/Views/MainWindow.axaml` – NativeMenu.Menu + UI
- `src/otpApp/App.axaml.cs` – App-Bootstrapping (DataContext-Setup)
- `src/otpApp/ViewModels/MainWindowViewModel.cs` – ShowAboutCommand
- `src/otpApp/Services/LocalizationService.cs` – EN/DE-Strings
- `src/otpApp/ViewModels/AccountItemViewModel.cs` – Edit-Logik, Timer, Code-Generierung
- `src/otpApp/CompositionRoot.cs` – DI-Setup
