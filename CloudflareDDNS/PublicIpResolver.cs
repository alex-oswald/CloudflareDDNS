using DnsClient;
using System.Net;
using System.Net.Sockets;

namespace CloudflareDDNS;

internal class PublicIPResolver : IPublicIPResolver
{
    private readonly ILogger<PublicIPResolver> _logger;
    private readonly ILookupClient _client;

    public PublicIPResolver(
        ILogger<PublicIPResolver> logger,
        ILookupClient client)
    {
        _logger = logger;
        _client = client;
    }

    public async Task<IPAddress?> ResolveIPv4Async(CancellationToken cancellationToken)
    {
        var query = "whoami.cloudflare";
        var ipv4NameServers = new List<IPAddress>
        {
            IPAddress.Parse("1.1.1.1"),
            IPAddress.Parse("1.0.0.1"),
        };
        var ipv4ServerOptions = new DnsQueryAndServerOptions(ipv4NameServers.ToArray())
        {
            UseCache = false,
        };

        var ip = await QueryAsync(query, ipv4ServerOptions, cancellationToken).ConfigureAwait(false);

        if (ip!.AddressFamily == AddressFamily.InterNetwork)
        {
            return ip;
        }

        return null;
    }

    private async Task<IPAddress> QueryAsync(string query, DnsQueryAndServerOptions options, CancellationToken cancellationToken)
    {
        var question = new DnsQuestion(query, QueryType.TXT, QueryClass.CH);
        var response = await _client.QueryAsync(question, options, cancellationToken).ConfigureAwait(false);
        var ip = response.Answers.TxtRecords().FirstOrDefault()?.Text.FirstOrDefault();

        return IPAddress.Parse(ip!);
    }
}