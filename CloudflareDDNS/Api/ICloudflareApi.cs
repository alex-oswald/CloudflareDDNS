using CloudflareDDNS.Api.Models;

namespace CloudflareDDNS.Api;

public interface ICloudflareApi
{
    Task<List<ZoneDetails>> GetZonesAsync(CancellationToken cancellationToken);
    Task<ZoneDetails> GetZoneDetailsAsync(string zoneId, CancellationToken cancellationToken);
    Task<List<DNSRecord>> GetDnsRecordsAsync(string zoneId, CancellationToken cancellationToken);
    Task<DNSRecord> CreateDnsRecordAsync(string zoneId, string type, string name, string content, long ttl, bool proxied, CancellationToken cancellationToken);
    Task<DNSRecord> UpdateDnsRecordAsync(string zoneId, string id, string type, string name, string content, long ttl, bool proxied, CancellationToken cancellationToken);
}