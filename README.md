# X11 Pointer Interop for .NET

A small C# example of calling **XQueryPointer** and **XWarpPointer** directly from Linux through P/Invoke. Read the pointer's root-window coordinates, optionally move it to the center of the default X screen, and see how native display ownership is handled.

Originally created by [Daniel Baumert](https://github.com/facebamm) and [Rick Armbruster](https://github.com/Rick-laboratory).

## Requirements

- Linux with an accessible **X11** session and `libX11.so.6`.
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).
- A valid `DISPLAY` and permission to access that display.

There are no NuGet dependencies. This is an Xlib example, not a native Wayland API; behavior under XWayland depends on the compositor and session restrictions.

## Run

```sh
dotnet build -c Release
dotnet run -c Release -- --once
dotnet run -c Release -- --center --once
dotnet run -c Release
```

| Option | Behavior |
| --- | --- |
| No options | Print coordinates every 100 ms until Ctrl+C |
| `--once` | Query once and exit |
| `--center` | Move the pointer to the default X screen's center first |
| `--help` | Print usage without opening a display |

Movement is explicit: use `--center` to demonstrate `XWarpPointer`. The screen dimensions come from Xlib rather than a hard-coded 1920 × 1080 assumption. On a multi-monitor X screen, the combined screen center may fall between monitors.

## Code guide

- [Program.cs](Program.cs) — arguments, platform check, polling and orderly Ctrl+C shutdown.
- [Xlib.cs](Xlib.cs) — the small native API surface and an owning `SafeHandle`.
- [X11PointerDemo.csproj](X11PointerDemo.csproj) — a complete SDK project.

All Xlib calls run on one thread. A synchronous query does not need a new thread-pool task on every iteration. The native `Window` type is an unsigned C long, represented by `nuint` on Linux; Xlib's `Bool` remains a 32-bit `int`. Root and child outputs use separate storage.

A failed `XOpenDisplay` produces an actionable error. A pointer on another X screen is reported rather than being mistaken for coordinates `0, 0`. The display is closed on normal exit and managed exceptions.

## Validation and limits

The project builds with warnings treated as errors. Help, argument handling and the non-Linux guard can be checked without a display. Actual querying and movement require a Linux X11 session; those runtime paths were not exercised on the Windows maintenance machine.

See the upstream [Xlib manual](https://www.x.org/releases/current/doc/libX11/libX11/libX11.html) for X11 semantics.

## License

[MIT](LICENSE). Original contributor attribution is retained.
