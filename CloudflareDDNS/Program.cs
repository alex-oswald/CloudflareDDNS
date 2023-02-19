using Serilog.Events;
using Serilog;
using Microsoft.AspNetCore.Builder;
using CloudflareDDNS;
using Microsoft.Extensions.DependencyInjection;
using DnsClient;
using CloudflareDDNS.Api;
using Microsoft.Extensions.Options;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Debug()
    .CreateLogger();

try
{
    Log.Information("Starting CloudflareDDNS host");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();
    builder.Services.AddHostedService<DDNSBackgroundService>();
    builder.Services.AddTransient<ILookupClient, LookupClient>();
    builder.Services.AddTransient<IPublicIPResolver, PublicIPResolver>();
    builder.Services.AddTransient<ICloudflareApi, CloudflareApi>(sp =>
    {
        var logger = sp.GetRequiredService<ILogger<CloudflareApi>>();
        var options = sp.GetRequiredService<IOptions<CloudflareApiOptions>>();
        return new CloudflareApi(logger, options, new HttpClient());
    });
    builder.Services.AddOptions<DDNSOptions>()
        .Bind(builder.Configuration.GetSection(DDNSOptions.Section))
        .ValidateDataAnnotations();
    builder.Services.AddOptions<CloudflareApiOptions>()
        .Bind(builder.Configuration.GetSection(CloudflareApiOptions.Section))
        .ValidateDataAnnotations();

    var app = builder.Build();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.Information("Host ended, flushing log...");
    Log.CloseAndFlush();
}