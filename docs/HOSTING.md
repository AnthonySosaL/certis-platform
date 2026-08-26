# Hosting (deferred)

Everything runs **locally** for now. Hosting research here was originally
written for a different project (see
[errors/2026-08-26-scope-mixup.md](errors/2026-08-26-scope-mixup.md)) under
the assumption of a real commercial storefront with paying customers — that
urgency doesn't apply to a tool for two people practicing English. No
hosting decision is needed yet; this section is kept short and revisited
once (if) this platform needs to actually go live for you and your friend,
or later for institutions.

## Facts worth keeping (not a decision)

- **Cloudflare Workers cannot run ASP.NET Core** — it's a JS/TS edge
  runtime (V8 isolates). Rule it out for the C# backend specifically if
  hosting ever comes up; Cloudflare Pages (static hosting) is unrelated to
  this and still fine for the React frontend.
- **Vercel's free (Hobby) tier prohibits commercial/revenue use** in its
  ToS. Irrelevant for a private 2-person tool; relevant again if this ever
  gets sold to institutions.
- **MonsterASP.NET** exists as a .NET-specific free/cheap host (one-click
  deploy from Visual Studio, free MSSQL, free HTTPS) if a C# backend needs
  a home later.
- **Render's free tier** spins down after 15 minutes of inactivity and its
  free Postgres expires after 30 days — fine for occasional personal use,
  not for something that needs to always be up.

## Local development (today)

Docker Compose runs Postgres locally (see `docker-compose.yml` at the repo
root, port 5433 — see
[errors/2026-08-26-postgres-port-conflict.md](errors/2026-08-26-postgres-port-conflict.md)
for why not 5432).
