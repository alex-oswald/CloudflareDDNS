# Cloudflare DDNS Service

Cloudflare Dynamic DNS service. This .NET application will check your public IP address on a
specified interval, and create a DNS record if it does not exist, or update it if the IP
address has changed.

## Configuration

| Variable | Default | Description |
|--|--|--|
| `CloudflareApi__ApiToken` | Required | Cloudflare API token with DNS Edit priviledges |
| `DDNS__ZoneName` | Required | The name of the zone. i.e. exmaple.com |
| `DDNS__DnsRecordName` | Required | The name of the DNS record. i.e. vpn.example.com |
| `DDNS__UpdateIntervalSeconds` | 900 (15min) | The delay between updates |

## Runing locally via Docker Compose

From the root of the repo

```
docker-compose up --build
```
