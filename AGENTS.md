# AGENTS.md

## Summary

TOTP/HOTP-Authenticator-App mit Avalonia/ReactiveUI/Zafiro – Code-Generierung, Bearbeitung, i18n, About-Dialog, macOS-Menüanbindung, Import/Export und Duplikaterkennung.

## Constraints & Preferences

- macOS: About im systemeigenen Anwendungsmenü via `NativeMenu.Menu` auf `<Application>` in `App.axaml`
- `Application.DataContext` wird in `App.axaml.cs` auf `MainWindowViewModel` gesetzt (für Bindings im NativeMenu)
- Compiled Bindings (`AvaloniaUseCompiledBindingsByDefault=true`)
- `[Reactive]`-Attribute + `IEnhancedCommand` + `CompositeDisposable`-Patterns
- ViewModels dürfen keine Avalonia-Typen referenzieren (Services übernehmen Plattform-Logik)
- TOTP + HOTP (Counter-basiert) in Modell, Service und UI
- Digits nur 6 oder 8 (NumericUpDown Min/Max + Validierung)
- Code-Anzeige per `CodeSplitterConverter` + `ItemsControl` mit 10px Spacing: "xxx xxx" (6) / "xxx xxx xx" (8)

## Progress

### Done

- **ProgressBar**: `Minimum="0" Maximum="1"`
- **Code sofort beim Start**: `CurrentCode` im Konstruktor von `AccountItemViewModel`
- **Edit-Funktion**: ✎-Button pro Karte, umschaltbarer Edit-Modus (alle Felder)
- **Digits auf 6/8 beschränkt**: NumericUpDown + Validierung in beiden ViewModels
- **i18n**: `LocalizationService` EN/DE, via DI (`base(loc)` auf `ViewModelBase`), Sprachauswahl per ComboBox
- **Globale Usings**: `ImplicitUsings` + `<Using>`-Items in csproj
- **AboutWindow**: `AboutWindowViewModel` mit Version + `CloseCommand`, modaler Dialog via `DialogService`
- **macOS About im Anwendungsmenü**: `NativeMenu.Menu` auf `<Application>`, `MacAppMenuLocalizer` (positionale Header-Suche)
- **MVVM-Refactoring (Review-Fixes)**:
  - `TotpService`: HMACs mit `using var` disposen
  - `AccountRepository`: `IDisposable` für `LiteDatabase`
  - `IPlatformService` für `ShowAboutButton` (statt `OperatingSystem.IsMacOS()` im VM)
  - `LocalizationService` via DI (kein statischer Singleton mehr)
  - `IAccountRepository` + `ITotpService` Interfaces
  - Action-Callbacks → `Interaction<,>` (Delete, Edit, CounterAdvanced)
  - `PropertyChanged`-Leak in `MainWindow.axaml.cs` gefixt
  - `ToUri()` → `IOtpUriSerializer`-Service (Model enthält kein Serialisierungs-API)
  - Add-Dialog via `ContentControl`/`DataTemplate` statt Property-Path-Bindings
  - `MacAppMenuLocalizer`: positionale statt String-basierte Header-Suche
- **Import aus Zwischenablage**: `IClipboardService`, `OtpAuthUriParser`, `PopulateFrom()`, SplitButton-Flyout
- **Import aus Datei**: `IFileDialogService`/`FileDialogService`, zeilenweises Parsen, Batch-Insert
- **Export in Datei**: `IOtpUriSerializer`, `ExportToFileCommand` – alle Accounts als `otpauth://`-URIs
- **Duplikaterkennung**: `GetAccountKey()` vergleicht Type/Issuer/Label/Secret/Algorithm/Digits + Period|Counter je nach Type; auch beim manuellen Speichern (Add-Dialog)
- **Secret-Normalisierung**: `OtpAccount.NormalizeSecret()` – uppercase, ohne Leerzeichen/Dashes/Padding, an allen Speicher-Stellen + im Vergleich
- **Löschbestätigung**: `ConfirmDialog` + `IDialogService.ConfirmAsync()`
- **ToolTips**: an allen Eingabefeldern (Edit + Add Dialog) mit lokalisierter Beschreibung
- **About lokalisiert**: Titel, AppName, Subtitle, Description über `Loc.*`
- **Code-Trennung**: `CodeSplitterConverter` (neu) gruppiert von rechts in Dreierblöcke, `ItemsControl` mit horizontalem `StackPanel` + `Spacing="10"`

### In Progress

- *(none)*

### Blocked

- *(none)*

## Key Decisions

- `LocalizationService` per DI (kein statischer Singleton mehr) – alle ViewModels erhalten via `base(loc)`
- Inline-Edit auf der Karte statt separatem Dialog
- About per `ShowDialog()` statt Zafiro-Interaktion
- `NativeMenu.Menu` auf `<Application>` (nicht Window) – macOS systemweite Menüleiste
- `MacAppMenuLocalizer` in `Platform/` – positionale Header-Suche (letzte 5 Items) statt String-Matching
- `RefreshAfterEdit()` statt `NotifyAccountUpdated()` – stellt gesamten Karten-Zustand neu dar
- HOTP-Counter lokal im ViewModel + via `Interaction<,>` persistiert
- `OtpAuthUriParser` als statischer Parser ohne DI (kein Zustand)
- `IOtpUriSerializer` als Service (Serialisierung aus Model ausgelagert)
- `IAccountRepository` + `ITotpService` über Interfaces (konkrete Typen aus ViewModels verbannt)
- `Interaction<,>` statt Action-Callbacks für Parent-Child-Kommunikation
- `CodeSplitterConverter` + `ItemsControl` statt String-Manipulation für Code-Darstellung

## Next Steps

- *(none)*

## Critical Context

- `NativeMenu.Menu` auf `Application` erlaubt compiled Bindings via `x:DataType` + `this.DataContext = vm`
- `{Binding Account.Issuer}` (Zwei-Ebenen) wird in Avalonia nicht durch `RaisePropertyChanged` aktualisiert → immer reaktive Flat-Properties (`DisplayIssuer`) nutzen
- `CompositeDisposable` für Timer (`_timerDisposables`) getrennt von Haupt-`_disposables` für Stopp bei Typ-Wechsel
- In Avalonia 12 heißt die Clipboard-Lesemethode `TryGetTextAsync()` (Extension auf `IClipboard`)
- `FilePickerOpenOptions`/`FilePickerFileType` in `Avalonia.Platform.Storage`; `StorageProvider` auf `TopLevel`
- `ShowAddDialog = true` triggert `ContentScrollViewer.ScrollToEnd()` – Code-behind in `MainWindow.xaml.cs`
- `AccountRepository: IDisposable` – wird in `MainWindowViewModel.Dispose()` gecallt
- `GetAccountKey()` zentraler Schlüssel für Duplikat-Erkennung (Import + Add-Dialog)
- `OtpAccount.NormalizeSecret()` vor Speicherung und Vergleich aufgerufen
- `CodeSplitterConverter` in `App.axaml` global als `{StaticResource CodeSplitter}` registriert
- `ConfirmDialog` hat parameterlosen Constructor für XAML-Loader + parameterisierten für Aufruf

## Relevant Files

- `src/otpApp/App.axaml` – `NativeMenu.Menu`, `x:DataType`, Converter-Resources (CodeSplitter)
- `src/otpApp/App.axaml.cs` – `this.DataContext = vm`, `MacAppMenuLocalizer`
- `src/otpApp/Platform/MacAppMenuLocalizer.cs` – positionale Header-Suche
- `src/otpApp/Views/MainWindow.axaml` – ItemsControl für Code, ContentControl für Add-Dialog, ToolTips, ConfirmDelete, NativeMenu-Einträge (Import/Export)
- `src/otpApp/Views/MainWindow.axaml.cs` – PropertyChanged-handling (abgemeldet bei DataContext-Wechsel)
- `src/otpApp/Views/AboutWindow.axaml` – komplett lokalisiert via `Loc.*`
- `src/otpApp/Views/ConfirmDialog.axaml` + `.axaml.cs` – Löschbestätigung
- `src/otpApp/ViewModels/MainWindowViewModel.cs` – Export, Import, Duplikat-Prüfung, ConfirmDelete, IAccountRepository/IOtpUriSerializer/IPlatformService
- `src/otpApp/ViewModels/AccountItemViewModel.cs` – TOTP/HOTP-Display, Timer, `Interaction<,>` (Delete/Edit/CounterAdvanced), Code an `CurrentCode` unformatiert
- `src/otpApp/ViewModels/AddAccountViewModel.cs` – Type/Counter, `PopulateFrom(OtpAccount)`, `Reset()`
- `src/otpApp/ViewModels/AboutWindowViewModel.cs` – Version + CloseCommand
- `src/otpApp/ViewModels/ViewModelBase.cs` – `Loc` via Constructor-Injection (statt statischem Singleton)
- `src/otpApp/Models/OtpAccount.cs` – `OtpType`, `NormalizeSecret()`, kein `ToUri()` mehr
- `src/otpApp/Models/ITotpService.cs` + `TotpService.cs` – Interface + `using var` HMACs
- `src/otpApp/Models/OtpAuthUriParser.cs` – statischer Parser
- `src/otpApp/Services/IAccountRepository.cs` + `AccountRepository.cs` – Interface, `IDisposable`
- `src/otpApp/Services/IOtpUriSerializer.cs` + `OtpUriSerializer.cs` – Serialisierung `otpauth://`
- `src/otpApp/Services/IPlatformService.cs` + `PlatformService.cs` – `ShowAboutButton`
- `src/otpApp/Services/IClipboardService.cs` + `ClipboardService.cs`
- `src/otpApp/Services/IFileDialogService.cs` + `FileDialogService.cs`
- `src/otpApp/Services/IDialogService.cs` + `DialogService.cs` – `ShowAbout()`, `ConfirmAsync()`
- `src/otpApp/Services/LocalizationService.cs` – EN/DE inkl. alle ToolTips, ConfirmDelete, About, Export
- `src/otpApp/Converters/CodeSplitterConverter.cs` – gruppiert Code von rechts in Dreierblöcke
- `src/otpApp/Converters/InvertBoolConverter.cs`
- `src/otpApp/Converters/StringNotEmptyConverter.cs`
- `src/otpApp/CompositionRoot.cs` – DI mit allen Services (auch `LocalizationService`, `OtpUriSerializer`, `PlatformService`)
