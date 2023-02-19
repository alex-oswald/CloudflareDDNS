using System.Text.Json.Serialization;

namespace CloudflareDDNS.Api.Models;

public class PagedApiResult<T> where T : class
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("result")]
    public List<T> Result { get; set; } = null!;

    [JsonPropertyName("result_info")]
    public ApiResultPager? Pager { get; set; }
}