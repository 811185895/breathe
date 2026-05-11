using BreatheWidget.Core;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace BreatheWidget.App;

internal sealed class ActivityMonitor : IDisposable
{
    private static readonly TimeSpan ActivityWindow = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan InactiveBreak = TimeSpan.FromMinutes(3);

    private readonly object _keyboardLock = new();
    private readonly Queue<DateTimeOffset> _keyboardInputs = new();
    private readonly Queue<DateTimeOffset> _windowSwitches = new();
    private readonly Queue<MouseTravelSample> _mouseTravel = new();
    private readonly LowLevelKeyboardProc _keyboardProc;
    private IntPtr _keyboardHook;
    private WindowActivitySnapshot? _currentWindow;
    private DateTimeOffset? _windowStartedAt;
    private Point? _lastCursor;
    private bool _disposed;

    public ActivityMonitor()
    {
        _keyboardProc = HandleKeyboard;
        _keyboardHook = SetWindowsHookEx(WhKeyboardLl, _keyboardProc, GetCurrentModuleHandle(), 0);
    }

    public ActivitySnapshot Sample(DateTimeOffset now)
    {
        if (_disposed)
        {
            return CreateUnavailableSnapshot();
        }

        try
        {
            var idleDuration = ReadIdleDuration();
            if (idleDuration >= InactiveBreak)
            {
                ResetSession();
                return new ActivitySnapshot(
                    Window: WindowActivitySnapshot.Unknown,
                    WindowDwell: TimeSpan.Zero,
                    KeyboardInputs: 0,
                    MouseTravel: 0,
                    WindowSwitches: 0,
                    IdleDuration: idleDuration,
                    SampleDuration: TimeSpan.Zero,
                    IsAvailable: true);
            }

            var window = ReadForegroundWindow();
            if (_currentWindow is null || _currentWindow.Identity != window.Identity)
            {
                _currentWindow = window;
                _windowStartedAt = now;
                _lastCursor = null;
                _windowSwitches.Enqueue(now);
            }

            TrackMouseTravel(now);
            Prune(now);

            var dwell = now - (_windowStartedAt ?? now);
            var sampleDuration = TimeSpan.FromTicks(Math.Min(dwell.Ticks, ActivityWindow.Ticks));
            int keyboardInputs;
            lock (_keyboardLock)
            {
                keyboardInputs = _keyboardInputs.Count;
            }

            return new ActivitySnapshot(
                Window: _currentWindow,
                WindowDwell: dwell,
                KeyboardInputs: keyboardInputs,
                MouseTravel: _mouseTravel.Sum(sample => sample.Distance),
                WindowSwitches: Math.Max(0, _windowSwitches.Count - 1),
                IdleDuration: idleDuration,
                SampleDuration: sampleDuration,
                IsAvailable: true);
        }
        catch
        {
            return CreateUnavailableSnapshot();
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_keyboardHook != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_keyboardHook);
            _keyboardHook = IntPtr.Zero;
        }

        _disposed = true;
    }

    private IntPtr HandleKeyboard(int code, IntPtr wParam, IntPtr lParam)
    {
        if (code >= 0 && (wParam == WmKeydown || wParam == WmSyskeydown))
        {
            lock (_keyboardLock)
            {
                _keyboardInputs.Enqueue(DateTimeOffset.Now);
            }
        }

        return CallNextHookEx(_keyboardHook, code, wParam, lParam);
    }

    private void TrackMouseTravel(DateTimeOffset now)
    {
        if (!GetCursorPos(out var cursor))
        {
            return;
        }

        if (_lastCursor is not null)
        {
            var deltaX = cursor.X - _lastCursor.Value.X;
            var deltaY = cursor.Y - _lastCursor.Value.Y;
            var distance = Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY));
            if (distance > 0)
            {
                _mouseTravel.Enqueue(new MouseTravelSample(now, distance));
            }
        }

        _lastCursor = cursor;
    }

    private void Prune(DateTimeOffset now)
    {
        var oldest = now - ActivityWindow;

        lock (_keyboardLock)
        {
            while (_keyboardInputs.TryPeek(out var keyboardAt) && keyboardAt < oldest)
            {
                _keyboardInputs.Dequeue();
            }
        }

        while (_windowSwitches.TryPeek(out var switchAt) && switchAt < oldest)
        {
            _windowSwitches.Dequeue();
        }

        while (_mouseTravel.TryPeek(out var mouseAt) && mouseAt.At < oldest)
        {
            _mouseTravel.Dequeue();
        }
    }

    private void ResetSession()
    {
        _currentWindow = null;
        _windowStartedAt = null;
        _lastCursor = null;
        _windowSwitches.Clear();
        _mouseTravel.Clear();

        lock (_keyboardLock)
        {
            _keyboardInputs.Clear();
        }
    }

    private static ActivitySnapshot CreateUnavailableSnapshot()
    {
        return new ActivitySnapshot(
            Window: WindowActivitySnapshot.Unknown,
            WindowDwell: TimeSpan.Zero,
            KeyboardInputs: 0,
            MouseTravel: 0,
            WindowSwitches: 0,
            IdleDuration: TimeSpan.Zero,
            SampleDuration: TimeSpan.Zero,
            IsAvailable: false);
    }

    private static WindowActivitySnapshot ReadForegroundWindow()
    {
        var handle = GetForegroundWindow();
        if (handle == IntPtr.Zero)
        {
            return WindowActivitySnapshot.Unknown;
        }

        _ = GetWindowThreadProcessId(handle, out var processId);
        var title = ReadWindowTitle(handle);
        var processName = "unknown";

        try
        {
            using var process = Process.GetProcessById((int)processId);
            processName = process.ProcessName;
        }
        catch
        {
            processName = "unknown";
        }

        return new WindowActivitySnapshot(processName, title, (int)processId);
    }

    private static string ReadWindowTitle(IntPtr handle)
    {
        var builder = new StringBuilder(256);
        var length = GetWindowText(handle, builder, builder.Capacity);
        return length > 0 ? builder.ToString() : string.Empty;
    }

    private static TimeSpan ReadIdleDuration()
    {
        var info = new LastInputInfo
        {
            Size = (uint)Marshal.SizeOf<LastInputInfo>()
        };

        if (!GetLastInputInfo(ref info))
        {
            return TimeSpan.Zero;
        }

        var idleMilliseconds = unchecked((uint)Environment.TickCount - info.Time);
        return TimeSpan.FromMilliseconds(idleMilliseconds);
    }

    private static IntPtr GetCurrentModuleHandle()
    {
        using var process = Process.GetCurrentProcess();
        var moduleName = process.MainModule?.ModuleName;
        return string.IsNullOrWhiteSpace(moduleName) ? IntPtr.Zero : GetModuleHandle(moduleName);
    }

    private sealed record MouseTravelSample(DateTimeOffset At, double Distance);

    private const int WhKeyboardLl = 13;
    private static readonly IntPtr WmKeydown = 0x0100;
    private static readonly IntPtr WmSyskeydown = 0x0104;

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    private struct Point
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct LastInputInfo
    {
        public uint Size;
        public uint Time;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out Point point);

    [DllImport("user32.dll")]
    private static extern bool GetLastInputInfo(ref LastInputInfo info);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll")]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
}
