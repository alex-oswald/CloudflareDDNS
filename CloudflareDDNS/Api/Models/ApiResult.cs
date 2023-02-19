using System.Text.Json.Serialization;

namespace CloudflareDDNS.Api.Models;

public class ApiResult<T> where T : class
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("result")]
    public T Result { get; set; } = null!;
}
