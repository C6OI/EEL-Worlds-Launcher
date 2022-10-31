using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using Avalonia.Controls;
using EELauncher.Extensions;
using MessageBox.Avalonia;
using MessageBox.Avalonia.BaseWindows.Base;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;

namespace EELauncher; 

public static class Updater {
    static readonly Version? CurrentVersion = Assembly.GetExecutingAssembly().GetName().Version;
    static readonly string ExePath = AppDomain.CurrentDomain.BaseDirectory;

    public static async void CheckForUpdates(Version versionOnServer, Window window) {
        if (CurrentVersion == versionOnServer) return;

        IMsBoxWindow<ButtonResult> updateQuestion = MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams {
            Icon = Icon.Question,
            WindowIcon = window.Icon,
            ButtonDefinitions = ButtonEnum.YesNo,
            ContentTitle = "Обновление",
            ContentMessage = $"Доступна новая версия: {versionOnServer}.\nОбновить?",
        });

        ButtonResult result = await updateQuestion.ShowDialog(window);

        if (result != ButtonResult.Yes) return;

        string pathToUpdate = Path.Combine(Path.GetTempPath(), "Update.exe");
            
        await UrlExtensions.DownloadFile(new Uri("https://mods.eelworlds.ml/installers/EELauncher.exe"), pathToUpdate);

        Process updateProcess = new() {
            StartInfo = new ProcessStartInfo {
                FileName = pathToUpdate,
                Arguments = $"/DIR=\"{ExePath}\" /VERYSILENT /NORESTART"
            }
        };
        
        updateProcess.Start();

        Environment.Exit(0);
    }
    
    public static bool CanConnectToServer() {
        try {
            Dns.GetHostEntry("mods.eelworlds.ml");
            return true;
        } catch { return false; }
    }
}
