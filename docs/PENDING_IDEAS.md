# Pending ideas / not forgotten, just not built yet

A running list so nothing gets lost while other things get built first.
Move an item to "Done" (with a date and a link to the changelog entry)
instead of deleting it.

## Feature scope — not yet decided

This is the biggest open item: what does "practice and evaluate English
toward C1" actually consist of, concretely? Nothing below can be built
correctly (domain model, database schema, UI) until this is answered, at
least at a first-pass level:

- [ ] Which skills does the platform cover first — reading, writing,
      listening, speaking, vocabulary/grammar drills, full mock C1 exams,
      some combination?
- [ ] How is content authored — written/curated by hand and stored in the
      database, generated on demand (e.g. via an AI model), or both?
- [ ] What does "evaluate" mean concretely — self-scored exercises,
      AI-graded writing/speaking, a mock exam with a C1-equivalent score,
      progress tracking over time comparing you and your friend?
- [ ] Who can use it right now — just the two of you (simple private
      login is enough), or does it need to support other users from day
      one even before the "sell to institutions" idea is pursued?

## Blocked on you / needs a decision

- [ ] **Final project name.** Three tentative candidates in
      [NAMING.md](NAMING.md) — pick one, reject all three, or hold off
      until the feature scope above is clearer (recommended, since a
      C1-specific name is a poor fit if scope grows beyond C1 later).
## Scaffolded structurally, not implemented

- [ ] `client-backend` — layered scaffold exists, builds, and is verified
      end-to-end against the real MonsterASP.NET SQL Server database
      (see HOSTING.md), but there's no domain model (`AppDbContext` has
      zero `DbSet`s) and no auth yet.
- [ ] Spanish translations for the app's own UI chrome (nav labels etc.)
      — resources exist in `client-frontend/src/i18n/locales/es`, not
      exposed via a switcher. Low priority: the platform's primary
      language is English by design.
- [ ] Dark mode toggle — fully wired in `ThemeProvider`, light stays the
      default until you say otherwise.

## Nice-to-haves mentioned along the way, not urgent

- [ ] Admin/teacher-facing panel, only if this grows beyond a 2-person
      tool (institutions idea) — not needed for the current scope.
- [ ] Toasts for lightweight confirmations, modals reserved for anything
      more consequential (e.g. submitting a graded exercise).
- [ ] Carousels / scroll-triggered animations on the landing page, once
      there's real content to showcase.

## Done

- [x] **Database hosting.** Created on MonsterASP.NET's free plan (SQL
      Server 2025, EU datacenter). Backend switched from Npgsql/Postgres
      to `Microsoft.EntityFrameworkCore.SqlServer`, connected and verified
      with a real migration against the live database. Local Docker
      Postgres demoted to an offline-only fallback. See
      [STRUCTURE_CHANGELOG.md](STRUCTURE_CHANGELOG.md#2026-08-26--real-database-monsteraspnet-sql-server-dropped-local-postgres)
      and [HOSTING.md](HOSTING.md). — 2026-08-26
- [x] **C# namespace rename.** `NutriBoost.Client.*` → `EnglishC1.Client.*`
      across every `.sln`/`.csproj`/`namespace` — folders, solution,
      migrations, all updated in one pass. Still a placeholder (not the
      final brand name — see [NAMING.md](NAMING.md)), but no longer the
      wrong-project holdover. — 2026-08-26
