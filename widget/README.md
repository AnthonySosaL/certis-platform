# Task widget

A minimal pending/done task tracker, opened as its own small browser window
alongside the dev servers by `scripts/start-dev.ps1`. Minimize it with the
`—` button in its header (state persists via `localStorage`, per-browser-
profile — it does not sync anywhere).

## Why a local HTML page instead of a native tray app

The notes describe "an external widget that opens with the app, can be
minimized." A native always-on-top tray widget (Electron, WinUI, etc.) is a
separate app with its own build/update pipeline — real scope on its own.
This first pass gets the actual behavior (a small window, pending/done
lists, minimizable, opens with the dev environment) for near-zero build
cost. If it turns out to not be enough — needs to survive closing the
browser, needs to sit above other windows, needs to sync across
machines — that's a deliberate upgrade to track as its own item in
[../docs/PENDING_IDEAS.md](../docs/PENDING_IDEAS.md), not a silent scope
change.
