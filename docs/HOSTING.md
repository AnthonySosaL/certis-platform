# Hosting

## Database: MonsterASP.NET, SQL Server 2025 (decided 2026-08-26)

Live and in use, not just researched. Created on the FREE plan
(`db65520.databaseasp.net`, EU/Germany datacenter — Free plan only offers
EU). EF Core connects to it directly, including from local dev — see
"Local development" below.

Why SQL Server over MySQL (the other free option): this is a C#/.NET
project specifically to build .NET skills for the job market, and SQL
Server is what pairs with .NET/enterprise roles in practice, plus it has
first-party EF Core support. MySQL is more broadly used across the *whole*
market (open source, PHP/Node/web-dev ubiquity), but that's not this
project's stack.

Free-plan limits worth remembering (from MonsterASP's own plan page): low
performance servers, 1GB disk max, limited traffic/features, EU
datacenters only, "not recommended for production" — fine for building and
learning, revisit before anything with real users depends on it.

Two connection modes MonsterASP exposes:
- **Local access** — only reachable from apps hosted on MonsterASP itself
  (their internal network). Not usable from a home dev machine.
- **Remote access (for SSMS)** — public internet access, what local dev
  and any external CI need. Enabled for this database. Connection string
  lives in `dotnet user-secrets` on the dev machine, never in a committed
  file — see `docs/GIT_WORKFLOW.md`.

## Backend hosting: not decided yet

The database is live; the API isn't deployed anywhere yet (still runs
locally, pointed at the remote DB). MonsterASP.NET is the natural next
step when that's needed (same free plan, .NET-specific, one-click deploy
from Visual Studio or a downloadable publish profile) — revisit then.

## Frontend hosting: not decided yet, low stakes either way

Cloudflare Pages is still the better default if/when this needs a public
URL — Vercel's Hobby tier prohibits commercial use in its ToS, which
matters if this ever gets sold to institutions and doesn't matter at all
for a private 2-person tool today. Not an active decision right now.

## Local development

The app talks to the real MonsterASP database directly — no local
database server needed day to day. `docker-compose.yml` (Postgres, port
5433) is kept only as an **offline fallback**; it does not start
automatically (`scripts/start-dev.ps1` no longer launches it). If you ever
need to work without internet access, `docker compose up -d` it yourself
and swap the connection string — but note the app currently targets SQL
Server (`UseSqlServer`), so a Postgres fallback would need the Npgsql
provider back too; not wired up right now since it wasn't needed once the
real database was live.
