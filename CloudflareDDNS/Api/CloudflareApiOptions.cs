using System.ComponentModel.DataAnnotations;

namespace CloudflareDDNS.Api;

public class CloudflareApiOptions
{
    public static string Section = "CloudflareApi";

    [Required]
    public string ApiToken { get; set; } = null!;
}