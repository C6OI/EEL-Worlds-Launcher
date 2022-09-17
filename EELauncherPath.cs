using System;
using System.IO;
using CmlLib.Core;

namespace EELauncher; 

public class EELauncherPath : MinecraftPath {
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
    
    protected new static string Dir(string path)
    {
        string p = NormalizePath(path);
        if (!Directory.Exists(p))
            Directory.CreateDirectory(p);

        return p;
    }
    
    public new static string GetOSDefaultPath() => MRule.OSName
        switch {
            "osx" => MacDefaultPath,
            "linux" => LinuxDefaultPath,
            "windows" => WindowsDefaultPath,
            _ => Environment.CurrentDirectory
        };
    
    public new static readonly string
        MacDefaultPath = $"{Environment.GetEnvironmentVariable("HOME")}/Library/Application Support/eelauncher",
        LinuxDefaultPath = $"{Environment.GetEnvironmentVariable("HOME")}/.eelauncher",
        WindowsDefaultPath = $"{Environment.GetEnvironmentVariable("appdata")}\\.eelauncher";
}
