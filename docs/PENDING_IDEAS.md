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

## Scaffolded structurally, not implemented

- [ ] **Password reset + email confirmation.** The UI side has a real
      "Forgot your password?" link now (`pages/forgot-password/`), but
      it's an honest placeholder — actually sending a reset email still
      needs a sender (SendGrid free tier, or similar) and
      `AddDefaultTokenProviders()` isn't called yet. **When built: an
      emailed token link, not OTP codes** — explicitly requested, OTP
      flows are easy to get wrong if every failure path isn't handled.
      Not urgent for 2 users who won't forget their own passwords
      immediately, but real before anyone else ever gets an account.
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
- [ ] **Technical rename to Certis, deferred.** The name is decided
      (see NAMING.md) and applied everywhere user-visible, but the
      backend namespace (`EnglishC1.Client.*`), the repo folder
      (`english-c1-platform`), and the two live subdomains
      (`english-c1-api.runasp.net`, `english-c1.runasp.net`) still say
      the old placeholder name. Worth doing eventually for consistency;
      deliberately not done as a side effect of picking the name, since
      re-pointing the live subdomains means re-provisioning HTTPS certs
      and CORS from scratch again.

## Nice-to-haves mentioned along the way, not urgent

- [ ] Admin/teacher-facing panel, only if this grows beyond a 2-person
      tool (institutions idea) — not needed for the current scope.
- [ ] Toasts for lightweight confirmations, modals reserved for anything
      more consequential (e.g. submitting a graded exercise).
- [ ] Carousels / scroll-triggered animations on the landing page, once
      there's real content to showcase.

## Flagged for a future design pass (needs more direction first)

Raised 2026-08-27 alongside the quiz redesign - real, but too open-ended
to build without picking a direction first (see the changelog entry for
what *was* built that session):

- [ ] **"Ver cursos" - a course catalog.** Named once, no scope attached
      yet: is this a list of future skill modules (reading, listening...)
      shown as "coming soon," or an actual multi-course structure the
      placement test would feed into? Needs a real decision, not a guess.
- [ ] **Real imagery/illustrations**, not just inline SVG icons - the
      site currently has none. Needs a source (stock, commissioned,
      generated) and a place they'd actually earn their spot (the
      landing hero is the obvious first candidate).
- [ ] **Broader visual pass beyond the test module** - the redesign this
      session focused on the quiz/results/navbar specifically. Home and
      About got a content pass earlier (2026-08-27, design system entry)
      but not this round's card/animation treatment.
- [ ] Auto-resume: reloading mid-test currently lands back on the
      `/test` intro screen, not straight into the in-progress quiz - the
      answers and timer *are* preserved (see changelog), but the user
      has to click "Retake the test" once to see them restored. A fully
      seamless resume would skip that click.

## Done

- [x] **Login/register merged into one modal-style page**, sliding
      between Sign in/Register instead of two separate routes, plus a
      real "Forgot your password?" link (honest placeholder - see
      "Scaffolded structurally" above for what's still missing).
      Verified `redirectTo` still works after the merge. See
      [STRUCTURE_CHANGELOG.md](STRUCTURE_CHANGELOG.md#2026-08-27--loginregister-merged-into-one-modal-style-page). — 2026-08-27
- [x] **Brand name picked: Certis.** Chosen from four recommendations
      (Nivelo, Bandly, Fluentia, Certis) - "certify" evocation, good fit
      if this sells to institutions later. Wordmark replaces the "C"
      with a seal-and-ribbon icon. Applied to the navbar, footer, and
      page title. See [NAMING.md](NAMING.md) - the deeper technical
      rename (namespace, folder, subdomains) is deliberately deferred,
      see "Rough edges" above. — 2026-08-27
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
