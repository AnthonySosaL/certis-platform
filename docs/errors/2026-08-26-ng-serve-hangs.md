# ng serve hangs when spawned from start-dev.ps1

**Date:** 2026-08-26
**Area:** client-frontend (Angular), scripts/start-dev.ps1

## Symptom

The frontend dev server would sometimes not come up at all after
`scripts/start.bat` — port 4200 never bound, `curl`/the browser got
connection-refused indefinitely. Backend and everything else started
fine every time; only the frontend was affected. It happened both after
a normal `stop-dev.ps1` and, once, after the frontend window apparently
closed/died on its own mid-session with no `stop-dev.ps1` run at all.

## Root cause

Two separate issues, found by comparing a working manual `ng serve`
invocation against the one `start-dev.ps1` actually used:

1. **Corrupted `.angular/cache`.** Once the `ng serve` process is
   terminated abruptly (window closed, `taskkill`, a crash) instead of
   exiting cleanly, its cache directory (`client-frontend/.angular/cache`)
   is left in a state that makes the *next* `ng serve` hang forever
   during the initial build — no error, no output, no port. Deleting that
   directory and retrying always fixed it.
2. **Hangs when output isn't redirected**, independent of the cache
   issue. `Start-Process powershell -ArgumentList @('-NoExit', '-Command',
   "...; npx ng serve")` (output going straight to the spawned console)
   hung even with a freshly-cleared cache. The exact same command with
   `npx ng serve *> logfile.txt` appended (output redirected to a file)
   worked reliably every time it was tried — confirmed repeatedly, not a
   one-off. Root mechanism not fully understood (plausibly something in
   Angular CLI's/Vite's TTY/interactive-console detection behaving
   differently in a `Start-Process`-spawned console versus a redirected
   stream) but the workaround is solid.

## Fix

`scripts/start-dev.ps1` now, every time it starts the frontend:
1. Deletes `client-frontend/.angular/cache` first.
2. Redirects `ng serve`'s output to `%TEMP%\ng-serve.log` instead of the
   spawned console.

Verified with a clean stop -> start round-trip, twice in a row, both
times frontend + backend up within seconds.

## How to avoid it again

If the frontend window ever looks stuck after `Start-Project`, check
`%TEMP%\ng-serve.log` first — it has the real build output that the
window itself no longer shows directly (traded away for reliability).
If a future change to `start-dev.ps1` removes the output redirection
"to see live logs again," expect this hang to come back — re-add it
rather than assuming it was unrelated.
