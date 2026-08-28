# Pending ideas / not forgotten, just not built yet

A running list so nothing gets lost while other things get built first.
Move an item to "Done" (with a date and a link to the changelog entry)
instead of deleting it.

## Feature scope — placement test shipped, what's next

The placement test (see "Done" below) answered the questions that used
to sit here as open decisions:

- **Format**: fixed-form, not adaptive - one sitting, self-gradable, no
  AI needed, far simpler to get right than adaptive branching for a v1.
  Started at 32 multiple-choice questions (Grammar + Vocabulary only);
  48 after Reading joined the assessed skills, then 64 after Listening
  did too (both 2026-08-28) - see "Done" below.
- **Scoring**: a 0-10 school-style grade per (level, skill) cell, 7/10 to
  pass (raised from a 60% threshold, 2026-08-28 - see "Done" below);
  placement = highest level passed consecutively from A2.
- **Content authoring**: hand-written, stored in the database, seeded at
  startup (`QuestionSeeder`) - not AI-generated.

Still open, now that the first slice exists:

- [ ] Should a reinforcement attempt that's passed update anything about
      the original placement, or stay purely a practice log forever? No
      auto-update exists yet - retaking the full placement test is
      currently the only way to change your recorded level.

## Requested 2026-08-28 — AI-generated practice, new skills, tutor assignment

One long message, broken into concrete items here so the `/loop` can work
them one at a time instead of trying to swallow all of it at once. Which
skills come next (the open question right above, until now) is answered
by this: **Reading and Listening**, explicitly named.

- [ ] **Speaking/conversation practice, matching with a real user.** The
      AI-only roleplay slice is done (see Done section below). What's
      left is "con quién voy a practicar" (presumably meaning the user's
      partner) - a materially bigger feature than the roleplay slice:
      presence, pairing, likely real-time state. Needs its own design
      pass before being buildable in a single loop iteration - not
      attempting this without scoping it properly first.

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
- [ ] i18n (Spanish for the app's own UI chrome) — not set up in the
      Angular rebuild yet; low priority, the platform's primary language
      is English by design. See ARCHITECTURE.md for the library options
      considered (`@angular/localize` vs `ngx-translate`).

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
## Done

- [x] **Listening comprehension as a real skill.** Was blocked on Groq's
      TTS (terms not accepted) and ElevenLabs (not installed); unblocked
      by a live product decision to generate audio locally with
      `edge-tts` instead. New `SkillArea.Listening` + `Question.AudioUrl`;
      16 clips generated by `scripts/generate_listening_audio.py` and
      served as static files (`app.UseStaticFiles()`). Placement test
      grew from 48 to 64 questions as the same intended consequence
      Reading caused. AI-generated reinforcement doesn't support
      Listening yet, same as Reading. See
      [STRUCTURE_CHANGELOG.md](STRUCTURE_CHANGELOG.md#2026-08-28--listening-comprehension-the-second-new-skill-audio-generated-locally-with-edge-tts). — 2026-08-28
- [x] **Speaking practice: AI-only roleplay, Cambridge-exam style.** Four
      fixed scenarios modeled on the four parts of a real Cambridge
      speaking exam; a text chat where Groq plays the examiner/partner
      role and stays in character. Stateless for this first slice - full
      history sent by the client each turn, no persistence yet.
      Matching with a real user is split out as its own not-yet-scoped
      item above. See
      [STRUCTURE_CHANGELOG.md](STRUCTURE_CHANGELOG.md#2026-08-28--speaking-practice-ai-only-roleplay-cambridge-exam-style). — 2026-08-28
- [x] **Reading comprehension as a real skill.** New `SkillArea.Reading`
      + `Question.Passage`; 16 hand-written passage-based questions (4
      per level). Placement test grew from 32 to 48 questions as an
      intended consequence (a CEFR result that ignored Reading wouldn't
      reflect reading ability) - intro copy and About page updated to
      match. Admin question editor shows/validates the Passage field
      reactively. AI-generated reinforcement doesn't support Reading yet
      (would need a passage-aware prompt) - the "AI-generated" button is
      hidden for Reading rather than generating a passage-less
      pseudo-reading question. See
      [STRUCTURE_CHANGELOG.md](STRUCTURE_CHANGELOG.md#2026-08-28--reading-comprehension-the-first-new-skill-beyond-grammarvocabulary). — 2026-08-28
- [x] **Tutor-student assignment, with a seeded "AI Tutor" persona.**
      `ApplicationUser.TutorId` (self-referencing, one tutor per student)
      + a real seeded `ai-tutor@certis.local` account (Tutor role, no
      one can sign into it) auto-assigned to the requesting account.
      Admin Access tab can assign/clear any account's tutor; Dashboard
      shows "Your tutor: X". Verified end-to-end including logging out
      and back in to see the assignment take effect. See
      [STRUCTURE_CHANGELOG.md](STRUCTURE_CHANGELOG.md#2026-08-28--tutor-student-assignment-with-a-seeded-ai-tutor-persona). — 2026-08-28
- [x] **AI-generated reinforcement exercises (Groq).** A "Practice
      different questions (AI-generated)" button on the reinforcement
      page generates fresh questions for that (level, skill) via Groq
      (structured JSON output, validated, one retry on a bad
      generation), persisted as real `Question` rows
      (`IsAiGenerated = true`) so grading/sampling/pooling all work
      through the exact same path as the hand-written bank - no
      special-casing needed anywhere else. Admin Content tab shows an
      "AI" badge per generated question. See
      [STRUCTURE_CHANGELOG.md](STRUCTURE_CHANGELOG.md#2026-08-28--ai-generated-reinforcement-practice-groq). — 2026-08-28
- [x] **Early-warning threshold reworked to a 0-10 school-style grade,
      below 7 = priority.** Explicit decision from the user. Raised the
      pass bar from 60% to 70% (`PlacementScorer.PassThreshold`), added
      a computed `Grade` (/10) to `SkillBreakdown` and `TestResultDto`,
      shown as a badge on every breakdown card. Pulled the threshold out
      of three separate hardcoded `0.6`s into a shared `core/grading.ts`
      on the frontend while doing it. About/Home copy updated to say
      "7/10" instead of "60%". See
      [STRUCTURE_CHANGELOG.md](STRUCTURE_CHANGELOG.md#2026-08-28--school-style-0-10-grading-pass-threshold-raised-to-710). — 2026-08-28
- [x] **Toasts for lightweight confirmations.** `core/toast.ts`
      (`Toast` service over `MatSnackBar`, styled to match the app's
      pill language). Wired into the three real gaps that existed -
      Admin Content tab (question added/updated/deleted) and Admin
      Access tab (role toggle) - all of which used to succeed silently.
      See [STRUCTURE_CHANGELOG.md](STRUCTURE_CHANGELOG.md#2026-08-27--toasts-for-lightweight-confirmations-and-another-stale-docs-fix). — 2026-08-27
- [x] **Dark mode toggle.** Another stale line found while working this
      list - fully wired in the `Theme` service since the Angular switch
      (commit `4999c01`), and has been in active use every session since
      (every dark-mode screenshot/verification in this changelog used
      it). Nothing left to build; light just stays the default until
      changed. — 2026-08-27
- [x] **Auto-resume on the placement test.** A reload mid-test now lands
      straight back in the quiz - exact questions, answers, and timer
      restored - no more one click on "Retake the test" first. Also fixed
      a real bug the previous entry's sampling change had introduced (a
      reload could silently re-fetch a *different* random sample of
      questions than the one the saved answers were keyed to). See
      [STRUCTURE_CHANGELOG.md](STRUCTURE_CHANGELOG.md#2026-08-27--real-auto-resume-for-the-placement-test-and-a-stale-docs-fix). — 2026-08-27
- [x] **"How it works" / methodology content on the About page.** Found
      already done while working this list, just never checked off - the
      earlier About redesign (icon cards + scroll-reveal) replaced the
      "Coming soon" placeholder with real methodology content (Format,
      How the level is calculated, Reinforcement, Scope honestly). No new
      code needed here, just fixing a stale line in this file. See
      `git log -- client-frontend/src/app/pages/about/about.html`
      (commit `d6f4a24`). — 2026-08-27
- [x] **Question bank doubled (32 → 64), placement stays fixed at 32.**
      4 more hand-written questions per (level, skill) cell, same
      explanation-per-question quality bar. `GetPlacementQuestionsAsync`
      now randomly samples 4-of-8 per cell instead of loading everything,
      so the placement test stays the advertised ~15 minutes while two
      attempts won't always show the identical 32 questions. Reinforcement
      quizzes now show all 8 per cell (a pure improvement, not
      length-advertised anywhere). Also fixed a latent `TotalQuestions`
      bug this sampling would otherwise have exposed (it was counting the
      loaded question pool, not what was actually answered - see
      changelog). See
      [STRUCTURE_CHANGELOG.md](STRUCTURE_CHANGELOG.md#2026-08-27--doubled-the-question-bank-32--64-placement-stays-32-via-sampling). — 2026-08-27
- [x] **Admin/Tutor panel (`/admin`) + question-bank CRUD + AI insight.**
      Real Identity roles (Admin, Tutor); Students/Content/Access tabs;
      question editor dialog (create/edit/delete) replacing
      seed-only content; Access tab to grant/revoke roles with a
      self-demotion guard. Plus: an on-demand "Get AI feedback on this
      attempt" button on the test-result page, calling Groq
      (`openai/gpt-oss-20b`, `reasoning_effort: low` — confirmed correct
      against [Groq's own reasoning docs](https://console.groq.com/docs/reasoning))
      for a personalized diagnostic beyond the generic early-warning flag.
      `Admin:Email` is now set to `anthonysosa44@gmail.com` so the panel
      is actually reachable — picked as a working default, not a final
      decision; revisit who should hold Admin vs Tutor once both accounts
      exist and you've looked at the Access tab.
      See [STRUCTURE_CHANGELOG.md](STRUCTURE_CHANGELOG.md#2026-08-27--admintutor-panel-admin-question-bank-crud-and-an-ai-generated-personalized-insight). — 2026-08-27
- [x] **Student dashboard (`/dashboard`).** Full attempt history (not
      just the latest), split into placement and reinforcement activity,
      behind `authGuard`. Pulled the CEFR level and skill-icon mappings
      into shared `core/cefr.ts`/`core/skill-icons.ts` while doing it.
      Groundwork the admin panel can reuse later (same `GetHistoryAsync`
      shape, per-user instead of platform-wide). See
      [STRUCTURE_CHANGELOG.md](STRUCTURE_CHANGELOG.md#2026-08-27--student-dashboard-dashboard). — 2026-08-27
- [x] **3D model in the hero: picked and placed.** Open Book, rotating
      next to the headline in `home__hero-row` - the two runner-up
      candidates (grad-cap.glb, globe.glb) stay in `public/models/`,
      reserved for another spot or a loading screen. See
      [STRUCTURE_CHANGELOG.md](STRUCTURE_CHANGELOG.md#2026-08-27--picked-the-open-book-model-moved-it-into-the-hero-itself). — 2026-08-27
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
