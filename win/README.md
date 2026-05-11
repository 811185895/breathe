# Breathe Widget

Windows ambient breathing reminder MVP.

## What It Does

- Creates a transparent, always-on-top WPF overlay.
- Uses Win32 extended window styles for click-through and no-activate behavior.
- Renders an adaptive breathing atmosphere at the screen golden-lower point by default.
- Samples the nearby background and shifts between moonlight, cool fog, and low-saturation tinted fog.
- Uses a stronger cool-fog tone on white backgrounds so the atmosphere remains visible.
- Provides a tray icon with `Visible`, `Subtle`, position, and `Exit` menu items.

## Run

```powershell
dotnet run --project BreatheWidget.App\BreatheWidget.App.csproj
```

Right-click the tray icon to switch visibility, move it to center/lower-third/golden point, or choose `Exit`.

## Test

```powershell
dotnet run --project BreatheWidget.Tests\BreatheWidget.Tests.csproj
dotnet build BreatheWidget.sln
```
