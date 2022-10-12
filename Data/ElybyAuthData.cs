using System.Collections.Generic;

namespace EELauncher.Data;

public struct AvailableProfile {
    public string Id { get; set; }
    public string Name { get; set; }
}

public struct Property {
    public string Name { get; set; }
    public string Value { get; set; }
}

public struct ElybyAuthData {
    public string AccessToken { get; set; }
    public string ClientToken { get; set; }
    public List<AvailableProfile> AvailableProfiles { get; set; }
    public SelectedProfile SelectedProfile { get; set; }
    public User User { get; set; }
}

public struct SelectedProfile {
    public string Id { get; set; }
    public string Name { get; set; }

    public override string ToString() => $"ID: {Id}, Nickname: {Name}";
}

public struct User {
    public string Id { get; set; }
    public string Username { get; set; }
    public List<Property> Properties { get; set; }
}