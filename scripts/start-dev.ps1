# Starts the whole local dev environment: Postgres (Docker), the frontend
# dev server (Vite, hot-reloads on save), the backend dev server (dotnet
# watch, hot-reloads on save), and the task widget.
#
# Safe to run again while things are already running - it checks each
# port first and skips anything already up, instead of spawning duplicate
# windows. That's the point: start it once, then just save files and let
# Vite/dotnet watch reload themselves. No need to stop/start per change.
#
# Run via the Desktop shortcut, or directly:
#   powershell -ExecutionPolicy Bypass -File scripts\start-dev.ps1

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

# The .NET SDK is installed per-user (not machine-wide), and this process
# may have been spawned before that PATH change was visible system-wide
# (Explorer/desktop shortcuts don't pick up a PATH change until logoff or
# reboot). Prepend it explicitly so "dotnet" always resolves here and in
# any window this script spawns, regardless of ambient PATH staleness.
$dotnetPaths = @("$env:USERPROFILE\.dotnet", "$env:USERPROFILE\.dotnet\tools")
foreach ($p in $dotnetPaths) {
    if ($env:Path -notlike "*$p*") {
        $env:Path = "$p;$env:Path"
    }
}

function Test-PortOpen($port) {
    return [bool](Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue)
}

Write-Host "Starting Postgres (Docker)..." -ForegroundColor Cyan
try {
    docker compose -f "$root\docker-compose.yml" up -d
} catch {
    Write-Warning "Docker didn't start Postgres - is Docker Desktop running? Start it and re-run this script."
}

if (Test-PortOpen 5173) {
    Write-Host "client-frontend already running on :5173 - skipping." -ForegroundColor DarkYellow
} else {
    Write-Host "Starting client-frontend dev server..." -ForegroundColor Cyan
    Start-Process powershell -ArgumentList @(
        '-NoExit', '-Command',
        "`$host.ui.RawUI.WindowTitle = 'Project - client-frontend'; Set-Location '$root\client-frontend'; npm run dev"
    )
}

$backendCsproj = Get-ChildItem -Path "$root\client-backend" -Filter '*.Api.csproj' -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
if ($backendCsproj) {
    if (Test-PortOpen 5223) {
        Write-Host "client-backend already running on :5223 - skipping." -ForegroundColor DarkYellow
    } else {
        Write-Host "Starting client-backend dev server..." -ForegroundColor Cyan
        Start-Process powershell -ArgumentList @(
            '-NoExit', '-Command',
            "`$host.ui.RawUI.WindowTitle = 'Project - client-backend'; Set-Location '$($backendCsproj.DirectoryName)'; dotnet watch run --urls http://localhost:5223"
        )
    }
} else {
    Write-Host "client-backend not scaffolded yet - skipping." -ForegroundColor DarkYellow
}

Write-Host "Opening task widget..." -ForegroundColor Cyan
$edge = "$env:ProgramFiles (x86)\Microsoft\Edge\Application\msedge.exe"
if (Test-Path $edge) {
    Start-Process $edge -ArgumentList "--app=file:///$($root -replace '\\','/')/widget/index.html", '--window-size=320,480'
} else {
    Start-Process "$root\widget\index.html"
}

Write-Host ""
Write-Host "Done. Frontend: http://localhost:5173  |  Backend: http://localhost:5223/swagger" -ForegroundColor Green
Write-Host "Both auto-reload on save - just edit and save, no need to re-run this." -ForegroundColor Green
