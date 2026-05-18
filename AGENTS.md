# AGENTS.md

## Summary

TOTP-Authenticator-App mit Avalonia/ReactiveUI/Zafiro – Code-Generierung, Bearbeitung, i18n, About-Dialog und macOS-Menüanbindung.

## Constraints & Preferences
- macOS: About im systemeigenen Anwendungsmenü via `NativeMenu.Menu` auf `<Application>` in App.axaml
- `Application.DataContext` wird in App.axaml.cs auf MainWindowViewModel gesetzt (für Bindings im NativeMenu)
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
- **macOS About im Anwendungsmenü**: `NativeMenu.Menu` auf `<Application>` (App.axaml) + `x:DataType="vm:MainWindowViewModel"` + `this.DataContext = vm` in App.axaml.cs

### Blocked
- *(none)*

## Key Decisions
- LocalizationService als statischer Singleton (Default) statt DI – Zugriff über ViewModelBase.Loc
- IServiceProvider in MainWindowViewModel für AddAccountViewModel (DI)
- Inline-Edit auf der Karte statt separatem Dialog
- About per ShowDialog() statt Zafiro-Interaktion
- **macOS NativeMenu auf Application (nicht Window)**: `NativeMenu.Menu` attached property auf `<Application>` in App.axaml, da macOS eine systemweite Menüleiste hat. Bindings lösen über `Application.DataContext` (= MainWindowViewModel) auf.
- Window.NativeMenu.Menu → erzeugt separates Menü (nicht App-Menü). Application.NativeMenu und NativeMenu.AppMenu existieren nicht in Avalonia 12.

## Relevant Files
- `src/otpApp/App.axaml` – NativeMenu.Menu (Zeile 29–33), x:DataType (Zeile 5)
- `src/otpApp/App.axaml.cs` – this.DataContext = vm (Zeile 24)
- `src/otpApp/Views/MainWindow.axaml` – UI, Header, Add-Dialog
- `src/otpApp/ViewModels/MainWindowViewModel.cs` – ShowAboutCommand
- `src/otpApp/Services/LocalizationService.cs` – EN/DE-Strings
- `src/otpApp/ViewModels/AccountItemViewModel.cs` – Edit-Logik, Timer, Code-Generierung
- `src/otpApp/CompositionRoot.cs` – DI-Setup
