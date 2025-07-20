# OpenTelemetry Visualization Setup

This setup provides a complete local observability stack for your .NET OpenTelemetry demo application.

## 🏗️ Architecture

```
.NET App → OpenTelemetry Collector → Jaeger (Traces) + Prometheus (Metrics) → Grafana (Visualization)
```

## 🚀 Quick Start

### Prerequisites
- Docker Desktop installed and running
- .NET 8 SDK
- PowerShell

### 1. Start the Observability Stack
```powershell
.\setup-observability.ps1
```

### 2. Build and Run Your .NET Application
```powershell
cd OtelDemo
dotnet build
dotnet run
```

### 3. Generate Some Traffic
```powershell
.\test-otel-demo.ps1
```

### 4. Explore the Data!

## 📊 Available Tools

### Jaeger UI - Distributed Tracing
- **URL**: http://localhost:16686
- **Purpose**: View distributed traces, request flows, and performance bottlenecks
- **What to look for**:
  - Trace timelines showing request flow
  - Span details with tags and logs
  - Service dependencies
  - Error traces

### Grafana - Dashboards & Visualization
- **URL**: http://localhost:3000
- **Login**: admin / admin
- **Purpose**: Create dashboards combining metrics and traces
- **Pre-configured**:
  - OpenTelemetry .NET Demo dashboard
  - Prometheus data source
  - Jaeger data source

### Prometheus - Metrics Storage
- **URL**: http://localhost:9090
- **Purpose**: Query and explore raw metrics
- **Available metrics**:
  - `otel_demo_requests_total` - Request counts
  - `otel_demo_request_duration_ms` - Request latencies
  - Standard ASP.NET Core metrics
  - HTTP client metrics

## 🔍 What You'll See

### In Jaeger:
1. **Service Map**: Visual representation of your application services
2. **Traces**: Individual request traces showing:
   - Weather forecast generation with detailed spans
   - External API calls to httpbin.org
   - Error simulation traces
   - Temperature recording operations

### In Grafana:
1. **Request Rate**: Requests per second by endpoint
2. **Response Times**: P50, P95 latency percentiles
3. **Error Rates**: Failed requests over time
4. **Custom Metrics**: Temperature recordings, health checks

### In Prometheus:
1. **Raw Metrics**: All OpenTelemetry metrics in PromQL format
2. **Service Discovery**: Automatic discovery of metric endpoints
3. **Alerting**: (Can be configured for production use)

## 🛠️ Customization

### Adding More Dashboards
1. Create JSON files in `grafana/dashboards/`
2. Restart Grafana: `docker-compose restart grafana`

### Adding Alerting
1. Configure Prometheus alerting rules in `prometheus.yml`
2. Add Grafana notification channels

### Scaling for Production
- Replace Jaeger all-in-one with Jaeger production deployment
- Use external Prometheus with persistent storage
- Configure proper retention policies
- Add authentication and authorization

## 🔧 Troubleshooting

### Services Not Starting
```powershell
# Check Docker containers
docker-compose ps

# View logs
docker-compose logs jaeger
docker-compose logs otel-collector
docker-compose logs prometheus
docker-compose logs grafana
```

### No Data Appearing
1. Ensure your .NET app is running and configured with OTLP exporter
2. Check OTLP collector logs: `docker-compose logs otel-collector`
3. Verify your app can reach localhost:4317 (gRPC) or localhost:4318 (HTTP)

### Port Conflicts
If ports are already in use, modify `docker-compose.yml`:
- Jaeger UI: Change `16686:16686` to `<new-port>:16686`
- Grafana: Change `3000:3000` to `<new-port>:3000`
- Prometheus: Change `9090:9090` to `<new-port>:9090`

## 🛑 Cleanup

Stop and remove all containers:
```powershell
docker-compose down -v
```

Remove images (optional):
```powershell
docker-compose down --rmi all -v
```

## 📚 Learn More

- [OpenTelemetry Documentation](https://opentelemetry.io/docs/)
- [Jaeger Documentation](https://www.jaegertracing.io/docs/)
- [Grafana Documentation](https://grafana.com/docs/)
- [Prometheus Documentation](https://prometheus.io/docs/)

## 🎯 Production Considerations

This setup is designed for local development and learning. For production:

1. **Security**: Add authentication, HTTPS, network policies
2. **Persistence**: Configure persistent storage for metrics and traces
3. **Scalability**: Use clustered deployments
4. **Monitoring**: Monitor the monitoring stack itself
5. **Backup**: Regular backups of configuration and data
6. **Retention**: Configure appropriate data retention policies
