# Structure changelog

Every new top-level folder, new app, or significant architectural change
gets an entry here, newest first — this is the traceability log the notes
asked for, separate from git history so it reads as a narrative instead of
a diff.

## 2026-08-27 — Auth is now a real modal (MatDialog); clearer below-A2 label

Also from the same feedback round: the result badge's "—" for a
below-A2 placement read as too vague ("I don't even know roughly what
level I'm at") - changed to "Pre-A2" / "Beginner" (`test-result.ts`
`levelCode()`/`levelName()`), a short, unambiguous label instead of a
symbol.

Correction to the entry right below: that version still routed to
`/login`/`/register` on a full navigation - it *looked* like a modal but
wasn't one. Rebuilt as an actual `MatDialog` overlay:

- `pages/auth/auth-dialog.ts` (renamed from `auth-page.ts`) reads its
  initial mode from `MAT_DIALOG_DATA` instead of route data, and closes
  via `MatDialogRef` (with `{ redirectTo }` in the close result) instead
  of calling the router itself.
- `core/auth-dialog.service.ts` - `AuthDialogService.open(mode,
  redirectTo?)` opens it; refuses to stack a second one if one's already
  open.
- `/login` and `/register` routes removed entirely - nothing routes to
  auth anymore. The navbar's "Sign in" button, Home's test CTA, and the
  auth guard all call the service directly.
- `auth.guard.ts`: no longer returns a `UrlTree` to `/login` - it calls
  `authDialog.open('login', state.url)` and returns `false`, blocking
  the navigation so the user stays exactly where they were with the
  modal now open over it (rather than landing on a route that doesn't
  exist anymore).
- Added `@angular/animations` + `provideAnimationsAsync()` -
  `MatDialog`'s open/close transition needs it; wasn't installed before
  since nothing used Material's overlay animations yet.
- A global rule in `styles.scss` strips Material's own dialog surface
  chrome (`.auth-dialog-panel .mdc-dialog__surface`) so
  `auth-dialog.scss`'s own card (background/border/shadow/radius) isn't
  boxed twice.
- Verified: clicking "Sign in" opens the dialog with the URL unchanged
  (`/` stays `/`); hitting a protected route while logged out opens the
  dialog without ever navigating to the blocked route, and signing in
  through it lands on the originally-requested page - both the navbar
  and the guard path tested end-to-end.

## 2026-08-27 — Login/register merged into one modal-style page

Separate `/login` and `/register` pages felt like a full context switch
for what should be a quick in-and-out. Merged into
`pages/auth/auth-page.ts`: one component, two forms, an internal
`mode` signal - switching tabs is a local state change, not a route
navigation, so a CSS `transform: translateX` slide animates between
them (Sign in / Register tabs with an animated underline indicator on
top). `/login` and `/register` still both route here (`data: { mode }`
sets which panel opens), so deep links and the auth guard's
`redirectTo` keep working exactly as before - verified: logging out,
hitting a protected route, landing on `/login?redirectTo=...`, and
signing in still lands back on the original page.

Also added a real "Forgot your password?" link - it goes to
`pages/forgot-password/`, which states plainly that password recovery
isn't wired up yet (no email sender configured - see
PENDING_IDEAS.md) rather than shipping a form that silently does
nothing. When it does get built: an emailed reset-token link
(ASP.NET Identity's `GeneratePasswordResetTokenAsync`), not OTP codes
- flagged explicitly as the safer choice given how many ways an OTP
flow can go wrong if every failure path isn't handled.

## 2026-08-27 — Brand name picked: Certis

Chose from four recommendations (Nivelo, Bandly, Fluentia, Certis) -
see [NAMING.md](NAMING.md) for the reasoning. Applied to everything
user-visible:

- `core/brand.ts` - single `BRAND_NAME` constant, replacing the
  `'[Project name]'` placeholder duplicated in navbar and footer.
- Navbar wordmark: the "C" is a custom SVG seal-and-ribbon glyph (a
  certification badge) sized and positioned inline with the "ertis"
  text, sitting where the letter would be - not a separate icon next
  to a text label like before.
- `index.html` `<title>`.

Deliberately NOT done: the backend namespace (`EnglishC1.Client.*`),
repo folder (`english-c1-platform`), and the two live subdomains still
carry the old placeholder name - none of that is user-visible, and
re-pointing the live subdomains means redoing HTTPS provisioning and
CORS from scratch (see how much friction that was in the two entries
below). Tracked as its own item in PENDING_IDEAS.md, not bundled in
here.

## 2026-08-27 — Fixed dark-mode overflow, redesigned practice-area cards

- **Result badge overflow bug** (reported in dark mode, but present in
  light too): "Below A2" wrapped to two lines inside the fixed 190px
  ring and spilled outside it top and bottom, since `.result__badge-inner`
  had no `overflow: hidden`. Fixed by keeping the big number short (a
  level code, or "—" for the below-A2 case) and moving the descriptive
  text ("Just starting out", "Intermediate", ...) to a small caption
  underneath - plus a slightly bigger ring (13.5rem) and inset padding
  so nothing touches the edge.
- **Practice-area cards redesigned** - they read as flat "Practice this"
  buttons on solid red blocks, alarming when every cell needs work and
  not far off a plain list. Now: an icon per skill (open book for
  Grammar, speech bubble for Vocabulary), a level-colored badge and
  border instead of a full color fill, and the CTA as a proper pill
  button - closer to how a course catalog presents a module than an
  error state.

## 2026-08-27 — Reinforcement review + Contact page removed

- **Reinforcement failure review.** `Question.Explanation` (nullable,
  hand-written for all 32 seed questions) + a new `TestResultDto.MissedQuestions`
  list, built server-side only for wrong answers - never sent before or
  during the test, so it can't leak answers early. Failing a
  reinforcement quiz now shows exactly which questions were wrong, what
  you answered, the correct answer, and a one-line grammar/vocabulary
  explanation, instead of just a score.
  - Since the question bank was already seeded (locally and in
    production), adding `Explanation` couldn't just re-seed - that would
    orphan the `QuestionId` on every `TestAttempt` already recorded (no
    real FK constraint stops it, but `GetLatestResultAsync`'s breakdown
    lookup would silently come back empty for old attempts). Added
    `QuestionSeeder.BackfillExplanationsAsync`, which matches existing
    rows by `Text` and fills in the explanation - runs every startup,
    no-ops once nothing's missing.
- **Contact page removed** - route, nav link, and the page itself
  deleted.

## 2026-08-27 — Quiz redesign: timer, persistence, hidden difficulty, premium look

Feedback after using the live test: felt mediocre for something meant to
be sold eventually, the elapsed timer wasn't visible, progress was lost
on a reload/reconnect, and each question openly showed its CEFR
level/skill (undermining the whole point of a placement test). Used the
`design` skill first to settle the visual direction before touching
Angular code - a 2-artboard canvas (quiz screen + result badge),
published at the design canvas link shared in chat.

- `shared/quiz/quiz.ts`: dropped `mat-radio-group` for plain clickable
  option "cards" (letter badge, fill + underline + checkmark on select,
  a pop/draw-in animation) - re-skinning Material's MDC radio internals
  fought the new look more than it helped. Added a visible elapsed timer
  and, when a `storageKey` input is set, full persistence: answers and
  start time survive a reload or the connection dropping mid-test,
  restored on init and re-saved on every answer. Cleared only once the
  parent confirms the submit actually succeeded, not on stage change -
  the old behavior silently lost every answer if a submit failed, since
  the quiz component gets destroyed the moment the page leaves the
  "taking" stage.
- Removed the per-question `{{ level }} · {{ skillArea }}` label
  entirely - a placement test should not tell the test-taker which
  questions are "harder" while they're answering.
- `placement-test.ts`/`reinforcement.ts`: pass a `storageKey` down
  (`placement-test-in-progress`, `reinforcement-in-progress-{level}-{skill}`)
  and clear it directly after a confirmed successful submit.
- `test-result.ts`/`.html`/`.scss`: the flat colored badge became an
  animated SVG progress ring (radial glow, score-proportional arc) around
  the CEFR letter - closer to a reward than a stat.
- `layout/navbar/`: added a shadow, border, gradient logo mark, and an
  animated underline on the active link - it was reading as flat and
  unbranded before.
- Verified in-browser: timer counts up and survives a reload (answers +
  elapsed time both restored exactly), submitting clears the persisted
  draft, level/skill no longer shown per question, navbar and result
  badge render as designed.

Not done this pass (explicitly flagged, larger/vaguer asks that need
more direction before building): a course catalog ("ver cursos"), real
downloaded imagery/illustrations beyond icons, and a broader visual
pass across pages beyond the test module. See PENDING_IDEAS.md.

## 2026-08-27 — Frontend live in production too: english-c1.runasp.net

The whole platform is now publicly reachable, not just the API. Full
details in [HOSTING.md](HOSTING.md#frontend-hosting-monsteraspnet-live-decided-2026-08-27):

- Second FreeSite on the same MonsterASP account (`site87768`) instead of
  a new Cloudflare Pages signup - `ng build`'s output is plain static
  files, IIS serves that fine, no reason to add a second hosting provider
  for a 2-person tool.
- `core/api-config.ts` now picks the backend URL from `location.hostname`
  at runtime (localhost vs production) instead of a hardcoded local URL.
- Added a hand-maintained `web.config` with an IIS rewrite rule so direct
  hits on Angular routes (`/test`, `/about`, a page refresh) serve
  `index.html` instead of 404ing - `ng build` doesn't generate this,
  it has to be copied into `dist/` after every clean build.
- Backend `Program.cs` CORS policy extended to allow the new frontend
  origin (both http and https initially, since the SSL cert hadn't
  finished provisioning at first deploy and the http-vs-https scheme
  mismatch failed CORS preflight - see HOSTING.md for the cleanup note).
- Deploy hit the same locked-DLL issue as the backend's redeploy
  (see the entry below) on the *backend* redeploys needed for the CORS
  change - same `app_offline.htm` fix, now a routine step.
- Verified end-to-end against the live domains: HTTPS on both, a real
  login from `https://english-c1.runasp.net` reaching
  `https://english-c1-api.runasp.net` with no CORS errors, the
  `redirectTo` post-login flow landing back on `/test`, and the 32-item
  question bank confirmed present in the production database.

## 2026-08-27 — First real feature: placement test, end to end

The first vertical slice beyond auth - a full CEFR placement test,
decided and scoped in `docs/PENDING_IDEAS.md` weeks ago, built this
session in one pass (domain model, API, Angular UI, verified in-browser).

**Backend** (`client-backend/src/EnglishC1.Client.Domain/PlacementTest/`,
`.../Application/PlacementTest/`, `.../Infrastructure/PlacementTest/`,
`.../Api/PlacementTest/`):
- Domain: `Question`/`QuestionOption`/`TestAttempt`/`TestAnswer` entities,
  plus `PlacementScorer` - the actual scoring/placement algorithm, kept
  as pure functions with zero EF Core dependency specifically so it's
  unit-testable. 6 tests added in
  `tests/EnglishC1.Client.Domain.Tests/PlacementTest/PlacementScorerTests.cs`,
  covering the threshold, the "consecutive from A2" placement rule (a
  weak A2 cell caps placement even with a perfect C1 score - the subtle
  case worth locking in with a test), and the empty-input edge case.
- **Format decided**: fixed-form (not adaptive) - 32 questions, one
  sitting, 4 questions per (level, skill) cell across A2/B1/B2/C1 x
  Grammar/Vocabulary. Chosen over adaptive because it's self-gradable
  with no AI needed and far simpler to build correctly, per the tradeoff
  noted in PENDING_IDEAS.
- **Scoring decided**: a cell passes at >=60% correct. Placement = the
  highest level where every cell from A2 up passed, consecutively - one
  gap caps the result there, matching how CEFR placement is meant to
  work. Any cell under 60%, anywhere, is flagged for reinforcement
  independent of the overall placement.
- Question bank: 32 hand-written multiple-choice items (not AI-generated
  or copied), seeded once at startup via `QuestionSeeder` if the table's
  empty. Options shuffle server-side per fetch so "the first option is
  always right" isn't a discoverable pattern.
- `TestController`: `GET placement/questions`, `POST placement/submit`,
  `GET results/placement/latest`, `GET reinforcement/{level}/{skill}/questions`,
  `POST reinforcement/{level}/{skill}/submit` - all `[Authorize]`.
  Reinforcement attempts are logged via the same `TestAttempt` table
  (`Kind` + `FocusLevel`/`FocusSkill`) rather than a second table, so
  there's a free history without extra schema.
- Enums now serialize as strings (`"B1"`, `"Grammar"`), not the JSON
  default numeric index - added `JsonStringEnumConverter` in `Program.cs`.
- New EF Core migration `AddPlacementTest`, applied to the real database.

**Frontend** (`client-frontend/src/app/pages/test/`,
`shared/quiz/`, `core/test-api.ts`, `core/auth.guard.ts`):
- `shared/quiz/`: one reusable quiz-taking component (progress bar,
  per-question radio groups, submit-when-complete) used by both the
  placement test and every reinforcement quiz - real shared complexity,
  not premature abstraction.
- `/test`: shows the previous result (if any) or an intro, then the full
  quiz; submits and routes to `/test/results`.
- `/test/results`: CEFR badge (color-coded, see below), score, and a
  breakdown grid - every weak cell gets a "Practice this" button routing
  straight to its reinforcement quiz.
- `/test/reinforce/:level/:skill`: a short targeted quiz for one cell,
  immediate pass/fail feedback, retry inline.
- All three routes guarded by a new `authGuard` (`core/auth.guard.ts`,
  the first protected-route guard in the app) - unauthenticated visitors
  bounce to `/login` with a `redirectTo` query param that `login.ts`/
  `register.ts` now honor instead of always landing on `/`.
- Verified in-browser end-to-end: answered all 32 (scored 3/32
  deliberately with wrong answers) -> "Below A2" result with every cell
  flagged -> clicked into A2 Grammar reinforcement -> answered correctly
  -> "Nice - you've got this." Confirmed via the real API too (GET
  `results/placement/latest` returns the persisted attempt).

**Design pass** (`styles.scss`, `layout/navbar/`, `pages/home/`,
`pages/about/`): replaced the generic Material green/blue starter
palette with azure (primary) + orange (tertiary) - warmer, more
education-associated, per the "needs color, needs to feel intuitive"
feedback. Added CEFR band CSS variables (`--cefr-a2` etc.) reused
everywhere a level displays. Home and About rewritten with real content
(hero, "how it works" steps, level chips; methodology write-up) instead
of scaffold placeholders. Navbar got a real icon instead of a "?" and a
"Take the test" link.

**Note on browser-testing Material radio buttons**: clicking a
`mat-radio-button` host element directly does nothing - Angular Material
only wires the click handler to the label and the native `<input>`
inside it. Click one of those two, not the custom element itself, when
scripting interactions.

## 2026-08-27 — Auth live in production (JWT signing key + stale DLL fix)

Finished what the previous entries left pending: the deployed API at
`https://english-c1-api.runasp.net` now has working auth. Turned into a
real debugging session, not just a config drop-in — full diagnosis in
[errors/2026-08-27-stale-publish-output-dll-mismatch.md](errors/2026-08-27-stale-publish-output-dll-mismatch.md):

- Added `Jwt__SigningKey` (a fresh, random, production-only value — never
  shared with the local dev secret) to `wwwroot/web.config` alongside the
  existing `ConnectionStrings__AppDb`.
- First redeploy attempt revealed auth had never actually been deployed
  at all — `/api/auth/*` 404'd because the live `.dll` predated
  `AuthController`; only a one-off `web.config` edit had been pushed
  before, never a full `dotnet publish` + upload since auth was built.
- Second attempt (`scp -r`) partially failed: IIS had the running
  process's own DLLs locked, so several core assemblies silently kept
  their old versions. Fixed by uploading an empty `app_offline.htm`
  first (stops the app, releases the locks), then re-uploading, then
  deleting `app_offline.htm` via an interactive `sftp` session to bring
  the site back.
- That still 500'd on *every* request, including `/health`. Found via
  MonsterASP's control panel **Logs → ASP.NET Core debug** (new
  discovery this session, much faster than pulling raw log files over
  SFTP): a `FileNotFoundException` for `System.IdentityModel.Tokens.Jwt,
  Version=7.1.2.0` — the locally published copy of that one DLL was
  stuck at an old `6.35.0` from reusing `publish-output/` across many
  publishes this session without ever clearing it. Deleted every
  `bin`/`obj`/`publish-output` folder in the backend, republished clean,
  redeployed the same app_offline way.
- Verified end-to-end against the live server: `/health` → 200,
  `POST /api/auth/register` → 200 with a real JWT, against the real
  production database.

`docs/HOSTING.md` updated with the app_offline procedure, the
clean-rebuild-before-deploy rule, and a pointer to the MonsterASP log
panels for next time.

## 2026-08-27 — Reinstated password policy with a visible hint

After the previous fix relaxed the policy to length-only (8+ chars, any
characters), the user asked for a real rule back, as long as it's shown
to the person typing: "la politica si la veo necesaria... que salga ahi
tipo abajo como que con caracteres minimo uno y una minima longitud."
Chose the simplest rule that still says something (8+ characters, at
least one digit) so it fits in one line of hint text under the field:

- `DependencyInjection.cs`: `Password.RequireDigit = true` (length stays
  8, everything else — uppercase/lowercase/symbol — stays off).
- `register.ts`: added `Validators.pattern(/\d/)` to the password
  control and a `passwordHint` string, both explicitly commented as
  needing to stay in sync with the backend's Identity config (this is
  exactly the kind of drift that caused
  [errors/2026-08-27-password-policy-mismatch.md](errors/2026-08-27-password-policy-mismatch.md)).
- `register.html`: `<mat-hint>` shows the rule under the password field
  by default, and switches to the same text as a `<mat-error>` once the
  field is touched and invalid (missing length or missing digit).

Verified end-to-end in the browser: a password without a digit disables
the submit button and shows the hint as an error; a valid password
(`testpass01`) registers successfully, redirects home, and logs in
correctly afterward.

## 2026-08-27 — Fixed misleading registration error (password policy mismatch)

Found immediately after shipping auth: the user's real registration
attempt failed with "may already be registered, or password too weak" -
neither was actually true. Root cause was two compounding bugs, full
diagnosis in
[errors/2026-08-27-password-policy-mismatch.md](errors/2026-08-27-password-policy-mismatch.md):
Identity's default password rules (needs uppercase/lowercase/digit/symbol)
didn't match what the Angular form actually validated (8-char minimum
only), and the frontend showed a fixed guessed message instead of the
backend's real error. Fixed both: password policy relaxed to length-only
in `DependencyInjection.cs`, and `register.ts` now parses and displays
the actual `ValidationProblem()` response. Cleaned up the test account
accidentally created under the user's real email while diagnosing this.
Verified: a real duplicate-email attempt now shows the specific "Email
'...' is already taken" message instead of a guess.

## 2026-08-27 — Frontend auth: login/register pages, wired end-to-end

Angular side of auth, on top of yesterday's backend work:

- `core/auth.ts` (`Auth` service): signals for `token`/`email`/
  `isAuthenticated`, `register()`/`login()`/`logout()`, session persisted
  in `localStorage` (survives a page reload — verified).
- `core/auth.interceptor.ts`: attaches `Authorization: Bearer <token>`
  only to requests whose URL starts with our own API's base URL (never
  to third-party requests).
- `core/api-config.ts`: `API_BASE_URL` pointed at `localhost:5223` for
  now — no Angular `environment.ts` setup yet (the CLI doesn't scaffold
  one by default anymore), revisit when the frontend itself gets
  deployed somewhere.
- `pages/login/` and `pages/register/`: Reactive Forms + Material
  (`mat-form-field`/`mat-input`/`mat-button`), client-side validation
  (required, email format, 8-char minimum password, password-confirmation
  match on register), loading state, error messages.
- Navbar: "Sign in" now actually links to `/login`; once authenticated it
  shows the user's email and a "Sign out" menu (desktop dropdown and
  mobile drawer both updated).
- `app.config.ts`: added `provideHttpClient(withInterceptors([authInterceptor]))`.

No new npm packages needed — Reactive Forms, HttpClient, and the Material
form components were already available from the existing Angular/Material
install.

Verified in the real browser end-to-end: register -> navbar shows email
-> reload persists the session -> sign out -> sign back in with the same
credentials, all against the live backend + real database. (Note for
future browser-driven testing in this repo: `computer` click+type on
Material form fields was unreliable in this session — layout apparently
shifts after the floating label animates, landing clicks/typing on the
wrong element. `form_input` by ref, or a direct `element.click()` /
`.value = ...` via `javascript_tool`, worked reliably instead.)

Not done yet: route guard (nothing needs protecting client-side yet — no
routes exist that require auth), the JWT signing key still isn't in the
production `web.config` (see yesterday's entry and HOSTING.md).

## 2026-08-27 — Backend auth: register/login/JWT, first real domain model

`client-backend` now has actual auth, not just a health check:

- `AppDbContext` -> `IdentityDbContext<ApplicationUser, IdentityRole<Guid>,
  Guid>`. `ApplicationUser` lives in `Infrastructure/Identity/` (tightly
  coupled to the Identity framework, not a pure Domain concept).
- `AddIdentityCore<ApplicationUser>` wired up in
  `Infrastructure/DependencyInjection.cs` (8-char minimum password,
  unique email required, `RequireConfirmedEmail = false` since there's no
  email sender yet). `AddDefaultTokenProviders()` deliberately not called
  yet — only needed for password-reset/email-confirmation tokens.
- JWT bearer auth: `JwtOptions` (Issuer/Audience/ExpirationMinutes in
  `appsettings.json`, `SigningKey` in `dotnet user-secrets` only — a
  freshly generated random secret, not tied to any account) and
  `JwtTokenService` in Infrastructure; `AddAuthentication().AddJwtBearer()`
  wired in `Program.cs`. JWT over cookies because Angular calls the API
  cross-origin as a separate SPA.
- `AuthController` (`Api/Auth/`): `POST /api/auth/register`,
  `POST /api/auth/login`, `GET /api/auth/me` (`[Authorize]`). Swagger UI
  now has a Bearer auth scheme configured so these are testable directly
  from `/swagger`.
- Migration `AddIdentity` created and applied against the real MonsterASP
  database (all `AspNetUser*`/`AspNetRole*` tables). Verified end-to-end
  with curl: register -> login -> `/me` without a token (401) -> `/me`
  with a valid token (200, correct email back).
- CORS origin fixed from the stale `localhost:5173` (old React/Vite port)
  to `localhost:4200` (Angular) — leftover from the frontend switch that
  hadn't been caught since nothing had exercised CORS yet.

Not done yet: Angular-side auth (login/register pages, auth service,
interceptor, route guard), the JWT signing key isn't added to the
production `web.config` yet (same class of gotcha as the connection
string — deployed auth endpoints will 500 until that's done), Google
OAuth, and password reset. See
[PENDING_IDEAS.md](PENDING_IDEAS.md#rough-edges-worth-revisiting).

## 2026-08-26 — Fixed backend hang too: dotnet.exe now called by full path

Follow-up to the ng serve fixes below — once the frontend was reliable,
the **backend** started hanging the same way (window open, port 5223
never bound). Redirecting its output too (same trick as the frontend)
revealed the real cause: a plain `dotnet` call in the spawned window was
resolving to the wrong `dotnet.exe` (a machine-wide, SDK-less one at
`C:\Program Files\dotnet`) instead of the per-user SDK. An earlier fix
for this exact class of problem (prepending to `$env:Path`) didn't
actually work — its "is the path already present" check matched a
substring that existed *later* in PATH, so it skipped prepending and the
wrong exe kept winning. Fixed for real by calling the SDK's `dotnet.exe`
by its full path everywhere in the script, avoiding PATH resolution
entirely. Full trail in
[errors/2026-08-26-ng-serve-hangs.md](errors/2026-08-26-ng-serve-hangs.md#update-the-backend-had-a-third-cause-too).
Verified: two clean stop/start round-trips, frontend + backend both up
within seconds each time.

## 2026-08-26 — Fixed ng serve hanging on start

Found (the frontend silently failed to come up after `start.bat`, twice)
and fixed two separate causes — full diagnosis in
[errors/2026-08-26-ng-serve-hangs.md](errors/2026-08-26-ng-serve-hangs.md).
`scripts/start-dev.ps1` now clears `client-frontend/.angular/cache` before
every frontend start and redirects `ng serve`'s output to
`%TEMP%\ng-serve.log` instead of the spawned console window (the second
cause; that specific combination reliably prevented the hang across
repeated testing). Verified with two clean stop/start round-trips.

## 2026-08-26 — Frontend switched: React -> Angular 22 + Material

Deliberate change, not a correction — the user has many existing React
projects and wants portfolio breadth. Blazor (writing the frontend in C#
too, leaning fully into the .NET-skills goal) was floated first and
briefly started, but the user chose Angular by name after weighing it, so
that's what got built.

- Deleted `client-frontend` (React) and rebuilt it from scratch with
  `ng new client-frontend --routing --style=scss --ssr=false`, then
  `ng add @angular/material` (Material 3 theming via `mat.theme()`,
  palette retargeted from the default azure-blue to green to match the
  brand accent the React version used).
- Rebuilt the same shell the React version had: `Navbar` (responsive —
  horizontal nav >= 768px, `mat-menu` mobile dropdown below that),
  `Footer`, a placeholder `Home` page, and a signal-based `Theme` service
  (`src/app/core/theme.ts`) — light default, dark mode fully wired via an
  `html.dark` class.
- Icons: inline SVG (menu, sun/moon) instead of a package — `lucide-angular`
  doesn't support Angular 22 yet (peer dependency conflict), and inline
  SVG was simplest not to block on that.
- Had to update Node.js (v22.22.0 -> v22.22.3 via the already-installed
  `nvm4w`) — the latest Angular CLI refused to run on the older patch
  version. See `docs/DEPENDENCIES.md`.
- Updated `scripts/start-dev.ps1` / `stop-dev.ps1` (port 5173 -> 4200) and
  `.claude/launch.json` / `scripts/dev-frontend.cmd` for the preview
  tooling. Verified end-to-end: build clean, dev server serves correctly,
  dark mode toggle works, mobile breakpoint correctly collapses the nav.
- Old React code isn't lost — fully recoverable from git history — but is
  not part of the active codebase.

## 2026-08-26 — Backend deployed live: MonsterASP.NET

`client-backend` is reachable at `https://english-c1-api.runasp.net`
(FreeSite plan, EU datacenter, HTTPS via Let's Encrypt with HTTP->HTTPS
redirect on). `/health` verified 200 over both HTTP (redirects) and
HTTPS. Deployed via `dotnet publish` + `scp` (SFTP) to `wwwroot/` — chosen
over WebDeploy (would've needed installing Web Deploy/msdeploy locally)
and Git deploy (needs a GitHub repo, not set up yet). Full steps and the
production-connection-string handling (env var injected into
`web.config`, not committed anywhere) in
[HOSTING.md](HOSTING.md#backend-hosting-monsteraspnet-live-decided-2026-08-26).
Known fragility logged in [PENDING_IDEAS.md](PENDING_IDEAS.md#rough-edges-worth-revisiting):
`web.config` gets regenerated (and the env var lost) on every fresh
`dotnet publish`.

## 2026-08-26 — Backend renamed: NutriBoost.Client.* -> EnglishC1.Client.*

Closed out the last tracked holdover from the scope mix-up (see
[errors/2026-08-26-scope-mixup.md](errors/2026-08-26-scope-mixup.md)):
every folder, `.sln`/`.csproj` file, and C# `namespace` declaration in
`client-backend` renamed `NutriBoost.Client.*` -> `EnglishC1.Client.*`
(Domain, Application, Infrastructure, Api, and the Domain.Tests project).
Rebuilt clean, tests still pass (0 tests — the project is still empty),
and re-verified the app starts and connects to the real MonsterASP
database under the new names. `EnglishC1` is a descriptive placeholder
tied to the platform's subject matter, not a proposed final brand — see
[NAMING.md](NAMING.md), still undecided. Updated every doc and script that
referenced the old path (`README.md`, `client-backend/README.md`,
`ARCHITECTURE.md`, `DEPENDENCIES.md`, `PENDING_IDEAS.md`, `NAMING.md`) —
`scripts/start-dev.ps1` needed no change, it finds the backend project by
glob (`*.Api.csproj`), not by name.

## 2026-08-26 — Real database: MonsterASP.NET SQL Server, dropped local Postgres

- Created the real dev database on MonsterASP.NET's free plan: SQL Server
  2025, EU/Germany datacenter, Remote Access (SSMS) enabled for
  connections from outside their network. See
  [HOSTING.md](HOSTING.md#database-monsterasp.net-sql-server-2025-decided-2026-08-26)
  for the full picture (why SQL Server over MySQL, free-tier limits).
- Swapped `client-backend`'s EF Core provider: `Npgsql.EntityFrameworkCore.
  PostgreSQL` -> `Microsoft.EntityFrameworkCore.SqlServer`
  (`DependencyInjection.cs`: `UseNpgsql` -> `UseSqlServer`).
- Removed the local Postgres connection string from
  `appsettings.Development.json` (it's gone now — the real connection
  string lives only in `dotnet user-secrets`, never in a committed file).
- Ran `dotnet ef migrations add InitialCreate` (empty — no entities yet)
  and `dotnet ef database update` against the real remote database as an
  end-to-end connectivity test: it connected, created
  `__EFMigrationsHistory`, and recorded the migration. Confirms the full
  local-machine -> internet -> MonsterASP path works before any real
  domain model gets built on top of it.
- `scripts/start-dev.ps1` no longer auto-starts Docker/local Postgres —
  the local container is now an explicit offline-only fallback (see
  HOSTING.md), not part of the normal day-to-day flow.

## 2026-08-26 — Launcher follow-up: widget URL bug + real window cleanup

Two more real bugs found by actually using the launcher repeatedly (the
way it'd get used day to day), both now fixed and round-trip tested
(start -> stop -> start, three times, verified clean each time):

- **Widget opened broken.** The repo path contains a space
  (`PROYECTOS PERSONALES`), which wasn't percent-encoded in the
  `file://` URL passed to Edge's `--app=` flag — Windows' argument
  splitting cut the URL off at the space, so Edge opened
  `file:///D:/PROYECTOS` (nonexistent) instead of the widget. Every
  re-run of Start created another broken Edge window. Fixed: the URL is
  now percent-encoded, and opening the widget is now idempotent too
  (skipped if a `widget/index.html` Edge process is already found).
- **Stop didn't actually close the visible windows.** `taskkill /F /T` on
  the port-owning process (node.exe/dotnet.exe) kills that process and
  its descendants, but not its *parent* - so the wrapper PowerShell
  window (title "Project - client-frontend"/"-backend") was left behind,
  now just an idle empty prompt, indistinguishable from a live one at a
  glance. Compounded by testing start.bat repeatedly without stopping
  cleanly in between: the user ended up with a cluttered taskbar (several
  duplicate "Project - client-*" windows, several broken widget Edge
  windows) and reasonably asked "what do I close and what not?" Fixed:
  `stop-dev.ps1` now walks up the parent chain from the port-owner (past
  the intermediate cmd.exe npm spawns), specifically looking for the
  "Project - ..." wrapper by its command line, and kills from there - the
  window itself closes now, not just the process inside it. Also closes
  the widget window on stop.
- Manually cleaned up the mess left behind by the testing above (5+ stray
  windows, several broken Edge tabs) - confirmed nothing was left running
  outside of a fresh `start.bat` afterward.

## 2026-08-26 — Reload-safe launcher + icon

- Added `assets/app-icon.ico` — a small generated icon (brand green
  rounded square, "C1"), no download involved, so the Desktop shortcuts
  have something better than the default batch-file icon while there's no
  real logo yet.
- Added `scripts/start.bat` / `scripts/stop.bat` (thin wrappers around the
  existing `.ps1` scripts) and repointed the Desktop shortcuts at them
  ("Start Project" / "Stop Project"), with the new icon.
- Made `start-dev.ps1` safe to run repeatedly: it checks each dev-server
  port first and skips relaunching anything already up, instead of
  spawning duplicate windows. Combined with Vite/`dotnet watch`'s own
  hot-reload, the intent is: run Start once per session, then just save
  files — no more stop/start per change.
- Rewrote `stop-dev.ps1` to kill by **port ownership**
  (`Get-NetTCPConnection` + `taskkill /F /T`) instead of matching spawned
  windows by title — title matching turned out unreliable (a spawned
  console window's title wasn't reliably readable from outside it, so the
  old version silently did nothing to the frontend/backend processes).
  Verified: start -> stop -> start again all work cleanly now.
- Fixed a real bug found while testing the above: `start-dev.ps1`'s
  spawned backend window couldn't find `dotnet` at all (user-scoped PATH
  change not yet visible to that process tree) — see
  `docs/DEPENDENCIES.md` "Known PATH gotcha". The script now sets its own
  `$env:Path` defensively before spawning anything.
- Also cleaned up Docker Compose's stale project registration left over
  from the folder rename (it still pointed at the old `NutriBoost\`
  path) — recreated the container fresh under the `english-c1-platform`
  project name and removed the two orphaned volumes from the old name
  (both were empty dev databases, nothing lost).

## 2026-08-26 — Scope correction

The "Initial scaffold" entry below built the wrong project — a fitness
e-commerce storefront ("NutriBoost") instead of this English-practice
platform. Full explanation: [errors/2026-08-26-scope-mixup.md](errors/2026-08-26-scope-mixup.md).
Fixes made:

- Root folder renamed `NutriBoost` → `english-c1-platform` (a plain
  placeholder, not a proposed name — see [NAMING.md](NAMING.md)). Updated
  to match: `.claude/launch.json`, `scripts/dev-frontend.cmd`, the window
  titles in `scripts/start-dev.ps1`/`stop-dev.ps1`, and the two Desktop
  shortcuts (now "Project - Start" / "Project - Stop").
- Removed the e-commerce domain from `client-backend`: `Product` entity,
  `ProductsController`, `IProductRepository`/`ProductRepository`, and the
  `InitialCreate` migration (rolled back against the local database first,
  then deleted). `NutriBoostDbContext` renamed to `AppDbContext`, now with
  no entities — see `docs/DEPENDENCIES.md` "Removed".
- `docker-compose.yml`: container/env names genericized
  (`nutriboost-postgres` → `app-postgres`, `nutriboost`/`nutriboost_client`
  → `app`/`app_dev`), container recreated. Still on host port 5433 (that
  part of the earlier fix was unrelated to the scope mistake and stays).
- Frontend: removed e-commerce-specific nav items (Shop, Cart) and page
  copy; `i18n` `brand.name` set to an explicit `[Project name]` placeholder
  instead of a made-up brand; `index.html` title updated; the
  `localStorage` key in `ThemeProvider` and in `widget/index.html`
  genericized.
- Docs rewritten to describe the actual project and stop asserting
  decisions (hosting, naming, pending features) that belonged to the other
  project: `ARCHITECTURE.md`, `HOSTING.md`, `NAMING.md`, `PENDING_IDEAS.md`,
  `COLOR_PALETTE.md`, `GIT_WORKFLOW.md`, `AI_WORKFLOW.md`, this file,
  `README.md`.
- **Not yet fixed** (tracked in `PENDING_IDEAS.md`): the backend's C#
  project/namespace names are still `NutriBoost.Client.*`. Left alone
  deliberately — a full rename is better done once alongside the real
  domain model than as two separate mechanical passes.

## 2026-08-26 — Initial scaffold (wrong project — see correction above)

- Created the monorepo root `NutriBoost/` at
  `D:\PROYECTOS PERSONALES\NutriBoost` (previous working directory,
  `D:\download`, was Downloads — not a real project location).
- Added `docs/` with `ARCHITECTURE.md`, `DEPENDENCIES.md`, `HOSTING.md`,
  `COLOR_PALETTE.md`, `NAMING.md`, `AI_WORKFLOW.md`, `GIT_WORKFLOW.md`,
  `PENDING_IDEAS.md`, `errors/`.
- Scaffolded `client-frontend/`: Vite + React + TypeScript, Tailwind CSS v4,
  shadcn/ui ("Nova" preset — Radix + Lucide + Geist), `react-router-dom`,
  `motion`, `i18next` + `react-i18next`.
  - Built `Navbar` (responsive: horizontal nav ≥ md, `Sheet` drawer below),
    `Footer`, a placeholder `Home` page, and a `ThemeProvider` (light
    default, dark-mode CSS variables already wired but not yet exposed to
    users).
  - Chose a green brand accent (`--primary`/`--accent`/`--ring`, hue 150)
    over shadcn's default neutral-only palette — see
    [COLOR_PALETTE.md](COLOR_PALETTE.md).
- Scaffolded `client-backend/`: ASP.NET Core 8, layered as
  `NutriBoost.Client.sln` → `src/NutriBoost.Client.{Domain,Application,
  Infrastructure,Api}`, referenced Domain → Application → Infrastructure →
  Api per [ARCHITECTURE.md](ARCHITECTURE.md#backend-layered--clean-architecture).
  First vertical slice end-to-end: `Product` entity (Domain) →
  `IProductRepository` + `GetProductsQueryHandler` (Application) →
  `NutriBoostDbContext` + `ProductRepository` (Infrastructure, EF Core +
  Npgsql, PostgreSQL `xmin` used as the optimistic-concurrency token) →
  `ProductsController` (`GET /api/products`) + CORS for the Vite dev
  origin + `GET /health` (Api). Builds clean, 0 warnings. `InitialCreate`
  migration created and applied against local Postgres; `GET /health` and
  `GET /api/products` verified end-to-end against a real database (empty
  result, as expected — no seed data yet).
- Added `tests/NutriBoost.Client.Domain.Tests` (xUnit) with 5 passing tests
  covering `Product`'s validation — establishes the testing pattern; the
  other layers don't have tests yet.
- `docker-compose.yml` Postgres moved from host port 5432 to **5433** —
  5432 was already taken by an unrelated local Postgres service on this
  machine. See [errors/2026-08-26-postgres-port-conflict.md](errors/2026-08-26-postgres-port-conflict.md).
- `admin-frontend/`, `admin-backend/` **not yet scaffolded** — see
  [PENDING_IDEAS.md](PENDING_IDEAS.md).
- Root `docker-compose.yml` added for local Postgres. Docker Desktop was
  installed but not running — started it this session to run the migration
  and end-to-end check above; it does not auto-start on login by default,
  so start it manually (or via the Desktop shortcut) each time.
- `scripts/start-dev.ps1` / `scripts/stop-dev.ps1` plus two Desktop
  shortcuts added, per the notes' request to not have to reopen everything
  by hand each time.
- `widget/` added: a minimal local HTML/JS pending-vs-done tracker,
  minimizable, opened alongside the dev environment by `start-dev.ps1` —
  see its own README for why it's a lightweight web widget rather than a
  native tray app (scope tradeoff, revisit if it's not enough later).
