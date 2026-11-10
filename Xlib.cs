using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace X11PointerDemo;

/// <summary>Owns one XOpenDisplay result and closes it exactly once.</summary>
internal sealed class XDisplay : SafeHandleZeroOrMinusOneIsInvalid
{
    internal XDisplay(nint value) : base(ownsHandle: true) => SetHandle(value);
    protected override bool ReleaseHandle()
    {
        _ = Xlib.XCloseDisplay(handle);
        return true;
    }
}

/// <summary>
/// Minimal Linux Xlib bindings. Window is unsigned long (nuint on Linux);
/// Bool is int, and unsigned int is uint. These distinctions matter on x64.
/// </summary>
internal static class Xlib
{
    private const string Library = "libX11.so.6";

    internal static XDisplay OpenDisplay() => new(XOpenDisplay(0));

    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
    private static extern nint XOpenDisplay(nint displayName);

    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int XCloseDisplay(nint display);

    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int XDefaultScreen(XDisplay display);

    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
    internal static extern nuint XRootWindow(XDisplay display, int screen);

    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int XDisplayWidth(XDisplay display, int screen);

    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int XDisplayHeight(XDisplay display, int screen);

    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int XWarpPointer(XDisplay display, nuint sourceWindow,
        nuint destinationWindow, int sourceX, int sourceY, uint sourceWidth,
        uint sourceHeight, int destinationX, int destinationY);

    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int XFlush(XDisplay display);

    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int XQueryPointer(XDisplay display, nuint window,
        out nuint rootWindow, out nuint childWindow, out int rootX, out int rootY,
        out int windowX, out int windowY, out uint modifierMask);
}
