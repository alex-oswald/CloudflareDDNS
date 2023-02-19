using System.Text.Json.Serialization;

namespace CloudflareDDNS.Api.Models;

public class ZoneDetails
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
}