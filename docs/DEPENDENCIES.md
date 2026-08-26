# Dependencies & tooling log

Every tool, package, or skill added to this project gets an entry here —
what it is, why it was chosen, and when. Removing something? Move its row
to the "Removed" section at the bottom instead of deleting it, so the
history stays visible (per the notes' traceability requirement).

## System tooling

| Tool | Version | Why | Added |
|---|---|---|---|
| .NET SDK | 8.0.424 (LTS) | Backend runtime + CLI (`dotnet new`, EF Core migrations). Only the runtime was preinstalled. First tried `winget install Microsoft.DotNet.SDK.8`, but that install requires admin elevation and hung indefinitely waiting on a UAC prompt with no interactive desktop to show it to — killed it and installed to the user profile instead (`dotnet-install.ps1 -InstallDir %USERPROFILE%\.dotnet`, no admin needed). `PATH` and `DOTNET_ROOT` (User scope) point there now. If `dotnet --list-sdks` ever stops showing 8.0.424, check those two env vars first. | 2026-08-26 |
| Node.js | v22.22.0 (already installed) | Frontend tooling (npm, Vite). | pre-existing |
| Docker Desktop | 29.4.3 (already installed, not yet running) | Local Postgres via `docker-compose.yml`. Start Docker Desktop before `docker compose up`. | pre-existing, wired in 2026-08-26 |
| git | 2.53.0 (already installed) | Version control. | pre-existing |

## `client-frontend` (React + Vite)

| Package | Why |
|---|---|
| `react`, `react-dom` | UI framework — scaffolded by `create-vite`. |
| `typescript` | Type safety. |
| `vite`, `@vitejs/plugin-react` | Dev server / bundler. |
| `tailwindcss` v4, `@tailwindcss/vite` | Utility CSS, required by shadcn/ui. |
| `shadcn` CLI (`radix-ui` base, "Nova" preset) | Component library mandated by the source notes. Generates owned source in `src/components/ui`, not an opaque dependency. |
| `lucide-react` | Icon set (SVG, no emoji/text-icons per the notes). Pulled in automatically by the shadcn "Nova" preset. |
| `@fontsource-variable/geist` | Self-hosted variable font, no external font CDN request. |
| `react-router-dom` | Client-side routing. |
| `motion` | Animation (the renamed/current Framer Motion package) — the "motion skill" the notes asked for. |
| `i18next`, `react-i18next`, `i18next-browser-languagedetector` | i18n scaffold — English active now, Spanish resource file present but not exposed, per the notes' "make it easy to add languages later" request. |
| `@types/node` (dev) | Needed for `import.meta.dirname` path aliasing in `vite.config.ts`. |

## `client-backend` (ASP.NET Core, layered: Domain/Application/Infrastructure/Api)

| Package | Project | Why |
|---|---|---|
| `Microsoft.EntityFrameworkCore` 8.0.11 | Infrastructure | ORM — pinned to the 8.x line to match the net8.0 target (`dotnet add` defaults to the newest major, which was 10.x and incompatible). |
| `Npgsql.EntityFrameworkCore.PostgreSQL` 8.0.11 | Infrastructure | PostgreSQL provider for EF Core. |
| `Microsoft.EntityFrameworkCore.Design` 8.0.11 | Api | Enables `dotnet ef migrations` from the Api project. |
| `Swashbuckle.AspNetCore` | Api | Swagger/OpenAPI UI — scaffolded by default with `dotnet new webapi`, kept for local API exploration. |

Concurrency note: `Product.RowVersion` was tried first as a `byte[]`
mapped with `.IsRowVersion()` (the SQL Server pattern) — Npgsql doesn't
generate that automatically. Landed on Npgsql's actual idiom: a shadow
`xmin` property (`entity.Property<uint>("xmin").IsRowVersion()`), no
mapped CLR property needed. `UseXminAsConcurrencyToken()` also exists but
is obsolete as of this Npgsql version.

## Claude Code skills relied on for this project

Not installed via a package manager — these are skills already available to
Claude Code sessions in this environment. Listed here so it's clear which
ones this project leans on and why, per the notes' request to track "skills
used" like any other tool:

| Skill | Why |
|---|---|
| `anthropic-skills:dev-engineering-rules` | Protect-existing-logic / one-layer-per-change / reuse-before-creating / 200-line-file-cap rules — applied to every coding session on this repo. |
| `engineering:code-review` | Use before merging any feature branch to `main`. |
| `engineering:architecture` | Use when a new architecture decision record is needed (e.g. finalizing the concurrency or invoicing design). |
| `engineering:testing-strategy` | Use once the backend has enough surface area to need a real test plan. |
| `engineering:documentation` | Use for README/runbook writing beyond what's already in `docs/`. |

## Removed

_(nothing removed yet)_
