# Pending ideas / not forgotten, just not built yet

A running list so nothing gets lost while other things get built first.
Move an item to "Done" (with a date and a link to the changelog entry)
instead of deleting it.

## Feature scope — first slice decided

The first real feature is now defined at a first-pass level: a
**placement/evaluation test**. Concretely:

- Registration is required to take it (ties results to a person, needed
  for progress tracking and for "guide me from here" follow-up).
- Both the user and their partner take it — this is the two-person
  private tool's actual starting point, not a hypothetical.
- Result: places the test-taker on a CEFR band (currently estimated
  around A2/B1-B2 heading toward C1) and the platform guides next steps
  from there.
- Explicitly wanted for the user's own portfolio — build it with real
  rigor, not a throwaway demo.
- The page also needs a visible "how it works" / methodology section
  (see the About page placeholder) — framed partly for future
  institutional visitors evaluating the platform's credibility.

Still open, needed before building the domain model:

- [ ] **Test format.** Multiple choice / gap-fill (self-gradable, no AI
      needed) was the earlier working assumption for a first slice — is
      that still right for a placement test specifically, or does
      placement need something more adaptive (e.g. question difficulty
      responds to answers so far)?
- [ ] **CEFR band scoring.** How do raw answers map to an A2-C1 estimate —
      a fixed scoring table, item-response-theory-style weighting, or
      something simpler for v1?
- [ ] Beyond the placement test: which skills come next (reading,
      writing, listening, speaking, vocabulary/grammar drills, full mock
      exams)?
- [ ] How is content authored — written/curated by hand and stored in the
      database, generated on demand (e.g. via an AI model), or both?

## Blocked on you / needs a decision

- [ ] **Final project name.** Three tentative candidates in
      [NAMING.md](NAMING.md) — pick one, reject all three, or hold off
      until the feature scope above is clearer (recommended, since a
      C1-specific name is a poor fit if scope grows beyond C1 later).

## Scaffolded structurally, not implemented

- [ ] `client-backend` — layered scaffold exists, builds, and is verified
      end-to-end against the real MonsterASP.NET SQL Server database
      (see HOSTING.md). Domain model so far is auth (Identity's own
      tables) — the actual English-practice entities still don't exist.
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
      `ConnectionStrings__AppDb` (see HOSTING.md) and now also
      `Jwt__SigningKey`, needed the same way before auth works on the
      live site at all. Fragile (a redeploy that forgets this step
      silently breaks the live site — 500.30 for the DB, 500 for any
      auth endpoint if the signing key's missing). Worth finding whether
      MonsterASP has a persistent env-var/app-settings panel outside
      `web.config`, or scripting the web.config edit so it's one command
      instead of a manual step, before this trips someone up. The next
      deploy needs to re-add *both* variables, not just the one from
      last time.
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

- [x] **Frontend auth: login/register pages, wired end-to-end.** Auth
      service, HTTP interceptor, Login/Register pages (Reactive Forms +
      Material), Navbar reflects real session state. Verified in the
      browser: register -> reload persists session -> sign out -> sign
      back in, against the real backend + database. See
      [STRUCTURE_CHANGELOG.md](STRUCTURE_CHANGELOG.md#2026-08-27--frontend-auth-loginregister-pages-wired-end-to-end). — 2026-08-27
- [x] **Backend auth: register/login/JWT.** ASP.NET Core Identity +
      JWT bearer, `AuthController` (`register`/`login`/`me`), migration
      applied and verified end-to-end against the real database. Not
      deployed to production yet (signing key missing from web.config —
      see "Rough edges" above) and no Angular UI yet (see above). See
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
