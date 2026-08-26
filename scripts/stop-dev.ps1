# Stops the local dev environment started by start-dev.ps1: closes the
# frontend/backend terminal windows (matched by title) and stops Postgres.
# Run via the "NutriBoost - Stop" Desktop shortcut, or directly:
#   powershell -ExecutionPolicy Bypass -File scripts\stop-dev.ps1

$root = Split-Path -Parent $PSScriptRoot

Write-Host "Stopping dev server windows..." -ForegroundColor Cyan
Get-Process powershell -ErrorAction SilentlyContinue |
    Where-Object { $_.MainWindowTitle -like 'NutriBoost - *' } |
    ForEach-Object {
        Write-Host "  closing: $($_.MainWindowTitle)"
        Stop-Process -Id $_.Id -Force
    }

Write-Host "Stopping Postgres (Docker)..." -ForegroundColor Cyan
try {
    docker compose -f "$root\docker-compose.yml" stop
} catch {
    Write-Warning "Could not stop Docker containers — Docker Desktop may not be running."
}

Write-Host "Done." -ForegroundColor Green
