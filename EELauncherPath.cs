using System;
using System.IO;
using CmlLib.Core;
using EELauncher.Extensions;
using Serilog;

namespace EELauncher;

public class EELauncherPath : MinecraftPath {
    static readonly ILogger Logger = Log.Logger.ForType<EELauncherPath>();
    
    public EELauncherPath() {
        BasePath = NormalizePath(GetOSDefaultPath());

        Mods = NormalizePath($"{BasePath}/mods");
        Emotes = NormalizePath($"{BasePath}/emotes");

        Library = NormalizePath($"{BasePath}/libraries");
        Versions = NormalizePath($"{BasePath}/versions");
        Resource = NormalizePath($"{BasePath}/resources");

        Runtime = NormalizePath($"{BasePath}/runtime");
        Assets = NormalizePath($"{BasePath}/assets");

        CreateDirs();
    }

    public string Mods { get; set; }
    public string Emotes { get; set; }

    public new void CreateDirs() {
        Dir(BasePath);
        Dir(Mods);
        Dir(Emotes);
        Dir(Library);
        Dir(Versions);
        Dir(Resource);
        Dir(Runtime);
        Dir(Assets);
    }

    new static string Dir(string path) {
        string p = NormalizePath(path);
        if (Directory.Exists(p)) return p;
        
        Logger.Information($"Creating directory {p}");
        Directory.CreateDirectory(p);

        return p;
    }

    public new static string GetOSDefaultPath() {
        return MRule.OSName
            switch {
                "osx" => MacDefaultPath,
                "linux" => LinuxDefaultPath,
                "windows" => WindowsDefaultPath,
                _ => Environment.CurrentDirectory
            };
    }

    public new static readonly string
        MacDefaultPath = $"{Environment.GetEnvironmentVariable("HOME")}/Library/Application Support/eelauncher",
        LinuxDefaultPath = $"{Environment.GetEnvironmentVariable("HOME")}/.eelauncher",
        WindowsDefaultPath = $"{Environment.GetEnvironmentVariable("appdata")}\\.eelauncher";
}
