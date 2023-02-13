using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AutoCrafter.Manager;
class NativeManager
{
    [DllImport("user32.dll")]
    static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter, string lpszClass, string lpszWindow);

    [DllImport("user32.dll")]
    static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    public static extern IntPtr GetForegroundWindow();

    public static bool TryFindGameWindow(out IntPtr hwnd)
    {
        hwnd = IntPtr.Zero;
        while (true)
        {
            hwnd = FindWindowEx(IntPtr.Zero, hwnd, "FFXIVGAME", null);
            if (hwnd == IntPtr.Zero) break;
            GetWindowThreadProcessId(hwnd, out var pid);
            if (pid == Process.GetCurrentProcess().Id) break;
        }
        return hwnd != IntPtr.Zero;
    }


    public class Keypress
    {
        public const int LControlKey = 162;
        public const int Space = 32;

        public const uint WM_KEYUP = 0x101;
        public const uint WM_KEYDOWN = 0x100;

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        public static void SendKeycode(IntPtr hwnd, int keycode)
        {
            SendMessage(hwnd, WM_KEYDOWN, (IntPtr)keycode, (IntPtr)0);
            SendMessage(hwnd, WM_KEYUP, (IntPtr)keycode, (IntPtr)0);
        }
    }
}