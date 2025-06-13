using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Context;
using Serilog.Sinks.Grafana.Loki;

var builder = WebApplication.CreateBuilder(args);


var labelDict = new Dictionary<string, string> { { "app", "dotnet-api" }, { "env", "dev" } };
var lokiLabels = labelDict.Select(kvp => new LokiLabel { Key = kvp.Key, Value = kvp.Value });

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.GrafanaLoki(
        uri: "http://localhost:3100",
        labels: lokiLabels,
        propertiesAsLabels: new[] { "level" })
    .CreateLogger();




builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSerilogRequestLogging();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.MapGet("/weatherforecast", () =>
{
    using (LogContext.PushProperty("RequestId", Guid.NewGuid().ToString()))
    {
        Log.Information("Fetching weather forecast");
        Log.Warning("This is a test warning log");
    }
    return Results.Ok(new { Message = "Weather forecast retrieved" });
})
.WithName("GetWeatherForecast");

app.Run();