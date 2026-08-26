# Stops the local dev environment started by start-dev.ps1: kills whatever
# is listening on the frontend/backend dev ports, walking up to the
# wrapper PowerShell window that spawned it (if any) so the visible
# window actually closes too, not just its child process - and stops
# the task widget and Postgres.
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
        # Walk up the parent chain (through any intermediate cmd.exe/npm
        # wrapper) looking for the specific "Project - ..." PowerShell
        # window that start-dev.ps1 spawned, identified by its command
        # line (not just process name - npm's actual parent is cmd.exe,
        # not powershell.exe directly). Kill from there so the visible
        # window closes too, not just the process running inside it.
        # Give up after a few hops rather than climbing indefinitely, in
        # case the process wasn't started by start-dev.ps1 at all.
        $targetPid = $ownerPid
        $current = Get-CimInstance Win32_Process -Filter "ProcessId=$ownerPid" -ErrorAction SilentlyContinue
        for ($hop = 0; $hop -lt 6 -and $current -and $current.ParentProcessId; $hop++) {
            $parent = Get-CimInstance Win32_Process -Filter "ProcessId=$($current.ParentProcessId)" -ErrorAction SilentlyContinue
            if (-not $parent) { break }
            if ($parent.CommandLine -and $parent.CommandLine -like "*WindowTitle = 'Project -*") {
                $targetPid = $parent.ProcessId
                break
            }
            $current = $parent
        }
        Write-Host "Stopping $label (:$port, pid $targetPid)..." -ForegroundColor Cyan
        taskkill /F /T /PID $targetPid | Out-Null
    }
}

Stop-Port 5173 "client-frontend"
Stop-Port 5223 "client-backend"

$widgetProcs = Get-CimInstance Win32_Process -Filter "Name='msedge.exe'" -ErrorAction SilentlyContinue |
    Where-Object { $_.CommandLine -and $_.CommandLine -like '*widget/index.html*' }
if ($widgetProcs) {
    $rootWidgetPid = ($widgetProcs | Select-Object -First 1).ProcessId
    Write-Host "Closing task widget (pid $rootWidgetPid)..." -ForegroundColor Cyan
    taskkill /F /T /PID $rootWidgetPid | Out-Null
}

Write-Host "Stopping Postgres (Docker)..." -ForegroundColor Cyan
try {
    docker compose -f "$root\docker-compose.yml" stop
} catch {
    Write-Warning "Could not stop Docker containers - Docker Desktop may not be running."
}

Write-Host "Done." -ForegroundColor Green
