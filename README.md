# NutriBoost (working name — see [docs/NAMING.md](docs/NAMING.md))

Fitness-nutrition e-commerce platform for Ecuador. English-first UI (also
doubling as C1-English practice). Local development only for now.

## Stack

React + TypeScript + Vite + shadcn/ui (frontend) · ASP.NET Core 8 + EF Core
+ PostgreSQL (backend, in progress) · Stripe (payments, planned).
Full rationale in [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md).

## Repo layout

```
client-frontend/   the public storefront (React) — scaffolded, has a working Navbar
client-backend/    storefront API (ASP.NET Core) — scaffolding in progress
admin-frontend/    internal ops panel (React) — not started yet
admin-backend/     internal ops API (ASP.NET Core) — not started yet
docs/              architecture, dependencies, hosting, naming, pending ideas, error log
scripts/           local dev start/stop helpers
widget/            local pending/done task tracker (opens with start-dev.ps1)
docker-compose.yml local PostgreSQL
```

## Quick start

Easiest: double-click the **"NutriBoost - Start"** shortcut on the Desktop.
It starts Postgres (Docker), the frontend dev server, the backend dev
server (once scaffolded), and the task widget. **"NutriBoost - Stop"**
shuts it all down.

Manually:

```bash
# 1. Start Docker Desktop first (installed but not auto-started).
docker compose up -d

# 2. Frontend
cd client-frontend
npm install   # first time only
npm run dev   # http://localhost:5173

# 3. Backend (once scaffolded)
cd client-backend/src/NutriBoost.Client.Api
dotnet watch run
```

## Before you read/write code here

- [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) — the stack, the folder
  layout, and *why*, including where this deviates from the original idea
  notes and why (e.g. EF Core instead of Prisma).
- [docs/PENDING_IDEAS.md](docs/PENDING_IDEAS.md) — everything that's been
  thought through but not built yet. Check it before assuming something's
  missing by accident.
- [docs/GIT_WORKFLOW.md](docs/GIT_WORKFLOW.md) — branch-per-module, and the
  "is there anything sensitive in this diff?" check before every push.
- [docs/AI_WORKFLOW.md](docs/AI_WORKFLOW.md) — how coding sessions vs.
  ideation chats are meant to split, and two corrections to the original
  3-brain plan worth reading once.
- [docs/DEPENDENCIES.md](docs/DEPENDENCIES.md) — every package/tool added,
  and why.
