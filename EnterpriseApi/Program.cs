using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using EnterpriseApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add OpenTelemetry
builder.Services.AddOpenTelemetry()
.ConfigureResource(resource => resource.AddService(builder.Configuration["OTEL_SERVICE_NAME"]))
.WithTracing(tracing => 
{
    tracing.AddAspNetCoreInstrumentation();
    tracing.AddOtlpExporter();
})
.WithMetrics(metrics => 
{
    metrics.AddAspNetCoreInstrumentation();
    metrics.AddOtlpExporter();
});

builder.Logging.AddOpenTelemetry(options => 
{
    options.IncludeFormattedMessage = true;
    options.IncludeScopes = true;
    options.AddOtlpExporter();
});

builder.Services.AddProblemDetails(options => 
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Title = "Error";
        context.ProblemDetails.Status = StatusCodes.Status500InternalServerError;
        context.ProblemDetails.Detail = "Ocurrió un error interno.";
        context.ProblemDetails.Instance = context.HttpContext.Request.Path;
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
    };
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseNotFoundMiddleware();
app.UseExceptionHandler();
app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
