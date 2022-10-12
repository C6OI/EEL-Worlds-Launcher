namespace EELauncher.Data; 

public struct OptionsData {
    public string? JavaPath { get; set; }
    public string? JavaVersion { get; set; }
    public string[] JVMArguments { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int Memory { get; set; }
    public bool FullScreen { get; set; }
}
