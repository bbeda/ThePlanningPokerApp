# Build frontend for production deployment
Write-Host "Building Vue.js frontend..." -ForegroundColor Green

Set-Location "src\PlanningPoker.Web"

# Install dependencies if needed
if (-not (Test-Path "node_modules")) {
    Write-Host "Installing npm dependencies..." -ForegroundColor Yellow
    npm ci
    if ($LASTEXITCODE -ne 0) {
        Set-Location ..\..
        Write-Host "[ERROR] Failed to install npm dependencies" -ForegroundColor Red
        exit 1
    }
}

# Build for production
Write-Host "Building frontend assets..." -ForegroundColor Yellow
npm run build

if ($LASTEXITCODE -ne 0) {
    Set-Location ..\..
    Write-Host "[ERROR] Frontend build failed" -ForegroundColor Red
    exit 1
}

Write-Host "[SUCCESS] Frontend built successfully to src/PlanningPoker.Api/wwwroot" -ForegroundColor Green

Set-Location ..\..
