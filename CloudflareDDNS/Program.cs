using CloudflareDDNS;
using CloudflareDDNS.Api;
using DnsClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

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
    builder.Services.AddHttpClient();
    builder.Services.AddHostedService<DDNSBackgroundService>();
    builder.Services.AddTransient<ILookupClient, LookupClient>();
    builder.Services.AddTransient<IPublicIPResolver, PublicIPResolver>();
    builder.Services.AddTransient<ICloudflareApi, CloudflareApi>();
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