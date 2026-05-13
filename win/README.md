# Breathe Widget

Windows ambient breathing reminder MVP.

中文使用说明、**免安装下载包（GitHub Releases）**、以及从源码运行所需的 **.NET 8 SDK**，见仓库根目录 [`README.md`](../README.md)。

**Motivation (short):** a calm, peripheral cue against “time tunnel” during deep work—not a pomodoro popup. **Behavior:** adaptive breathing overlay; local-only activity heuristics (idle/keyboard/mouse/window dwell); optional gentle on-screen text after prolonged deep focus. Product notes (Chinese): [`docs/设计方案.md`](docs/设计方案.md), [`docs/设计方案-补充.md`](docs/设计方案-补充.md).

## Binary releases

End users can download a **self-contained portable zip** (no separate .NET install) from the repository’s [**Releases**](https://github.com/811185895/breathe/releases/latest) page. Extract the archive and run **`BreatheWidget.App.exe`**.

## Demo

Screenshots and a short screen recording: see the root [`README.md`](../README.md).

## What It Does

- Creates a transparent, always-on-top WPF overlay.
- Uses Win32 extended window styles for click-through and no-activate behavior.
- Renders an adaptive breathing atmosphere at the screen golden-lower point by default.
- Samples the nearby background and shifts between dawn glow and low-saturation tinted fog by default.
- Uses the same warm dawn-glow tone on very dark and very bright backgrounds so the atmosphere feels alive and remains visible.
- Keeps optional tray tone modes for `Classic Neutral` and `Moonlight`.
- Provides a tray icon with `Visible`, `Subtle`, tone, position, and `Exit` menu items.
- Uses **local-only** activity heuristics (idle time, keyboard rate, mouse travel, foreground window dwell) to infer light vs deep focus and adjust the breath profile.
- After prolonged deep focus, may show **brief, low-frequency gentle text** over the overlay (rotating lines, per-window cooldown)—still click-through and non-modal.

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
