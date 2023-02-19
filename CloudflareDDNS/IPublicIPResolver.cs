using System.Net;

namespace CloudflareDDNS;

internal interface IPublicIPResolver
{
    Task<IPAddress?> ResolveIPv4Async(CancellationToken cancellationToken);
}