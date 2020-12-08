using System.Threading.Tasks;
using System;
using X11;
using System.Drawing;
using System.Runtime.InteropServices;

namespace test
{
    using static X11.Xlib;
    class Program
    {

        [DllImport("libX11.so.6")]
        static extern void XWarpPointer(IntPtr display,
            Window src_w,
            Window dest_w,
            int src_x, int src_y,
            int src_width, int src_height,
            int dest_x, int dest_y);
        static async Task Main(string[] args)
        {
            var hDisplay = XOpenDisplay(null);
            int screen = XDefaultScreen(hDisplay);
            Window root_window = XRootWindow(hDisplay, screen);

            XWarpPointer(hDisplay, Window.None, root_window, 0, 0, 0, 0, 1920 / 2, 1080 / 2);
            XFlush(hDisplay);

            while (true)
            {
                Console.SetCursorPosition(0, Console.GetCursorPosition().Top);
                Point p = await GetMousePosAsync(hDisplay, root_window);
                Console.Write($"{{X:{p.X:0000},Y:{p.Y:0000}}}");
            }
        }

        private static async Task<Point> GetMousePosAsync(IntPtr hDisplay, Window root_window)
        {
            return await Task.Run(() =>
            {
                Window window_returned = default;
                int rootX, rootY, winX, winY;
                rootX = rootY = winX = winY = 0;
                uint maskReturn = 0;

                var result = XQueryPointer(hDisplay,
                                            root_window,
                                            ref window_returned, ref window_returned,
                                            ref rootX, ref rootY,
                                            ref winX, ref winY,
                                            ref maskReturn
                                            );
                return new Point(rootX, rootY);
            });
        }
    }
}