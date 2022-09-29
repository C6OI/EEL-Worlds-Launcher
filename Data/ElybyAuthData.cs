using System.Collections.Generic;
using Newtonsoft.Json;

namespace EELauncher.Data;

public struct AvailableProfile {
    [JsonProperty("id")] public string Id { get; set; }
    [JsonProperty("name")] public string Name { get; set; }
}

public struct Property {
    [JsonProperty("name")] public string Name { get; set; }
    [JsonProperty("value")] public string Value { get; set; }
}

public struct ElybyAuthData {
    [JsonProperty("accessToken")] public string AccessToken { get; set; }
    [JsonProperty("clientToken")] public string ClientToken { get; set; }
    [JsonProperty("availableProfiles")] public List<AvailableProfile> AvailableProfiles { get; set; }
    [JsonProperty("selectedProfile")] public SelectedProfile SelectedProfile { get; set; }
    [JsonProperty("user")] public User User { get; set; }
}

public struct SelectedProfile {
    [JsonProperty("id")] public string Id { get; set; }
    [JsonProperty("name")] public string Name { get; set; }

    public override string ToString() => $"ID: {Id}, Nickname: {Name}";
}

public struct User {
    [JsonProperty("id")] public string Id { get; set; }
    [JsonProperty("username")] public string Username { get; set; }
    [JsonProperty("properties")] public List<Property> Properties { get; set; }
}

public static class StaticData {
    public static ElybyAuthData Data { get; set; }
    public static string Password { get; set; } = null!;
}
