# Pending ideas / not forgotten, just not built yet

A running list so nothing gets lost while other things get built first.
Move an item to "Done" (with a date and a link to the changelog entry)
instead of deleting it.

## Feature scope — placement test shipped, what's next

The placement test (see "Done" below) answered the questions that used
to sit here as open decisions:

- **Format**: fixed-form, not adaptive - 32 multiple-choice questions, one
  sitting. Self-gradable, no AI needed, far simpler to get right than
  adaptive branching for a v1.
- **Scoring**: a fixed 60% pass threshold per (level, skill) cell;
  placement = highest level passed consecutively from A2.
- **Content authoring**: hand-written, stored in the database, seeded at
  startup (`QuestionSeeder`) - not AI-generated.

Still open, now that the first slice exists:

- [ ] Beyond the placement test: which skills come next (reading,
      writing, listening, speaking - only Grammar/Vocabulary exist so
      far)?
- [ ] Should a reinforcement attempt that's passed update anything about
      the original placement, or stay purely a practice log forever? No
      auto-update exists yet - retaking the full placement test is
      currently the only way to change your recorded level.
- [ ] Bigger question bank - 4 questions per (level, skill) cell is thin
      for anything beyond a first estimate (already flagged honestly on
      the About page).

## Blocked on you / needs a decision

- [ ] **Final project name.** Three tentative candidates in
      [NAMING.md](NAMING.md) — pick one, reject all three, or hold off
      until the feature scope above is clearer (recommended, since a
      C1-specific name is a poor fit if scope grows beyond C1 later).

## Scaffolded structurally, not implemented

- [ ] **Password reset + email confirmation.** Needs an email
      sender (SendGrid free tier, or similar) before it can work at all —
      `AddDefaultTokenProviders()` isn't even called yet. Not urgent for
      2 users who won't forget their own passwords immediately, but real
      before anyone else ever gets an account.
- [ ] **Google OAuth.** Nice-to-have from the original notes; email/password
      shipped first since it needed no external app registration.
- [ ] "How it works" / methodology content on the About page — currently
      just a placeholder ("Coming soon").
- [ ] i18n (Spanish for the app's own UI chrome) — not set up in the
      Angular rebuild yet; low priority, the platform's primary language
      is English by design. See ARCHITECTURE.md for the library options
      considered (`@angular/localize` vs `ngx-translate`).
- [ ] Dark mode toggle — fully wired in the `Theme` service, light stays
      the default until you say otherwise.

## Rough edges worth revisiting

- [ ] **Deploy loses production secrets on every redeploy.**
      `dotnet publish` regenerates `web.config` from scratch each time,
      which wipes any manually-added environment variables —
      `ConnectionStrings__AppDb` and `Jwt__SigningKey` (see HOSTING.md;
      both are live and verified as of 2026-08-27). Still fragile going
      forward — a redeploy that forgets this step silently breaks the
      live site (500 for the DB, 500 for any auth endpoint if the
      signing key's missing). Worth finding whether MonsterASP has a
      persistent env-var/app-settings panel outside `web.config`, or
      scripting the web.config edit so it's one command instead of a
      manual step, before this trips someone up. Every future deploy
      needs to re-add *both* variables, not just one — and should always
      start from a clean `bin`/`obj`/`publish-output` (see
      [errors/2026-08-27-stale-publish-output-dll-mismatch.md](errors/2026-08-27-stale-publish-output-dll-mismatch.md)).
- [ ] No CI — every deploy so far has been a manual `dotnet publish` +
      `scp` from a local machine. Fine for now (single developer, low
      frequency), revisit if that changes.

## Nice-to-haves mentioned along the way, not urgent

- [ ] Admin/teacher-facing panel, only if this grows beyond a 2-person
      tool (institutions idea) — not needed for the current scope.
- [ ] Toasts for lightweight confirmations, modals reserved for anything
      more consequential (e.g. submitting a graded exercise).
- [ ] Carousels / scroll-triggered animations on the landing page, once
      there's real content to showcase.

## Done

- [x] **Frontend deployed live.** `https://english-c1.runasp.net`, a
      second free site on the existing MonsterASP account - the whole
      platform is now publicly reachable, not just the API. See
      [STRUCTURE_CHANGELOG.md](STRUCTURE_CHANGELOG.md#2026-08-27--frontend-live-in-production-too-english-c1runaspnet)
      and [HOSTING.md](HOSTING.md). — 2026-08-27
- [x] **Placement test, end to end.** Backend (domain model, scoring
      algorithm with 6 unit tests, API) + Angular UI (test-taking,
      results with a color-coded CEFR badge, per-area breakdown,
      targeted reinforcement quizzes) + a design pass replacing the
      generic starter theme. Verified in-browser and via the API. See
      [STRUCTURE_CHANGELOG.md](STRUCTURE_CHANGELOG.md#2026-08-27--first-real-feature-placement-test-end-to-end). — 2026-08-27
- [x] **Frontend auth: login/register pages, wired end-to-end.** Auth
      service, HTTP interceptor, Login/Register pages (Reactive Forms +
      Material), Navbar reflects real session state. Verified in the
      browser: register -> reload persists session -> sign out -> sign
      back in, against the real backend + database. See
      [STRUCTURE_CHANGELOG.md](STRUCTURE_CHANGELOG.md#2026-08-27--frontend-auth-loginregister-pages-wired-end-to-end). — 2026-08-27
- [x] **Backend auth: register/login/JWT.** ASP.NET Core Identity +
      JWT bearer, `AuthController` (`register`/`login`/`me`), migration
      applied and verified end-to-end against the real database. Live in
      production too as of 2026-08-27 — see
      [STRUCTURE_CHANGELOG.md](STRUCTURE_CHANGELOG.md#2026-08-27--auth-live-in-production-jwt-signing-key--stale-dll-fix). See
      [STRUCTURE_CHANGELOG.md](STRUCTURE_CHANGELOG.md#2026-08-27--backend-auth-registerloginjwt-first-real-domain-model). — 2026-08-27
- [x] **Frontend switched to Angular.** React → Angular 22 + Material,
      deliberate (portfolio breadth), not a mistake. Same shell rebuilt
      (Navbar/Footer/theme). See
      [STRUCTURE_CHANGELOG.md](STRUCTURE_CHANGELOG.md#2026-08-26--frontend-switched-react---angular-22--material)
      and [ARCHITECTURE.md](ARCHITECTURE.md). — 2026-08-26
- [x] **Backend deployed live.** `https://english-c1-api.runasp.net` on
      MonsterASP.NET, HTTPS with redirect, connected to the real database.
      See [HOSTING.md](HOSTING.md). — 2026-08-26
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
