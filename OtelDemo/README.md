# OpenTelemetry Demo - .NET 8

This demo application showcases the power of OpenTelemetry in .NET 8, demonstrating comprehensive observability through **tracing**, **metrics**, and **logging**.

## 🚀 What is OpenTelemetry?

OpenTelemetry is an open-source observability framework that provides a vendor-neutral way to collect, process, and export telemetry data (metrics, logs, and traces) from your applications.

## 🆚 OpenTelemetry vs Plain ILogger

### Traditional ILogger Approach

```csharp
logger.LogInformation("Processing request for user {UserId}", userId);
logger.LogInformation("Database query took {Duration}ms", duration);
logger.LogError("Failed to process request: {Error}", error);
```

**Limitations:**
- ❌ **No correlation**: Logs from the same request are scattered
- ❌ **Limited context**: Hard to trace request flow across services
- ❌ **No performance insights**: Manual timing and metric collection
- ❌ **Vendor lock-in**: Tied to specific logging providers
- ❌ **No distributed tracing**: Can't follow requests across microservices

### OpenTelemetry Approach

```csharp
using var activity = activitySource.StartActivity("ProcessUser");
activity?.SetTag("user.id", userId);

logger.LogInformation("Processing request for user {UserId}", userId);
// OpenTelemetry automatically correlates this log with the active trace

requestCounter.Add(1, new TagList { ["operation"] = "process-user" });
```

**Advantages:**
- ✅ **Automatic correlation**: All telemetry from the same request is linked
- ✅ **Distributed tracing**: Follow requests across multiple services
- ✅ **Rich context**: Tags, attributes, and events provide detailed insights
- ✅ **Built-in metrics**: Automatic collection of performance data
- ✅ **Vendor neutral**: Works with any observability backend
- ✅ **Standardized**: Industry-standard semantic conventions

## 🎯 Key Benefits Demonstrated

### 1. **Distributed Tracing**
- **Request correlation**: Every log entry is automatically linked to its trace
- **Service dependencies**: See how requests flow between services
- **Performance bottlenecks**: Identify slow operations with timing data
- **Error propagation**: Track how errors spread through your system

### 2. **Rich Metrics**
- **Business metrics**: Custom counters, histograms, and gauges
- **Runtime metrics**: Memory usage, GC collections, thread pool stats
- **Infrastructure metrics**: HTTP request rates, response times, error rates
- **Resource utilization**: CPU, memory, disk I/O tracking

### 3. **Contextual Logging**
- **Structured logging**: JSON format with consistent fields
- **Trace correlation**: Every log automatically includes trace/span IDs
- **Custom attributes**: Rich metadata attached to log entries
- **Exception tracking**: Automatic exception capture and correlation

## 🛠️ Demo Endpoints

### 1. Weather Forecast (`GET /weatherforecast`)
Demonstrates:
- Custom tracing with detailed span attributes
- Structured logging with correlation
- Custom metrics for request tracking
- Exception handling and error reporting

### 2. External Data (`GET /external-data`)
Demonstrates:
- HTTP client instrumentation
- Outbound request tracing
- Service dependency mapping
- Network call performance metrics

### 3. Error Simulation (`GET /simulate-error?errorType=timeout`)
Demonstrates:
- Error tracing and reporting
- Exception correlation
- Different error types and status codes
- Error rate metrics

Supported error types: `timeout`, `notfound`, `validation`, or none for generic error

### 4. Temperature Recording (`POST /metrics/temperature`)
Demonstrates:
- Custom business metrics
- Manual metric recording
- Tagged metrics for dimensional analysis

Example:
```bash
curl -X POST "https://localhost:7xxx/metrics/temperature" -H "Content-Type: application/json" -d "25.5"
```

### 5. Health Check (`GET /health`)
Demonstrates:
- Application health monitoring
- Runtime metrics exposure
- System resource tracking

## 📊 What You'll See in the Console

The console exporter will display rich telemetry data:

### Traces
```
Activity.TraceId:            8d2c4f6a1b9e3f7d2c5a8b4e6f1a9c3d
Activity.SpanId:             a1b2c3d4e5f6a7b8
Activity.TraceFlags:         Recorded
Activity.ActivitySourceName: OtelDemo.WebApi
Activity.DisplayName:        GetWeatherForecast
Activity.Kind:               Internal
Activity.StartTime:          2024-01-20T10:30:45.123Z
Activity.Duration:           00:00:00.456
Activity.Tags:
    operation.name: get-weather-forecast
    forecast.count: 5
    forecast.min_temp: -5
    forecast.max_temp: 22
```

### Metrics
```
Metric: otel_demo_requests_total
Value: 15
Tags: method=GET, endpoint=/weatherforecast

Metric: otel_demo_request_duration_ms
Value: 456.78
Tags: method=GET, endpoint=/weatherforecast, status_code=200
```

### Logs
```
[2024-01-20 10:30:45.123] Information: Starting weather forecast generation
TraceId: 8d2c4f6a1b9e3f7d2c5a8b4e6f1a9c3d
SpanId: a1b2c3d4e5f6a7b8
```

## 🚀 Running the Demo

1. **Start the application:**
   ```bash
   dotnet run
   ```

2. **Open Swagger UI:**
   Navigate to `https://localhost:7xxx/swagger` (port may vary)

3. **Make some requests:**
   - Try the weather forecast endpoint
   - Simulate some errors
   - Record temperature metrics
   - Check the health endpoint

4. **Observe the telemetry:**
   Watch the console output to see traces, metrics, and logs with correlation

## 🏗️ Production Considerations

In production, instead of console exporters, you would configure:

- **Jaeger** or **Zipkin** for traces
- **Prometheus** for metrics  
- **Elasticsearch** or **Grafana Loki** for logs
- **OTLP exporters** for cloud platforms (Azure Monitor, AWS X-Ray, Google Cloud Trace)

Example configuration:
```csharp
.AddJaegerExporter()
.AddPrometheusExporter()
.AddOtlpExporter()
```

## 🎓 Key Takeaways

1. **Correlation is King**: OpenTelemetry automatically links all telemetry from the same request
2. **Standards Matter**: Using industry-standard semantic conventions improves observability
3. **Context is Everything**: Rich tags and attributes provide actionable insights
4. **Automation Wins**: Automatic instrumentation reduces manual effort and errors
5. **Vendor Neutrality**: One instrumentation works with multiple observability backends

OpenTelemetry transforms debugging from "finding needles in haystacks" to "following breadcrumbs with GPS coordinates"! 🗺️
