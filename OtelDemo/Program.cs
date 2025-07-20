using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Diagnostics.Metrics;

var builder = WebApplication.CreateBuilder(args);

// Create a custom activity source for our application
var activitySource = new ActivitySource("OtelDemo.WebApi");

// Create custom metrics
var meter = new Meter("OtelDemo.WebApi", "1.0.0");
var requestCounter = meter.CreateCounter<int>("otel_demo_requests_total", "requests", "Total number of requests");
var requestDuration = meter.CreateHistogram<double>("otel_demo_request_duration_ms", "milliseconds", "Request duration in milliseconds");

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

// Register our custom metrics and activity source as singletons
builder.Services.AddSingleton(activitySource);
builder.Services.AddSingleton(meter);
builder.Services.AddSingleton(requestCounter);
builder.Services.AddSingleton(requestDuration);

// Configure OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerBuilder =>
    {
        tracerBuilder
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService("OtelDemo.WebApi", "1.0.0")
                .AddAttributes(new[]
                {
                    new KeyValuePair<string, object>("deployment.environment", builder.Environment.EnvironmentName),
                    new KeyValuePair<string, object>("service.instance.id", Environment.MachineName)
                }))
            .AddSource(activitySource.Name)
            .AddAspNetCoreInstrumentation(options =>
            {
                options.RecordException = true;
                options.EnrichWithHttpRequest = (activity, httpRequest) =>
                {
                    activity.SetTag("http.request.body.size", httpRequest.ContentLength);
                };
                options.EnrichWithHttpResponse = (activity, httpResponse) =>
                {
                    activity.SetTag("http.response.body.size", httpResponse.ContentLength);
                };
            })
            .AddHttpClientInstrumentation(options =>
            {
                options.RecordException = true;
            })
            .AddConsoleExporter()
            .AddJaegerExporter(options =>
            {
                options.Endpoint = new Uri("http://localhost:14268/api/traces");
                options.Protocol = OpenTelemetry.Exporter.JaegerExportProtocol.HttpBinaryThrift;
            });
    })
    .WithMetrics(metricsBuilder =>
    {
        metricsBuilder
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService("OtelDemo.WebApi", "1.0.0"))
            .AddMeter(meter.Name)
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddConsoleExporter()
            .AddPrometheusExporter();
    });

// Configure logging with OpenTelemetry
builder.Logging.AddOpenTelemetry(options =>
{
    options.SetResourceBuilder(ResourceBuilder.CreateDefault()
        .AddService("OtelDemo.WebApi", "1.0.0"));
    options.AddConsoleExporter();
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add OpenTelemetry Prometheus scraping endpoint
app.MapPrometheusScrapingEndpoint();

app.UseHttpsRedirection();

// Middleware to track request metrics
app.Use(async (context, next) =>
{
    var stopwatch = Stopwatch.StartNew();
    
    // Increment request counter
    requestCounter.Add(1, new KeyValuePair<string, object?>("method", context.Request.Method),
                          new KeyValuePair<string, object?>("endpoint", context.Request.Path.Value ?? "unknown"));
    
    try
    {
        await next();
    }
    finally
    {
        stopwatch.Stop();
        
        // Record request duration
        requestDuration.Record(stopwatch.Elapsed.TotalMilliseconds,
            new KeyValuePair<string, object?>("method", context.Request.Method),
            new KeyValuePair<string, object?>("endpoint", context.Request.Path.Value ?? "unknown"),
            new KeyValuePair<string, object?>("status_code", context.Response.StatusCode.ToString()));
    }
});

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

// Enhanced weather forecast endpoint with detailed observability
app.MapGet("/weatherforecast", async (ILogger<Program> logger, HttpContext context) =>
{
    using var activity = activitySource.StartActivity("GetWeatherForecast");
    activity?.SetTag("operation.name", "get-weather-forecast");
    activity?.SetTag("operation.version", "1.0");
    
    logger.LogInformation("Starting weather forecast generation");
    
    try
    {
        // Simulate some processing time
        await Task.Delay(Random.Shared.Next(100, 500));
        
        var forecast = Enumerable.Range(1, 5).Select(index =>
        {
            var temp = Random.Shared.Next(-20, 55);
            var summary = summaries[Random.Shared.Next(summaries.Length)];
            
            // Add custom span events
            activity?.AddEvent(new ActivityEvent($"Generated forecast for day {index}", 
                DateTimeOffset.UtcNow,
                new ActivityTagsCollection(new[]
                {
                    new KeyValuePair<string, object?>("day", index),
                    new KeyValuePair<string, object?>("temperature", temp),
                    new KeyValuePair<string, object?>("summary", summary)
                })));
            
            return new WeatherForecast(
                DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                temp,
                summary
            );
        })
        .ToArray();
        
        activity?.SetTag("forecast.count", forecast.Length);
        activity?.SetTag("forecast.min_temp", forecast.Min(f => f.TemperatureC));
        activity?.SetTag("forecast.max_temp", forecast.Max(f => f.TemperatureC));
        
        logger.LogInformation("Successfully generated {Count} weather forecasts", forecast.Length);
        
        return Results.Ok(forecast);
    }
    catch (Exception ex)
    {
        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
        activity?.AddException(ex);
        
        logger.LogError(ex, "Failed to generate weather forecast");
        
        return Results.Problem("Failed to generate weather forecast");
    }
})
.WithName("GetWeatherForecast")
.WithOpenApi();

// Endpoint to demonstrate HTTP client instrumentation
app.MapGet("/external-data", async (ILogger<Program> logger, HttpClient httpClient) =>
{
    using var activity = activitySource.StartActivity("GetExternalData");
    activity?.SetTag("operation.name", "get-external-data");
    
    logger.LogInformation("Fetching data from external service");
    
    try
    {
        // Make HTTP call to demonstrate HTTP client instrumentation
        // Using a reliable public API that doesn't require authentication
        var response = await httpClient.GetStringAsync("https://httpbin.org/json");
        
        activity?.SetTag("external.response.length", response.Length);
        activity?.SetTag("external.service", "httpbin.org");
        
        logger.LogInformation("Successfully fetched external data, length: {Length}", response.Length);
        
        // Parse and return a summary to make it more interesting
        var jsonData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(response);
        
        return Results.Ok(new { 
            message = "External data fetched successfully", 
            length = response.Length,
            service = "httpbin.org",
            data_preview = jsonData?.Take(3).ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
        });
    }
    catch (Exception ex)
    {
        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
        activity?.AddException(ex);
        
        logger.LogError(ex, "Failed to fetch external data");
        
        return Results.Problem("Failed to fetch external data");
    }
})
.WithName("GetExternalData")
.WithOpenApi();

// Endpoint to demonstrate error handling and observability
app.MapGet("/simulate-error", (ILogger<Program> logger, string? errorType) =>
{
    using var activity = activitySource.StartActivity("SimulateError");
    activity?.SetTag("operation.name", "simulate-error");
    activity?.SetTag("error.type", errorType ?? "default");
    
    logger.LogInformation("Simulating error of type: {ErrorType}", errorType ?? "default");
    
    try
    {
        switch (errorType?.ToLower())
        {
            case "timeout":
                logger.LogWarning("Simulating timeout error");
                throw new TimeoutException("Simulated timeout occurred");
                
            case "notfound":
                logger.LogWarning("Simulating not found error");
                return Results.NotFound("Resource not found");
                
            case "validation":
                logger.LogWarning("Simulating validation error");
                return Results.BadRequest("Invalid input provided");
                
            default:
                logger.LogError("Simulating generic error");
                throw new InvalidOperationException("Simulated error occurred");
        }
    }
    catch (Exception ex)
    {
        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
        activity?.AddException(ex);
        
        logger.LogError(ex, "Error simulation completed");
        
        return Results.Problem($"Simulated error: {ex.Message}");
    }
})
.WithName("SimulateError")
.WithOpenApi();

// Endpoint to generate custom metrics
app.MapPost("/metrics/temperature", (double temperature, ILogger<Program> logger) =>
{
    using var activity = activitySource.StartActivity("RecordTemperature");
    activity?.SetTag("operation.name", "record-temperature");
    activity?.SetTag("temperature.value", temperature);
    
    logger.LogInformation("Recording temperature measurement: {Temperature}°C", temperature);
    
    // Update observable gauge (this would typically be done in a background service)
    // For demo purposes, we'll just log it
    logger.LogInformation("Temperature recorded successfully");
    
    return Results.Ok(new { message = "Temperature recorded", value = temperature, unit = "celsius" });
})
.WithName("RecordTemperature")
.WithOpenApi();

// Endpoint to get application health with metrics
app.MapGet("/health", (ILogger<Program> logger) =>
{
    using var activity = activitySource.StartActivity("HealthCheck");
    activity?.SetTag("operation.name", "health-check");
    
    var healthData = new
    {
        status = "healthy",
        timestamp = DateTime.UtcNow,
        version = "1.0.0",
        environment = app.Environment.EnvironmentName,
        uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime,
        metrics = new
        {
            memory_mb = GC.GetTotalMemory(false) / 1024 / 1024,
            gc_collections = new
            {
                gen0 = GC.CollectionCount(0),
                gen1 = GC.CollectionCount(1),
                gen2 = GC.CollectionCount(2)
            }
        }
    };
    
    activity?.SetTag("health.status", healthData.status);
    activity?.SetTag("health.memory_mb", healthData.metrics.memory_mb);
    
    logger.LogInformation("Health check completed - Status: {Status}, Memory: {Memory}MB", 
        healthData.status, healthData.metrics.memory_mb);
    
    return Results.Ok(healthData);
})
.WithName("GetHealth")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
