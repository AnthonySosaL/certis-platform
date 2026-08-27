# Starts the local dev environment: the frontend dev server (Angular CLI,
# hot-reloads on save), the backend dev server (dotnet watch, hot-reloads
# on save), and the task widget. The backend connects straight to the real
# MonsterASP.NET SQL Server database (via user-secrets) - local Docker
# Postgres is no longer started automatically; it's a fallback for offline
# work only, run `docker compose up -d` yourself if you ever need it.
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

# The .NET SDK is installed per-user (not machine-wide). There's also a
# machine-wide dotnet.exe (runtime-only, no SDK) at C:\Program Files\dotnet
# that can end up earlier in PATH than the per-user one depending on the
# ambient environment - a plain "dotnet" call then silently resolves to
# the wrong one ("No .NET SDKs were found"). Prepending to $env:Path
# turned out not to reliably fix the *order* (a previous version of this
# fix skipped prepending whenever any dotnet path already appeared
# anywhere in PATH, which didn't guarantee position). Calling the SDK's
# dotnet.exe by its full path sidesteps PATH resolution entirely -
# see $dotnetExe below, used for every `dotnet` invocation in this script.
$dotnetExe = "$env:USERPROFILE\.dotnet\dotnet.exe"

function Test-PortOpen($port) {
    return [bool](Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue)
}

if (Test-PortOpen 4200) {
    Write-Host "client-frontend already running on :4200 - skipping." -ForegroundColor DarkYellow
} else {
    # ng serve's .angular/cache gets corrupted whenever the process is
    # killed abruptly (closing the window, a crash, a forced stop) instead
    # of exiting cleanly - the next run then hangs forever without ever
    # binding the port. Clearing the cache before every start costs a few
    # seconds of rebuild but avoids that hang entirely.
    Remove-Item "$root\client-frontend\.angular\cache" -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "Starting client-frontend dev server..." -ForegroundColor Cyan
    # ng serve also hangs (separately from the cache issue above) when its
    # output goes straight to a detached console window instead of being
    # redirected - confirmed by testing both ways repeatedly. Redirecting
    # to a log file avoids that; check the log if something looks wrong.
    $frontendLog = "$env:TEMP\ng-serve.log"
    Start-Process powershell -ArgumentList @(
        '-NoExit', '-Command',
        "`$host.ui.RawUI.WindowTitle = 'Project - client-frontend'; Set-Location '$root\client-frontend'; Write-Host 'Logging to $frontendLog - tail it if this seems stuck.'; npx ng serve *> '$frontendLog'"
    )
}

$backendCsproj = Get-ChildItem -Path "$root\client-backend" -Filter '*.Api.csproj' -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
if ($backendCsproj) {
    if (Test-PortOpen 5223) {
        Write-Host "client-backend already running on :5223 - skipping." -ForegroundColor DarkYellow
    } else {
        Write-Host "Starting client-backend dev server..." -ForegroundColor Cyan
        # Same fix as the frontend above - unredirected output to a
        # detached console can hang the process outright.
        $backendLog = "$env:TEMP\dotnet-watch.log"
        Start-Process powershell -ArgumentList @(
            '-NoExit', '-Command',
            "`$host.ui.RawUI.WindowTitle = 'Project - client-backend'; Set-Location '$($backendCsproj.DirectoryName)'; Write-Host 'Logging to $backendLog - tail it if this seems stuck.'; & '$dotnetExe' watch run --urls http://localhost:5223 *> '$backendLog'"
        )
    }
} else {
    Write-Host "client-backend not scaffolded yet - skipping." -ForegroundColor DarkYellow
}

$widgetAlreadyOpen = [bool](Get-CimInstance Win32_Process -Filter "Name='msedge.exe'" -ErrorAction SilentlyContinue |
    Where-Object { $_.CommandLine -and $_.CommandLine -like '*widget/index.html*' })

if ($widgetAlreadyOpen) {
    Write-Host "Task widget already open - skipping." -ForegroundColor DarkYellow
} else {
    Write-Host "Opening task widget..." -ForegroundColor Cyan
    $edge = "$env:ProgramFiles (x86)\Microsoft\Edge\Application\msedge.exe"
    # The repo path has a space ("PROYECTOS PERSONALES") - it must be
    # percent-encoded in the file:// URL, otherwise Windows' command-line
    # argument splitting cuts the URL off at the space and Edge opens a
    # broken/truncated path instead of the widget.
    $widgetUrl = "file:///$(($root -replace '\\','/') -replace ' ','%20')/widget/index.html"
    if (Test-Path $edge) {
        Start-Process $edge -ArgumentList "--app=$widgetUrl", '--window-size=320,480'
    } else {
        Start-Process "$root\widget\index.html"
    }
}

Write-Host ""
Write-Host "Done. Frontend: http://localhost:4200  |  Backend: http://localhost:5223/swagger" -ForegroundColor Green
Write-Host "Both auto-reload on save - just edit and save, no need to re-run this." -ForegroundColor Green
