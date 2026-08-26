# Dependencies & tooling log

Every tool, package, or skill added to this project gets an entry here —
what it is, why it was chosen, and when. Removing something? Move its row
to the "Removed" section at the bottom instead of deleting it, so the
history stays visible (per the notes' traceability requirement).

## System tooling

| Tool | Version | Why | Added |
|---|---|---|---|
| .NET SDK | 8 (LTS) | Backend runtime + CLI (`dotnet new`, EF Core migrations). Only the runtime was preinstalled on this machine; the SDK was missing and got installed via `winget`. | 2026-08-26 |
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

## `client-backend` (ASP.NET Core) — added once scaffolded

Tracked here as soon as the backend project is created; see
[STRUCTURE_CHANGELOG.md](STRUCTURE_CHANGELOG.md) for the entry announcing
it.

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
