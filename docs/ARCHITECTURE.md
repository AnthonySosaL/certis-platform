# Architecture

Brand name: **Certis** — see [NAMING.md](NAMING.md) (the backend
namespace, repo folder, and production subdomains still say the old
placeholder `english-c1`, deliberately deferred — see "Rough edges" in
[PENDING_IDEAS.md](PENDING_IDEAS.md)). An English placement and
practice platform (targeting Cambridge C1) for you and a friend, built
with C# .NET partly as a learning goal in itself. Possibly expanded to
institutions later — not the near-term scope. This doc is the source of
truth for *what the platform actually is and why it's built this way*;
update it whenever the feature set or a real decision changes, and log
the change in [STRUCTURE_CHANGELOG.md](STRUCTURE_CHANGELOG.md) (that
file is the chronological "what happened when" log — this one is the
current-state snapshot).

Live in production: `https://english-c1.runasp.net` (frontend),
`https://english-c1-api.runasp.net` (backend) — see [HOSTING.md](HOSTING.md).

## What the platform actually does (2026-08-30)

- **Placement test**: a fixed 64-question multiple-choice test (Grammar,
  Vocabulary, Reading, Listening - 4 CEFR levels A2-C1 each) taken in one
  sitting, self-graded. Placement = the highest level passed
  consecutively from A2, each (level, skill) cell graded 0-10
  school-style, 7/10 to pass. Results page shows a circular level badge
  and a per-area breakdown grid with "Practice this" links into weak
  areas.
- **Reinforcement quizzes**: a focused quiz for one (level, skill) cell,
  reachable from the placement results breakdown, from Courses, or
  directly by URL (`/test/reinforce/:level/:skill`). Reviews missed
  questions with explanations after submitting. Also offers
  Groq-generated replacement questions ("Practice different questions")
  for Grammar/Vocabulary/Speaking-adjacent skills.
- **Courses** (`/courses`): a free-practice hub organized like a
  Cambridge exam - Grammar/Vocabulary ("Use of English"), Reading,
  Listening, Speaking. Picking a (level, skill) opens `/courses/:level/
  :skill`, which offers three real choices: take a full AI-generated
  multi-slide course (8-11 slides mixing explanation with ungraded
  drag-and-drop or type-the-answer self-check exercises - drag for A2/
  B1, write for B2/C1), take a real graded 3-question mini-quiz to
  gauge readiness, or skip straight to the full reinforcement quiz.
- **Speaking practice** (`/speaking`): AI-only roleplay across four
  fixed scenarios modeled on the parts of a real Cambridge speaking
  exam (interview, long turn, collaborative task, abstract discussion).
  Stateless text chat with Groq playing the examiner - full history
  sent by the client each turn, nothing persisted server-side yet.
  Matching with a real practice partner is explicitly out of scope for
  now (needs its own design pass - presence, pairing, real-time state).
- **Dashboard** (`/dashboard`): full attempt history (placement +
  reinforcement, split), current level, tutor assignment if any, and
  the **Coach panel** - an on-demand Groq call that aggregates a
  student's *entire* history per (level, skill) cell into one overall
  progress diagnostic, not tied to any single attempt.
- **Admin panel** (`/admin`, Admin/Tutor roles): Students tab (roster,
  early-warning flags), Content tab (question bank CRUD, shows an "AI"
  badge on Groq-generated questions), Access tab (grant/revoke Admin/
  Tutor roles, assign a tutor per student). A seeded `ai-tutor@certis.local`
  account exists as a real, assignable Tutor persona nobody can sign
  into.
- **About page**: methodology write-up (how scoring works, what's
  in/out of scope), plus a photo section and closing CTA.
- **Real imagery**: several hand-picked Pexels photos (landing/About/
  Courses banners), downloaded once as static assets - no live Pexels
  API calls at runtime, no key shipped to the client. A scrolling tips
  ticker runs below the navbar on every page.

## AI features (all via Groq, `openai/gpt-oss-20b`, on-demand only - never automatic)

| Feature | What it generates | Endpoint |
|---|---|---|
| Reinforcement question generation | One new MCQ for a (level, skill), structured JSON output | `POST /api/test/reinforcement/{level}/{skill}/generate` |
| Course generation | 8-11 slides (content + drag/write exercises) for a (level, skill) | `POST /api/test/reinforcement/{level}/{skill}/course` |
| Per-attempt insight | (backend method still exists; no longer surfaced in the UI - superseded by the Coach panel) | — |
| Coach panel (overall insight) | A progress diagnostic aggregated across a student's full history | `POST /api/test/insight/overall` |
| Speaking roleplay | A single in-character reply as the Cambridge examiner/partner | `POST /api/speaking/{scenarioId}/reply` |

All use `reasoning_effort: "low"` (gpt-oss-20b is a reasoning model -
without this, most of the token budget goes to hidden chain-of-thought
instead of the actual answer) and return `null`/503 when unavailable
(no `Groq:ApiKey` configured, or the upstream call failed) rather than
a generic 500.

## Stack decision

| Layer | Choice | Why |
|---|---|---|
| Backend | **ASP.NET Core 8 (C#) Web API** | Explicitly requested — partly to learn C#/.NET, which is widely asked for in job postings. LTS version. Live at `https://english-c1-api.runasp.net` — see [HOSTING.md](HOSTING.md). |
| Frontend | **Angular 22 + TypeScript** | Switched from React on 2026-08-26 — deliberate choice, not a mistake: the user already has "a ton" of React projects and wants portfolio breadth. |
| Component library | **Angular Material** (Material 3 theming via `mat.theme()`) | Official, best-integrated Angular UI kit. Custom azure+orange palette (not the generic starter green/blue), plus light/dark mode via the `Theme` service. |
| ORM | **EF Core** | Migrations live in source control, applied directly against the shared dev+prod database (no separate local DB). |
| Database | **SQL Server** (MonsterASP.NET, free tier) | Switched from PostgreSQL on 2026-08-26 — see [HOSTING.md](HOSTING.md). |
| Icons | **Inline SVG**, hand-written | Avoids an icon-library version conflict and an icon web font. |
| Auth | **ASP.NET Core Identity + JWT bearer tokens** | Email/password register + login. JWT (not cookies) because Angular is a separate SPA calling the API cross-origin. Roles: `Admin`, `Tutor`. |
| Images | **Pexels** (downloaded once, static) | A real Pexels API key the user already had in a sibling project, reused with explicit authorization - not a live runtime dependency. |
| Local text-to-speech | **edge-tts** (Python, already installed locally) | Generates the Listening question bank's audio once, offline from the app - not a runtime dependency either. Picked over Groq's TTS (blocked on terms acceptance) and ElevenLabs (not installed). |

## Repo layout

```
english-c1-platform/   (folder name — placeholder, see NAMING.md)
├── client-frontend/    Angular 22 + Material
├── client-backend/     ASP.NET Core Web API, Clean Architecture, live on MonsterASP.NET
├── docs/                this folder
└── scripts/             local dev start/stop helpers, scripts/generate_listening_audio.py
```

A single app for now (not split into separate "client" and "admin"
apps) - the admin panel is a route (`/admin`) inside the same Angular
app, guarded client-side and enforced server-side by role.

## Backend: layered / Clean Architecture

`client-backend` is split into separate projects in one `.sln` so
dependencies are enforced by the compiler, not just convention:

- **Domain** — entities and value objects, no dependencies on anything
  else. Key entities: `Question`/`QuestionOption` (with `IsAiGenerated`,
  `Passage`, `AudioUrl`), `TestAttempt`/`TestAnswer`, `ApplicationUser`
  (self-referencing `TutorId`), `CefrLevel`/`SkillArea`/`AttemptKind`
  enums.
- **Application** — use cases and interfaces for infrastructure
  (`ITestService`, `IContentService`, `IAdminService`, the `IAi*Service`
  family, `ISpeakingService`). Depends only on Domain.
- **Infrastructure** — EF Core `AppDbContext` (an `IdentityDbContext`),
  service implementations, Groq HTTP clients, `QuestionSeeder` (the
  hand-written question bank + `ReadingBank`/`ListeningBank`, idempotent
  match-by-text seeding on every startup).
- **Api** — controllers, DI wiring (`DependencyInjection.cs`), CORS, JWT
  bearer auth, `UseStaticFiles()` (serves the Listening audio bank). The
  only project that knows about HTTP.

Namespaces/project names are still `EnglishC1.Client.*` — a placeholder
tied to the original folder name, deliberately not renamed to match the
`Certis` brand yet (see [PENDING_IDEAS.md](PENDING_IDEAS.md) - renaming
would mean re-provisioning the live subdomains' HTTPS certs).

## Frontend structure

```
src/app/
├── layout/          Navbar (nav items vary by auth/role), Footer, TipsTicker
├── pages/           Route-level standalone components: home, about, test/
│                    (placement-test, test-result, reinforcement), course,
│                    courses, dashboard, admin, speaking, auth, forgot-password
├── core/             Cross-cutting services: Theme, Auth (JWT in localStorage,
│                     signals), test-api.ts/admin-api.ts/speaking-api.ts (thin
│                     HTTP wrappers), cefr.ts, skill-icons.ts, grading.ts,
│                     auth.guard.ts/admin.guard.ts, api-config.ts
├── shared/           Quiz (the reusable MCQ-taking component, used by both
│                     the placement test and every reinforcement quiz)
├── app.routes.ts    Route table
└── app.ts/.html     App shell (Navbar + TipsTicker + <router-outlet /> + Footer)
```

Standalone components throughout (no NgModules), signals for reactive
state. Light mode is the default; the full Material 3 dark theme is
built and toggle-able (`Theme` service, `html.dark`).

## Not built yet / deliberately deferred

See [PENDING_IDEAS.md](PENDING_IDEAS.md) for the full, current list -
as of this writing: password reset/email confirmation (needs an email
sender), Google OAuth, i18n, real-user speaking matching (needs its own
design pass), a course catalog beyond what Courses already covers, CI
(no GitHub remote configured), and the technical rename to `Certis`
(namespace/folder/subdomains).

## Design patterns in play

- **Signals** for reactive UI state throughout, Angular's current
  recommended default over RxJS `BehaviorSubject` for simple state.
- **Standalone components** — no NgModules, direct `imports: []` per
  component.
- **On-demand AI, never automatic** — every Groq call is triggered by an
  explicit user action (a button), with a clear "not available right
  now" state distinct from a generic error.
- **Hand-written content by default, AI as an addition** — the core
  question bank, Reading passages, and Listening transcripts are
  hand-authored and seeded; AI generation (questions, courses, insight)
  supplements it rather than replacing it.
