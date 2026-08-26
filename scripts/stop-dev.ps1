# Stops the local dev environment started by start-dev.ps1: kills whatever
# is listening on the frontend/backend dev ports (and its process tree,
# so npm/vite or dotnet's child processes go with it, not just the wrapper
# window) and stops Postgres.
#
# Finds processes by port rather than by window title - title matching is
# unreliable (a spawned console window's title isn't always readable back
# from outside it), port ownership isn't.
#
# Run via the Desktop shortcut, or directly:
#   powershell -ExecutionPolicy Bypass -File scripts\stop-dev.ps1

$root = Split-Path -Parent $PSScriptRoot

function Stop-Port($port, $label) {
    $conns = Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue
    if (-not $conns) {
        Write-Host "$label (:$port) not running." -ForegroundColor DarkYellow
        return
    }
    $pids = $conns | Select-Object -ExpandProperty OwningProcess -Unique
    foreach ($ownerPid in $pids) {
        Write-Host "Stopping $label (:$port, pid $ownerPid)..." -ForegroundColor Cyan
        taskkill /F /T /PID $ownerPid | Out-Null
    }
}

Stop-Port 5173 "client-frontend"
Stop-Port 5223 "client-backend"

Write-Host "Stopping Postgres (Docker)..." -ForegroundColor Cyan
try {
    docker compose -f "$root\docker-compose.yml" stop
} catch {
    Write-Warning "Could not stop Docker containers - Docker Desktop may not be running."
}

Write-Host "Done." -ForegroundColor Green
