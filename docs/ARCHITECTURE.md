# Architecture

Working name: **NutriBoost** (placeholder — see [NAMING.md](NAMING.md)).
An English-language fitness-nutrition e-commerce platform for Ecuador, built
as a personal project doubling as English/C1 practice. This doc is the
single source of truth for *why* things are built the way they are; update
it whenever a decision here changes, and log the change in
[STRUCTURE_CHANGELOG.md](STRUCTURE_CHANGELOG.md).

## Stack decision (and why it differs from the original notes)

| Layer | Choice | Notes |
|---|---|---|
| Backend | **ASP.NET Core 8 (C#) Web API** | Explicitly requested — ".NET porque he visto que mucho usan eso." LTS version. |
| Frontend | **React 18 + TypeScript + Vite** | The notes name Go/Django as options but fall back to React if they don't fit the free-hosting story — they don't (see [HOSTING.md](HOSTING.md)), and shadcn/ui (mandated in the notes) is React-only. So React it is. |
| Component library | **shadcn/ui** (Radix primitives, Tailwind CSS v4, "Nova" preset — Lucide icons, Geist font) | As specified in the source notes. |
| ORM | **EF Core** (not Prisma) | Prisma is a JS/TS-only ORM; it cannot target a C# backend. EF Core is the direct .NET equivalent — migrations, schema tracked in source control, same "controlled changes" goal the notes wanted from Prisma. |
| Database | **PostgreSQL** | Free-tier friendly, works cleanly with EF Core, and is what most of the free/cheap hosts (Render, Supabase, Neon) support well. |
| Animation | **Motion** (the successor to Framer Motion) | Matches the "skill motion" note. |
| Payments | **Stripe** | As specified. |
| Auth | **ASP.NET Core Identity + Google OAuth + JWT** | See "Auth" below. |

## Repo layout (monorepo)

A single git repo with isolated app folders, not four separate repos. The
notes ask for admin and client to be *fully isolated* (own frontend, own
backend) — that isolation is at the **app/deployment** level, not the repo
level. A monorepo keeps the point-6 "one branch per module, merge to main
after review" workflow simple (one PR, one repo, one history) while the
folders below still deploy independently:

```
NutriBoost/
├── client-frontend/   React + TS + Vite + shadcn — the public storefront
├── client-backend/    ASP.NET Core Web API — storefront API (planned)
├── admin-frontend/    React + TS + Vite + shadcn — internal ops panel (planned)
├── admin-backend/     ASP.NET Core Web API — internal ops API (planned)
├── docs/              this folder
└── scripts/           local dev start/stop helpers
```

`client-*` is scaffolded first because it's the customer-facing MVP asked
for this session. `admin-*` folders get scaffolded next session — tracked in
[PENDING_IDEAS.md](PENDING_IDEAS.md) so it isn't lost.

## Backend: layered / Clean Architecture

Each backend (`client-backend`, later `admin-backend`) follows the same
layering, as separate projects in one .sln so dependencies are enforced by
the compiler, not just convention:

- **Domain** — entities, value objects, domain logic. No dependencies on
  anything else.
- **Application** — use cases (CQRS-style commands/queries), interfaces for
  infrastructure (`IProductRepository`, `IPaymentGateway`, etc.). Depends
  only on Domain.
- **Infrastructure** — EF Core `DbContext`, repository implementations,
  Stripe client, email sender, Google OAuth integration. Implements
  Application's interfaces.
- **Api** — controllers/minimal-API endpoints, DI wiring, middleware, auth
  configuration. The only project that knows about HTTP.

Rationale: keeps business rules (stock concurrency, invoice rules, refund
rules) testable without spinning up a database or HTTP server, and keeps
"which layer am I editing" explicit — directly matches the source notes'
"cada modificación... piensa qué patrones se va usar" requirement and the
dev-engineering-rules skill's "layer protection" rule now active in this
project's coding sessions.

## Frontend structure (per app)

```
src/
├── app/            App shell: routing, providers (theme, etc.)
├── components/
│   ├── ui/         shadcn primitives (generated — don't hand-edit heavily)
│   └── layout/     Navbar, Footer, and other structural, reused components
├── features/       Feature-based modules as they're built (catalog, cart, auth...)
├── i18n/           Translation resources (English active, Spanish scaffolded)
├── lib/            Shared utilities (shadcn's cn() helper, etc.)
└── pages/          Route-level components
```

Component rule from the notes: build reusable components from the start
(Navbar, Footer, buttons, cards already are), prefer **cards over extra
tabs/pages** where content allows it, no emoji/text-icons — Lucide SVG icons
only, sized responsively.

## Cross-cutting concerns staged for later (not built yet)

These are architecturally accounted for (folder/config exists) but not
implemented — see [PENDING_IDEAS.md](PENDING_IDEAS.md) for the full list
with reasoning per item:

- **Theming**: light mode is the default and only active mode, but the full
  dark-mode CSS variable set already exists (`.dark` class, toggled via
  `ThemeProvider`) — flipping the default later is a one-line change, not a
  redesign.
- **i18n**: English is the only language wired into the UI, but the
  `i18next` setup and a Spanish resource file already exist side by side —
  adding a language switcher later doesn't require a translation pass done
  under deadline pressure.
- **Auth**: Google OAuth + email/password with password recovery, per the
  notes. Deferred until `client-backend` exists.
- **Stock concurrency**: prevent overselling when two buyers race for the
  last unit. Planned as a DB-level optimistic concurrency token (EF Core
  `[Timestamp]` / `rowversion`) plus a transaction that re-checks stock at
  commit time — needs a Fable 5 audit pass before it's implemented, per the
  notes' explicit ask.
- **Invoicing**: soft-delete only, full edit history, Ecuador-specific legal
  requirements — needs research + a Fable 5 audit pass before implementation
  (see AI_WORKFLOW.md).
- **Stripe dev/prod separation**: two API key sets, never committed — see
  the root `.gitignore` and the "before every push" reminder in the README.

## Design patterns in play so far

- **Provider pattern** for cross-cutting UI state (`ThemeProvider`).
- **Composition over configuration** for shadcn components (each is owned
  source code in `components/ui`, not an opaque npm dependency).
- **Repository pattern** (planned) for the backend's Infrastructure layer,
  so the Application layer never talks to EF Core directly.
- **CQRS-lite** (planned): commands and queries as distinct types in the
  Application layer, even without a full mediator library at first.
