# Breathe Widget

Windows ambient breathing reminder MVP.

中文使用说明、**免安装下载包（GitHub Releases）**、以及从源码运行所需的 **.NET 8 SDK**，见仓库根目录 [`README.md`](../README.md)。

## Binary releases

End users can download a **self-contained portable zip** (no separate .NET install) from the repository’s [**Releases**](https://github.com/811185895/breathe/releases/latest) page. Extract the archive and run **`BreatheWidget.App.exe`**.

## What It Does

- Creates a transparent, always-on-top WPF overlay.
- Uses Win32 extended window styles for click-through and no-activate behavior.
- Renders an adaptive breathing atmosphere at the screen golden-lower point by default.
- Samples the nearby background and shifts between dawn glow and low-saturation tinted fog by default.
- Uses the same warm dawn-glow tone on very dark and very bright backgrounds so the atmosphere feels alive and remains visible.
- Keeps optional tray tone modes for `Classic Neutral` and `Moonlight`.
- Provides a tray icon with `Visible`, `Subtle`, tone, position, and `Exit` menu items.

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
