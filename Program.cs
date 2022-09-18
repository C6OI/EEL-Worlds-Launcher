using System;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using MessageBox.Avalonia;
using MessageBox.Avalonia.BaseWindows.Base;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;

namespace EELauncher {
    internal static class Program {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.

        static Mutex _mutex = new(false, "EELauncher");
        static bool _taken;
        
        [STAThread]
        public static void Main(string[] args) { 
            AppDomain.CurrentDomain.UnhandledException += ExceptionHandler;
            
            if (TakeMemory())
                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            else {
                ReleaseMemory();
                Environment.Exit(0);
            }
        }

        static void ExceptionHandler(object? sender, UnhandledExceptionEventArgs e) {
            MessageBoxStandardParams mBoxParams = new() {
                ContentTitle = "Fatal error",
                ContentMessage = "Лаунчер не может продолжить работу из-за фатальной ошибки.\n" +
                                 "Пожалуйста, отправьте эту ошибку администратору сервера:\n" +
                                 e.ExceptionObject,
                Icon = Icon.Error,
                ShowInCenter = true,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            
            if (MessageBoxManager.GetMessageBoxStandardWindow(mBoxParams).Show().IsCompleted) Environment.Exit(0);
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
