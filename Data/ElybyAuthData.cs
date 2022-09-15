// ReSharper disable InconsistentNaming

using System.Collections.Generic;

namespace EELauncher.Data; 

public struct AvailableProfile {
    public string id { get; set; }
    public string name { get; set; }
}

public struct Property {
    public string name { get; set; }
    public string value { get; set; }
}

public struct ElybyAuthData {
    public string accessToken { get; set; }
    public string clientToken { get; set; }
    public List<AvailableProfile> availableProfiles { get; set; }
    public SelectedProfile selectedProfile { get; set; }
    public User user { get; set; }
}

public struct SelectedProfile {
    public string id { get; set; }
    public string name { get; set; }
}

public struct User {
    public string id { get; set; }
    public string username { get; set; }
    public List<Property> properties { get; set; }
}

public static class StaticData {
    public static ElybyAuthData Data { get; set; }
    public static string Password { get; set; } = null!;
}

