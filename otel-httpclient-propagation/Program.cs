using System.Diagnostics;
using System.Text;
using OpenTelemetry;
using OpenTelemetry.Trace;

var appUrl = "http://localhost:8042";

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddLogging(options =>
    {
        options
            .ClearProviders()
            .AddConsole();
    })
    .AddOpenTelemetry().WithTracing(b =>
    {
        b.AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddConsoleExporter();
    });

builder.Services.AddHttpClient();

var app = builder.Build();

app.MapGet("/", async (HttpContext context, IHttpClientFactory clientFactory) =>
{
    Activity.Current?.SetBaggage("ActivityBag", "foobar");
    Activity.Current?.SetTag("ActivityTag", "barfoo");
    Baggage.Current.SetBaggage("OtelBag1", "foofoo");
    Baggage.SetBaggage("OtelBag2", "foofoo1");

    using var client = clientFactory.CreateClient();
    var response = await client.GetAsync(new Uri($"{appUrl}/internal"));
    await context.Response.WriteAsync(await response.Content.ReadAsStringAsync());
});

app.MapGet("/internal", async context =>
{
    StringBuilder sb = new StringBuilder();

    sb.AppendLine("HTTP Headers:");
    foreach (var header in context.Request.Headers)
    {
        sb.AppendLine($"{header.Key}={header.Value}");
    }

    sb.AppendLine();
    sb.AppendLine("Activity Baggage:");
    foreach (var bag in Activity.Current?.Baggage ?? new List<KeyValuePair<string, string?>>())
    {
        sb.AppendLine($"{bag.Key}={bag.Value}");
    }
    
    sb.AppendLine();
    sb.AppendLine("OTEL Baggage:");
    foreach (var bag in Baggage.Current)
    {
        sb.AppendLine($"{bag.Key}={bag.Value}");
    }

    sb.AppendLine();
    sb.AppendLine("Tags:");
    foreach (var tag in Activity.Current?.Tags ?? new List<KeyValuePair<string, string?>>())
    {
        sb.AppendLine($"{tag.Key}={tag.Value}");
    }

    if (sb.Length > 0)
    {
        await context.Response.WriteAsync(sb.ToString());
        return;
    }

    await context.Response.WriteAsync("No baggage!!");
});

app.Run(appUrl);