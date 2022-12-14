using System;
using System.Threading;
using Avalonia;
using Avalonia.ReactiveUI;
using EELauncher.Extensions;
using EELauncher.Services;
using Serilog;

namespace EELauncher; 

internal static class Program {
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.

    static readonly ILogger Logger = Log.Logger.ForType(typeof(Program));
    static readonly Mutex Mutex = new(false, "EELauncher");
    static bool _taken;
        
    [STAThread]
    public static void Main(string[] args) {
        ServiceManager.Instance.Init();
        ServiceManager.Instance.Container.Resolve<ILoggerService>().Init();

        AppDomain.CurrentDomain.UnhandledException += ExceptionHandler;
        AppDomain.CurrentDomain.ProcessExit += ProcessExitHandler;
        
        if (TakeMemory()) {
            Logger.Information("Starting launcher...");
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        } else {
            Logger.Error("Another instance of EELauncher already running!");
            ServiceManager.Instance.Container.Dispose();
            ServiceManager.Instance.Dispose();
            ReleaseMemory();
            Environment.Exit(0);
        }
        
        ServiceManager.Instance.Container.Dispose();
        ServiceManager.Instance.Dispose();
    }

    static void ExceptionHandler(object? s, UnhandledExceptionEventArgs e) =>
        Logger.Fatal(e.IsTerminating
            ? $"Launcher crashed with exception: {e.ExceptionObject}\n{new string('-', 100)}"
            : $"An exception occured: {e.ExceptionObject}");

    static void ProcessExitHandler(object? s, EventArgs e) =>
        Logger.Information($"Launcher process exited\n{new string('-', 100)}");

    static bool TakeMemory() => _taken = Mutex.WaitOne(0, true);

    public static void ReleaseMemory() {
        Logger.Information("Releasing memory...");
        
        if (_taken) {
            try {
                Mutex.ReleaseMutex();
                Logger.Information("Releasing memory is successful");
            } catch (Exception e) {
                Logger.Error($"Releasing memory isn't successful.\nException: {e}");
            }
            
            return;
        }
            
        Logger.Error("Releasing memory isn't successful because memory isn't taken");
    }
        
    // Avalonia configuration, don't remove; also used by visual designer.
    static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI();
}