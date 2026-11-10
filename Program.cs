using System.Globalization;

namespace X11PointerDemo;

internal static class Program
{
    private const string Usage =
        "Usage: dotnet run -- [--once] [--center]\n" +
        "Observe the X11 pointer at 10 Hz; press Ctrl+C to stop.\n" +
        "--once    Print one position and exit.\n" +
        "--center  Move to the default X screen's center before observing.";

    private static int Main(string[] args)
    {
        if (args.Length == 1 && args[0] is "--help" or "-h")
        {
            Console.WriteLine(Usage);
            return 0;
        }

        bool once = false;
        bool center = false;
        foreach (string argument in args)
        {
            switch (argument)
            {
                case "--once" when !once: once = true; break;
                case "--center" when !center: center = true; break;
                default:
                    Console.Error.WriteLine(Usage);
                    return 2;
            }
        }

        if (!OperatingSystem.IsLinux())
        {
            Console.Error.WriteLine("This example requires Linux and an accessible X11 display.");
            return 1;
        }

        try
        {
            using var display = Xlib.OpenDisplay();
            if (display.IsInvalid)
                throw new InvalidOperationException(
                    "Cannot open the X display. Check DISPLAY and your session's X11 access.");

            int screen = Xlib.XDefaultScreen(display);
            nuint root = Xlib.XRootWindow(display, screen);
            if (center)
            {
                int x = Xlib.XDisplayWidth(display, screen) / 2;
                int y = Xlib.XDisplayHeight(display, screen) / 2;
                _ = Xlib.XWarpPointer(display, 0, root, 0, 0, 0, 0, x, y);
                _ = Xlib.XFlush(display);
            }

            using var stopped = new ManualResetEventSlim();
            ConsoleCancelEventHandler cancel = (_, eventArgs) =>
            {
                eventArgs.Cancel = true;
                stopped.Set();
            };
            Console.CancelKeyPress += cancel;
            try
            {
                do
                {
                    // All Xlib calls stay on this thread; no XInitThreads is needed.
                    int sameScreen = Xlib.XQueryPointer(display, root, out _, out _,
                        out int rootX, out int rootY, out _, out _, out _);
                    if (sameScreen == 0)
                    {
                        Console.Error.WriteLine("The pointer is on another X screen.");
                        if (once) return 1;
                    }
                    else
                    {
                        Console.WriteLine(string.Create(CultureInfo.InvariantCulture,
                            $"X: {rootX}, Y: {rootY}"));
                    }
                } while (!once && !stopped.Wait(100));
            }
            finally
            {
                Console.CancelKeyPress -= cancel;
            }
            return 0;
        }
        catch (Exception error) when (error is DllNotFoundException
                                      or EntryPointNotFoundException
                                      or InvalidOperationException)
        {
            Console.Error.WriteLine(error.Message);
            return 1;
        }
    }
}
