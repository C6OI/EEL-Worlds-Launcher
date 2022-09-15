using System;
using System.Threading;
using Avalonia;
using Avalonia.ReactiveUI;

namespace EELauncher {
    internal static class Program {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.

        static Mutex _mutex = new(false, "EELauncher");
        static bool _taken;
        
        [STAThread]
        public static void Main(string[] args) {
            if (TakeMemory())
                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            else {
                ReleaseMemory();
                Environment.Exit(0);
            }
        }

        static bool TakeMemory() => _taken = _mutex.WaitOne(0, true);

        public static void ReleaseMemory() {
            if (_taken) try { _mutex.ReleaseMutex(); } catch {/**/}
        }
        
        // Avalonia configuration, don't remove; also used by visual designer.
        static AppBuilder BuildAvaloniaApp() =>
            AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUI();
    }
}
