# Hosting research (2026-08-26)

Everything runs **locally** for now (Docker Compose — see root README). This
doc is prep for when we go to production, per the notes' request to decide
this early so the architecture doesn't fight the host later.

## Frontend: Cloudflare Pages over Vercel

| | Vercel (Hobby/free) | Cloudflare Pages (free) |
|---|---|---|
| Bandwidth | 100 GB/month cap, site pauses if exceeded | Unlimited |
| **Commercial use** | **Prohibited by ToS on the free tier** | **Allowed** |
| Builds | Generous | 500/month |

**Decision: Cloudflare Pages.** The commercial-use restriction is
disqualifying on its own — this is a real Ecuador storefront, not a demo, so
Vercel's Hobby tier's ToS violation risk isn't worth it. Cloudflare Pages
also builds and deploys any static Vite output (React here) with no
framework lock-in.

## Backend: MonsterASP.NET over Render / Cloudflare Workers

- **Cloudflare Workers is ruled out** for the C# backend — it's a JS/TS edge
  runtime (V8 isolates), it does not run ASP.NET Core. It stays usable for
  small edge helpers later (e.g. image resizing) but not the API.
- **Render free tier**: works for any Docker/container backend including
  ASP.NET Core, but the free web service spins down after 15 minutes of
  inactivity (30-60s cold start on the next request) and free Postgres
  expires after 30 days. Fine for staging, rough for a real storefront.
- **MonsterASP.NET** (the host named in the brief): built specifically for
  ASP.NET Core / .NET, free tier includes one-click deploy from Visual
  Studio, free MSSQL, free Let's Encrypt HTTPS, no credit card required.
  Paid "Premium" tiers (billed annually, 14-day money-back guarantee) let
  you pick a US or EU region.

**Leaning: MonsterASP.NET for the backend once we leave local dev** — it's
purpose-built for exactly this stack, which avoids fighting a generic
container host for .NET-specific concerns (IIS/Kestrel config, MSSQL vs
Postgres). Two things to confirm before committing, ideally with a Fable 5
pass since it touches money/compliance:
1. Whether its DB offering (MSSQL) or bringing external Postgres (e.g. a
   managed free/cheap Postgres elsewhere) is the better fit — EF Core
   supports both, but the [ARCHITECTURE.md](ARCHITECTURE.md) default is
   Postgres; switching to MSSQL is a provider-string change in EF Core, not
   a rewrite, so it's not a blocking decision.
2. Whether the free tier's limits (uptime, request volume) hold up once
   real customer traffic starts, versus paying for Premium from day one of
   going live.

## Admin app

Same hosts, deployed as separate Cloudflare Pages + MonsterASP.NET
projects/sites from the client app — keeps the "fully isolated from the
client" requirement true in production, not just in the repo folder layout.

## Local development (today)

Docker Compose runs Postgres locally (see `docker-compose.yml` at the repo
root). No cloud accounts needed yet — `docs/HOSTING.md` gets revisited once
we're ready to actually deploy.
