# Auto Deleter

Löscht Ordnerinhalte automatisch nach konfigurierbaren Intervallen.
Läuft auf Windows Server 2016+ (und Windows 10+).

## Voraussetzungen

- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) (nur zum Bauen)
- Windows Server 2016 / Windows 10 oder neuer
- Administrator-Rechte (für Service-Installation)

## Bauen

```powershell
# Einzelne .exe (self-contained, kein .NET auf dem Zielrechner nötig)
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

Die fertige `AutoDeleter.exe` liegt dann unter:
`bin\Release\net6.0-windows\win-x64\publish\AutoDeleter.exe`

## Verwendung

1. `AutoDeleter.exe` als **Administrator** starten (UAC-Dialog erscheint automatisch).
2. Mit **„＋ Ordner hinzufügen"** einen oder mehrere Ordner eintragen.
3. Für jeden Ordner das gewünschte Lösch-Intervall wählen (Stündlich / Täglich / Wöchentlich).
4. Optional: **„Als Windows-Dienst bei Systemstart ausführen"** aktivieren – der Dienst läuft dann im Hintergrund auch ohne offenes Fenster.
5. **„Alle Ordner sofort leeren"** löscht alle konfigurierten Ordner unmittelbar.

## Hinweise

- Dateien, die gerade von einem anderen Prozess geöffnet sind, werden **übersprungen** (keine Fehler).
- Die Konfiguration wird unter `C:\ProgramData\AutoDeleter\config.json` gespeichert.
- Wenn der Windows-Dienst aktiv ist, liest er die Konfiguration jede Minute neu ein – Änderungen über das UI werden sofort übernommen, ohne den Dienst neu zu starten.
- Der Dienst kann auch manuell über die Windows-Dienstverwaltung (`services.msc`) oder `sc stop/start AutoDeleter` gesteuert werden.
