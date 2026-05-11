# Gentle Focus Awareness V2 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the V2 awareness loop that observes foreground-window dwell and input activity, maps it into work states, and briefly fades in gentle text prompts during sustained deep focus.

**Architecture:** Keep desktop API sampling in the WPF app and keep behavior rules in `BreatheWidget.Core` so they can be tested without real hooks. `MainWindow` renders state by reusing `WorkStateMapper`, while `GentlePromptPolicy` controls prompt timing and copy.

**Tech Stack:** C#/.NET 8, WPF, Win32 `user32.dll` interop, existing console-based test project.

---

### Task 1: Core Awareness Rules

**Files:**
- Create: `win/BreatheWidget.Core/ActivitySnapshot.cs`
- Create: `win/BreatheWidget.Core/WindowActivitySnapshot.cs`
- Create: `win/BreatheWidget.Core/WorkStateEvaluator.cs`
- Create: `win/BreatheWidget.Core/GentlePromptPolicy.cs`
- Modify: `win/BreatheWidget.Tests/Program.cs`

- [x] Add failing tests for `WorkStateEvaluator` light, focused, and deep-focus threshold behavior.
- [x] Run `dotnet run --project BreatheWidget.Tests\BreatheWidget.Tests.csproj` and confirm those tests fail because the new types do not exist.
- [x] Implement the immutable snapshot records and evaluator with conservative thresholds from the spec.
- [x] Add failing tests for `GentlePromptPolicy` persistence threshold and cooldown behavior.
- [x] Implement prompt decisions and the prompt copy pool.
- [x] Run `dotnet run --project BreatheWidget.Tests\BreatheWidget.Tests.csproj` and confirm all tests pass.

### Task 2: Windows Activity Monitor

**Files:**
- Create: `win/BreatheWidget.App/ActivityMonitor.cs`
- Modify: `win/BreatheWidget.App/MainWindow.xaml.cs`

- [x] Implement foreground-window sampling through `GetForegroundWindow`, `GetWindowThreadProcessId`, `GetWindowText`, `GetLastInputInfo`, and `GetCursorPos`.
- [x] Track dwell duration, switches, input bursts, mouse movement, and inactivity in aggregate only.
- [x] Expose a `Sample(DateTimeOffset now)` method returning `ActivitySnapshot`.
- [x] Keep API failures non-fatal by returning a light, inactive snapshot.

### Task 3: WPF Rendering Integration

**Files:**
- Modify: `win/BreatheWidget.App/MainWindow.xaml`
- Modify: `win/BreatheWidget.App/MainWindow.xaml.cs`

- [x] Add a prompt `TextBlock` to the transparent canvas.
- [x] Add a separate awareness timer that samples activity every few seconds.
- [x] Use `WorkStateEvaluator` to update `BreathSampler` through `WorkStateMapper`.
- [x] Use `GentlePromptPolicy` to fade prompt text in, hold briefly, then fade out.
- [x] Preserve click-through, no-activate, always-on-top behavior.

### Task 4: Verification

**Files:**
- Modify as needed only if verification exposes a defect.

- [x] Run `dotnet run --project BreatheWidget.Tests\BreatheWidget.Tests.csproj`.
- [x] Run `dotnet build BreatheWidget.sln`.
- [x] Review `git diff` for unrelated changes and ensure `.cursor/` remains untouched.
