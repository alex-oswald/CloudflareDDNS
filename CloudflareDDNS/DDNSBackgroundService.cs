using CloudflareDDNS.Api;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace CloudflareDDNS;

internal class DDNSBackgroundService : BackgroundService
{
    private readonly ILogger<DDNSBackgroundService> _logger;
    private readonly DDNSOptions _options;
    private readonly IPublicIPResolver _resolver;
    private readonly ICloudflareApi _cloudflareApi;

    public DDNSBackgroundService(
        ILogger<DDNSBackgroundService> logger,
        IOptions<DDNSOptions> options,
        IPublicIPResolver publicIPResolver,
        ICloudflareApi cloudflareApi)
    {
        _logger = logger;
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _resolver = publicIPResolver;
        _cloudflareApi = cloudflareApi;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("{name} service started", nameof(DDNSBackgroundService));

        // Startup delay, wait for everything to get started before looping
        await Task.Delay(1000, stoppingToken).ConfigureAwait(false);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var ip = await _resolver.ResolveIPv4Async(stoppingToken).ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(ip?.ToString()))
                {
                    throw new Exception("Bad ip address");
                }
                _logger.LogInformation("Public IP: {ip}", ip!.ToString());

                var zones = await _cloudflareApi.GetZonesAsync(stoppingToken).ConfigureAwait(false);
                var zone = zones.Where(o => o.Name == _options.ZoneName).FirstOrDefault();
                if (zone == null)
                {
                    throw new Exception("Could not find zone");
                }

                var zoneDetails = await _cloudflareApi.GetZoneDetailsAsync(zone.Id, stoppingToken).ConfigureAwait(false);
                _logger.LogInformation("Zone info: id={id}, name={name}", zoneDetails.Id, zoneDetails.Name);

                var dnsRecords = await _cloudflareApi.GetDnsRecordsAsync(zoneDetails.Id, stoppingToken).ConfigureAwait(false);
                var record = dnsRecords.Where(o => o.Name == _options.DnsRecordName).FirstOrDefault();
                if (record is null)
                {
                    _logger.LogInformation("DNS record {record} for Zone {zone} does not exist ❌, creating it...", _options.DnsRecordName, zoneDetails.Name);
                    // Create DNS record
                    var newRecord = await _cloudflareApi.CreateDnsRecordAsync(
                        zoneDetails.Id, "A", _options.DnsRecordName, ip.ToString(), 1, true, stoppingToken).ConfigureAwait(false);
                    _logger.LogInformation("DNS record created: id={id}, name={name}, type={type}, content={content}, ttl={ttl}, proxied={proxied}",
                        newRecord.Id, newRecord.Name, newRecord.Type, newRecord.Content, newRecord.Ttl, newRecord.Proxied);
                }
                else
                {
                    _logger.LogDebug("Found DNS record {name}", _options.DnsRecordName);
                    if (record.Content == ip!.ToString())
                    {
                        _logger.LogDebug("Your public IP matches the DNS record content. Nothing to update here. 👍");
                    }
                    else
                    {
                        _logger.LogWarning("It seems that your public IP address has changed. Updating the DNS record.");
                        // Update DNS record
                        var updatedRecord = await _cloudflareApi.UpdateDnsRecordAsync(
                            zoneDetails.Id, record.Id, record.Type, record.Name, ip.ToString(), record.Ttl, record.Proxied, stoppingToken).ConfigureAwait(false);
                        _logger.LogInformation("DNS record updated: id={id}, name={name}, type={type}, content={content}, ttl={ttl}, proxied={proxied}",
                            updatedRecord.Id, updatedRecord.Name, updatedRecord.Type, updatedRecord.Content, updatedRecord.Ttl, updatedRecord.Proxied);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "DDNS update failed 🤬");
            }

            _logger.LogInformation("Waiting {s} seconds till next update... 😴", _options.UpdateIntervalSeconds);
            await Task.Delay(_options.UpdateIntervalSeconds * 1000, stoppingToken).ConfigureAwait(false);
        }
    }
}