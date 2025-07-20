# OpenTelemetry Demo Test Script

Write-Host "OpenTelemetry Demo Test Script" -ForegroundColor Green
Write-Host "===============================" -ForegroundColor Green

# Get the base URL (using HTTP as the app runs on HTTP by default)
$baseUrl = "http://localhost:5092"

Write-Host "`n1. Testing Weather Forecast endpoint..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/weatherforecast" -Method GET
    Write-Host "✅ Weather forecast retrieved successfully" -ForegroundColor Green
    Write-Host "   Forecast count: $($response.Count)"
} catch {
    Write-Host "❌ Weather forecast failed: $($_.Exception.Message)" -ForegroundColor Red
}

Start-Sleep -Seconds 2

Write-Host "`n2. Testing External Data endpoint..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/external-data" -Method GET
    Write-Host "✅ External data retrieved successfully" -ForegroundColor Green
    Write-Host "   Response: $($response.message)"
} catch {
    Write-Host "❌ External data failed: $($_.Exception.Message)" -ForegroundColor Red
}

Start-Sleep -Seconds 2

Write-Host "`n3. Testing Error Simulation endpoints..." -ForegroundColor Yellow

# Test timeout error
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/simulate-error?errorType=timeout" -Method GET
    Write-Host "⚠️  Timeout error simulation completed" -ForegroundColor Yellow
} catch {
    Write-Host "✅ Timeout error handled correctly: $($_.Exception.Message)" -ForegroundColor Green
}

Start-Sleep -Seconds 1

# Test not found error
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/simulate-error?errorType=notfound" -Method GET
    Write-Host "⚠️  Not found error simulation completed" -ForegroundColor Yellow
} catch {
    Write-Host "✅ Not found error handled correctly: $($_.Exception.Message)" -ForegroundColor Green
}

Start-Sleep -Seconds 2

Write-Host "`n4. Testing Temperature Metrics endpoint..." -ForegroundColor Yellow
try {
    $temperatures = @(22.5, 18.3, 25.7, 19.8, 21.2)
    foreach ($temp in $temperatures) {
        $response = Invoke-RestMethod -Uri "$baseUrl/metrics/temperature?temperature=$temp" -Method POST
        Write-Host "✅ Temperature $temp°C recorded" -ForegroundColor Green
        Write-Host "   Response: $($response.message)" -ForegroundColor Gray
        Start-Sleep -Milliseconds 500
    }
} catch {
    Write-Host "❌ Temperature recording failed: $($_.Exception.Message)" -ForegroundColor Red
}

Start-Sleep -Seconds 2

Write-Host "`n5. Testing Health Check endpoint..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/health" -Method GET
    Write-Host "✅ Health check completed" -ForegroundColor Green
    Write-Host "   Status: $($response.status)"
    Write-Host "   Memory: $($response.metrics.memory_mb) MB"
} catch {
    Write-Host "❌ Health check failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n🎉 Test script completed!" -ForegroundColor Cyan
Write-Host "Check the console output of your running application to see the OpenTelemetry data!" -ForegroundColor Cyan

# Instructions
Write-Host "`n📋 Next Steps:" -ForegroundColor Magenta
Write-Host "1. Start the application: dotnet run" -ForegroundColor White
Write-Host "2. Open Swagger UI: http://localhost:5092/swagger" -ForegroundColor White
Write-Host "3. Run this script: .\test-otel-demo.ps1" -ForegroundColor White
Write-Host "4. Observe the rich telemetry data in the console!" -ForegroundColor White
