using CloudflareDDNS.Api.Models;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Json;

namespace CloudflareDDNS.Api;

public class CloudflareApi : ICloudflareApi
{
    public const string ENDPOINT = "https://api.cloudflare.com/client/v4";

    private readonly ILogger _logger;
    private readonly CloudflareApiOptions _options;
    private readonly HttpClient _client;

    public CloudflareApi(
        ILogger<CloudflareApi> logger,
        IOptions<CloudflareApiOptions> options,
        HttpClient client)
    {
        _logger = logger;
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _client = client;
    }

    private HttpRequestMessage BuildRequestMessage(UriBuilder uriBuilder, HttpMethod method, object? content = null) => new()
    {
        RequestUri = uriBuilder.Uri,
        Method = method,
        Headers =
        {
            { HttpRequestHeader.Authorization.ToString(), $"Bearer {_options.ApiToken}" },
            { HttpRequestHeader.ContentType.ToString(), "application/json" },
        },
        Content = content == null ? null : JsonContent.Create(content),
    };

    public async Task<List<ZoneDetails>> GetZonesAsync(CancellationToken cancellationToken)
    {
        var builder = new UriBuilder($"{ENDPOINT}/zones");
        _logger.LogDebug("Get zones uri={uri}", builder.Uri);

        var message = BuildRequestMessage(builder, HttpMethod.Get);

        var response = await _client.SendAsync(message, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PagedApiResult<ZoneDetails>>(cancellationToken: cancellationToken).ConfigureAwait(false);
        if (result == null)
        {
            throw new Exception("Invalid api result");
        }

        return result.Result;
    }

    public async Task<ZoneDetails> GetZoneDetailsAsync(string zoneId, CancellationToken cancellationToken)
    {
        var builder = new UriBuilder($"{ENDPOINT}/zones/{zoneId}");
        _logger.LogDebug("Get zone details uri={uri}", builder.Uri);

        var message = BuildRequestMessage(builder, HttpMethod.Get);

        var response = await _client.SendAsync(message, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ApiResult<ZoneDetails>>(cancellationToken: cancellationToken).ConfigureAwait(false);
        if (result == null)
        {
            throw new Exception("Invalid api result");
        }

        return result.Result;
    }

    public async Task<List<DNSRecord>> GetDnsRecordsAsync(string zoneId, CancellationToken cancellationToken)
    {
        var builder = new UriBuilder($"{ENDPOINT}/zones/{zoneId}/dns_records");
        _logger.LogDebug("Get DNS records uri={uri}", builder.Uri);

        var message = BuildRequestMessage(builder, HttpMethod.Get);

        var response = await _client.SendAsync(message, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PagedApiResult<DNSRecord>>(cancellationToken: cancellationToken).ConfigureAwait(false);
        if (result == null)
        {
            throw new Exception("Invalid api result");
        }

        return result.Result;
    }

    public async Task<DNSRecord> CreateDnsRecordAsync(string zoneId, string type, string name, string content, long ttl, bool proxied, CancellationToken cancellationToken)
    {
        var builder = new UriBuilder($"{ENDPOINT}/zones/{zoneId}/dns_records");
        _logger.LogDebug("Create DNS record uri={uri}", builder.Uri);

        var dns = new { type, name, content, ttl, proxied };

        var message = BuildRequestMessage(builder, HttpMethod.Post, dns);

        var response = await _client.SendAsync(message, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ApiResult<DNSRecord>>(cancellationToken: cancellationToken).ConfigureAwait(false);
        if (result == null || !result.Success)
        {
            throw new Exception("Invalid api result");
        }

        return result.Result;
    }

    public async Task<DNSRecord> UpdateDnsRecordAsync(string zoneId, string id, string type, string name, string content, long ttl, bool proxied, CancellationToken cancellationToken)
    {
        var builder = new UriBuilder($"{ENDPOINT}/zones/{zoneId}/dns_records/{id}");
        _logger.LogDebug("Update DNS record uri={uri}", builder.Uri);

        var dns = new { type, name, content, ttl, proxied };

        var message = BuildRequestMessage(builder, HttpMethod.Put, dns);

        var response = await _client.SendAsync(message, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ApiResult<DNSRecord>>(cancellationToken: cancellationToken).ConfigureAwait(false);
        if (result == null || !result.Success)
        {
            throw new Exception("Invalid api result");
        }

        return result.Result;
    }
}