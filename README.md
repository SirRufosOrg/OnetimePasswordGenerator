# otpApp

TOTP & HOTP Authenticator – Desktop-App mit Avalonia UI, ReactiveUI und Zafiro.

## Features

- **TOTP**: Zeitbasierte Einmalpasswörter mit ProgressBar-Anzeige
- **HOTP**: Zählerbasierte Einmalpasswörter mit Next-Button
- **Inline-Edit**: Issuer, Label, Secret, Algorithmus, Digits (6/8), Period, Type
- **Import**: Zwischenablage (`otpauth://`) oder Datei (Batch-Insert)
- **Export**: Alle Accounts als `otpauth://`-URIs in Datei
- **Duplikaterkennung**: Beim Import und manuellen Hinzufügen
- **Secret-Normalisierung**: Uppercase, ohne Leerzeichen/Padding/Dashes
- **Löschbestätigung**: ConfirmDialog vor dem Entfernen
- **ToolTips**: An allen Eingabefeldern (Edit + Add Dialog)
- **Code-Trennung**: Anzeige in Dreierblöcken mit 10px Abstand
- **i18n**: Deutsch/Englisch (umschaltbar, erkennt Systemsprache)
- **macOS**: Systemeigenes Anwendungsmenü (About, Hide, Quit)
- **About lokalisiert**: App-Info in aktueller Sprache
- **Lokale Speicherung**: LiteDB
- **Copy to Clipboard**

## Tech Stack

- .NET 10 + Avalonia 12 + ReactiveUI + Zafiro + LiteDB

## Build & Run

```bash
dotnet run --project src/otpApp
```
