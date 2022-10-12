namespace EELauncher.Data;

public static class StaticData {
    public static OptionsData Options { get; set; } = new();
    public static ElybyAuthData Data { get; set; }
    public static string Password { get; set; } = null!;
}
