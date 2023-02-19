using System.ComponentModel.DataAnnotations;

namespace CloudflareDDNS;

public class DDNSOptions
{
    public static string Section = "DDNS";

    [Required]
    public int UpdateIntervalSeconds { get; set; } = 900;

    [Required]
    public string ZoneName { get; set; } = null!;

    [Required]
    public string DnsRecordName { get; set; } = null!;
}