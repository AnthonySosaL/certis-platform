# Starts the whole local dev environment: Postgres (Docker), the frontend
# dev server, the backend dev server (once it exists), and the task widget.
# Run via the "NutriBoost - Start" Desktop shortcut, or directly:
#   powershell -ExecutionPolicy Bypass -File scripts\start-dev.ps1

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

Write-Host "Starting Postgres (Docker)..." -ForegroundColor Cyan
try {
    docker compose -f "$root\docker-compose.yml" up -d
} catch {
    Write-Warning "Docker didn't start Postgres — is Docker Desktop running? Start it and re-run this script."
}

Write-Host "Starting client-frontend dev server..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList @(
    '-NoExit', '-Command',
    "`$host.ui.RawUI.WindowTitle = 'NutriBoost - client-frontend'; Set-Location '$root\client-frontend'; npm run dev"
)

$backendCsproj = Get-ChildItem -Path "$root\client-backend" -Filter '*.Api.csproj' -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
if ($backendCsproj) {
    Write-Host "Starting client-backend dev server..." -ForegroundColor Cyan
    Start-Process powershell -ArgumentList @(
        '-NoExit', '-Command',
        "`$host.ui.RawUI.WindowTitle = 'NutriBoost - client-backend'; Set-Location '$($backendCsproj.DirectoryName)'; dotnet watch run"
    )
} else {
    Write-Host "client-backend not scaffolded yet — skipping." -ForegroundColor DarkYellow
}

Write-Host "Opening task widget..." -ForegroundColor Cyan
$edge = "$env:ProgramFiles (x86)\Microsoft\Edge\Application\msedge.exe"
if (Test-Path $edge) {
    Start-Process $edge -ArgumentList "--app=file:///$($root -replace '\\','/')/widget/index.html", '--window-size=320,480'
} else {
    Start-Process "$root\widget\index.html"
}

Write-Host "Done. Frontend will be at http://localhost:5173 once Vite finishes starting." -ForegroundColor Green
