# otpApp

TOTP & HOTP Authenticator – Desktop-App mit Avalonia UI, ReactiveUI und Zafiro.

## Features

- **TOTP**: Zeitbasierte Einmalpasswörter mit ProgressBar-Anzeige
- **HOTP**: Zählerbasierte Einmalpasswörter mit Next-Button
- **Inline-Edit**: Issuer, Label, Secret, Algorithmus, Digits (6/8), Period, Type
- **i18n**: Deutsch/Englisch (umschaltbar, erkennt Systemsprache)
- **macOS**: Systemeigenes Anwendungsmenü (About, Hide, Quit)
- **Lokale Speicherung**: LiteDB
- **Copy to Clipboard**

## Tech Stack

- .NET 10 + Avalonia 12 + ReactiveUI + Zafiro + LiteDB

## Build & Run

```bash
dotnet run --project src/otpApp
```
