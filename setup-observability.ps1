# OpenTelemetry Visualization Setup Script

Write-Host "🚀 Setting up OpenTelemetry Visualization Stack" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green

# Check if Docker is running
Write-Host "`n📋 Checking Docker..." -ForegroundColor Yellow
try {
    docker --version | Out-Null
    Write-Host "✅ Docker is available" -ForegroundColor Green
} catch {
    Write-Host "❌ Docker is not available. Please install Docker Desktop first." -ForegroundColor Red
    Write-Host "Download: https://www.docker.com/products/docker-desktop" -ForegroundColor Cyan
    exit 1
}

# Start the observability stack
Write-Host "`n🐳 Starting observability stack..." -ForegroundColor Yellow
try {
    docker-compose up -d
    Write-Host "✅ Observability stack started successfully!" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to start observability stack: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Wait for services to be ready
Write-Host "`n⏳ Waiting for services to be ready..." -ForegroundColor Yellow
Start-Sleep -Seconds 30

# Check service health
Write-Host "`n🔍 Checking service health..." -ForegroundColor Yellow

$services = @(
    @{Name="Jaeger UI"; Url="http://localhost:16686"; Description="Distributed Tracing"},
    @{Name="Grafana"; Url="http://localhost:3000"; Description="Dashboards & Visualization"},
    @{Name="Prometheus"; Url="http://localhost:9090"; Description="Metrics Storage"},
    @{Name="OTLP Collector"; Url="http://localhost:4318/v1/traces"; Description="Telemetry Collector"}
)

foreach ($service in $services) {
    try {
        $response = Invoke-WebRequest -Uri $service.Url -Method GET -TimeoutSec 5 -ErrorAction Stop
        Write-Host "✅ $($service.Name) is ready" -ForegroundColor Green
    } catch {
        Write-Host "⚠️  $($service.Name) might still be starting up" -ForegroundColor Yellow
    }
}

Write-Host "`n🎉 Setup Complete!" -ForegroundColor Cyan
Write-Host "====================" -ForegroundColor Cyan

Write-Host "`n📊 Available Services:" -ForegroundColor Magenta
Write-Host "• Jaeger UI:     http://localhost:16686 (Distributed Tracing)" -ForegroundColor White
Write-Host "• Grafana:       http://localhost:3000  (Dashboards - admin/admin)" -ForegroundColor White
Write-Host "• Prometheus:    http://localhost:9090  (Metrics Storage)" -ForegroundColor White
Write-Host "• OTLP Endpoint: http://localhost:4317  (gRPC) / 4318 (HTTP)" -ForegroundColor White

Write-Host "`n🚀 Next Steps:" -ForegroundColor Magenta
Write-Host "1. Build and run your .NET app: dotnet run" -ForegroundColor White
Write-Host "2. Run the test script: .\test-otel-demo.ps1" -ForegroundColor White
Write-Host "3. View traces in Jaeger: http://localhost:16686" -ForegroundColor White
Write-Host "4. View metrics in Grafana: http://localhost:3000" -ForegroundColor White
Write-Host "5. Explore raw metrics in Prometheus: http://localhost:9090" -ForegroundColor White

Write-Host "`n🛑 To stop the stack:" -ForegroundColor Yellow
Write-Host "   docker-compose down" -ForegroundColor White
