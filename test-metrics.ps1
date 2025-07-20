# Script to generate test traffic for OpenTelemetry metrics
Write-Host "Generating test traffic for OpenTelemetry metrics..." -ForegroundColor Green

$baseUrl = "http://localhost:5092"
$endpoints = @(
    "/weatherforecast",
    "/external-data",
    "/health",
    "/simulate-error?errorType=validation"
)

Write-Host "`nMaking 5 requests to each endpoint..." -ForegroundColor Yellow

foreach ($endpoint in $endpoints) {
    Write-Host "`nTesting endpoint: $endpoint" -ForegroundColor Cyan
    
    for ($i = 1; $i -le 5; $i++) {
        try {
            $response = Invoke-RestMethod -Uri "$baseUrl$endpoint" -Method GET -TimeoutSec 30
            Write-Host "  Request $i`: SUCCESS" -ForegroundColor Green
        }
        catch {
            Write-Host "  Request $i`: ERROR - $($_.Exception.Message)" -ForegroundColor Red
        }
        
        Start-Sleep -Milliseconds 500
    }
}

Write-Host "`nGenerating temperature metrics..." -ForegroundColor Yellow
for ($i = 1; $i -le 3; $i++) {
    $temp = Get-Random -Minimum -10 -Maximum 40
    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/metrics/temperature" -Method POST -Body $temp -ContentType "application/json" -TimeoutSec 30
        Write-Host "  Posted temperature: $temp°C" -ForegroundColor Green
    }
    catch {
        Write-Host "  Error posting temperature: $($_.Exception.Message)" -ForegroundColor Red
    }
    Start-Sleep -Milliseconds 300
}

Write-Host "`nTest completed! Check Prometheus and Grafana for metrics." -ForegroundColor Green
Write-Host "Prometheus: http://localhost:9090" -ForegroundColor Cyan
Write-Host "Grafana: http://localhost:3000" -ForegroundColor Cyan
Write-Host "Jaeger: http://localhost:16686" -ForegroundColor Cyan
