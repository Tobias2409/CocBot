using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CocBot;
public class HookManager
{
    private static IntPtr _hook = IntPtr.Zero;
    private static LowLevelKeyboardProc _proc = KeyboardProc;

    private static HookManager _hookInstance;

    public static HookManager HookInstance
    {
        get
        {
            if (_hookInstance == null)
                throw new InvalidOperationException("use createInstance()");
            return _hookInstance;
        }

    }

    public static HookManager createInstance(Action<uint> callback)
    {
        if (_hookInstance != null) return _hookInstance;
        _hookInstance = new HookManager();
        HookedFunc = callback;
        return _hookInstance;
    }

    private HookManager()
    {
        using (var process = Process.GetCurrentProcess())
        using (var module = process.MainModule)
        {
            _hook = SetWindowsHookEx(WH_KEYBOARD_LL, _proc,
                GetModuleHandle(module.ModuleName), 0);
        }
    }

    ~HookManager()
    {
        UnhookWindowsHookEx(_hook);
    }


    public static Action<uint> HookedFunc;
    private static IntPtr KeyboardProc(int nCode, IntPtr wParam, KBDLLHOOKSTRUCT lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
        {
            HookedFunc(lParam.vkCode);
        }

        return CallNextHookEx(_hook, nCode, wParam, lParam);
    }


    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, KBDLLHOOKSTRUCT lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, KBDLLHOOKSTRUCT lParam);
    [StructLayout(LayoutKind.Sequential)]
    private struct KBDLLHOOKSTRUCT
    {
        public uint vkCode;
        public uint scanCode;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }
}

