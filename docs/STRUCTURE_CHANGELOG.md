# Structure changelog

Every new top-level folder, new app, or significant architectural change
gets an entry here, newest first — this is the traceability log the notes
asked for, separate from git history so it reads as a narrative instead of
a diff.

## 2026-08-28 — Speaking practice: AI-only roleplay, Cambridge-exam style

Fifth item from the 2026-08-28 request batch, and the first slice of the
biggest, least-scoped item in it ("situaciones para practicar con un
compañero... la IA ahí también puede entrar y darle qué caso literal como
si fuera a dar una prueba de Cambridge"). Deliberately split in two per
`PENDING_IDEAS.md`: this slice is AI-only roleplay; matching with another
real user is a separate, materially bigger feature (presence, pairing,
likely real-time) that needs its own design pass before it's buildable.

- New `Speaking` slice across all three backend layers, same shape as the
  existing AI features: `ISpeakingService`/`SpeakingDtos.cs`
  (Application), `SpeakingService`/`SpeakingScenario.cs` (Infrastructure),
  `SpeakingController` (Api). Registered in `DependencyInjection.cs` via
  `AddHttpClient<ISpeakingService, SpeakingService>`, matching
  `IAiInsightService`/`IAiQuestionGeneratorService`.
- Four fixed scenarios (`SpeakingScenarios.All`), loosely modeled on the
  four parts of a real Cambridge speaking exam - interview (B1+), long
  turn (B2+), collaborative task (B2+), abstract discussion (C1+). Each
  has its own system prompt instructing Groq to play the
  examiner/partner role and stay in character; the prompt never leaves
  the backend (`SpeakingScenarioDto` only exposes id/title/level/
  description).
- **Deliberately stateless for this first slice**: the full conversation
  history is sent by the client on every turn (`SpeakingReplyRequest.
  History`) instead of being persisted server-side - no new table, no
  attempt/session concept yet. `SpeakingService` caps what it forwards
  to Groq at the last 12 turns so a long session doesn't grow the prompt
  (and cost) without bound. Revisit persistence if this needs to survive
  a reload or show up in a student's history later.
- `GET /api/speaking/scenarios` and `POST /api/speaking/{scenarioId}/reply`,
  both `[Authorize]`-protected. A missing/invalid scenario id returns
  404; an unconfigured or failing Groq call returns 503 with a plain
  message, same pattern as `GroqInsightService`/
  `GroqQuestionGeneratorService` - never a generic 500 for "AI isn't
  available right now."
- Frontend: `core/speaking-api.ts` (thin wrapper, mirrors `test-api.ts`),
  new `pages/speaking/` component - a scenario-card picker that swaps to
  a chat view on pick. Picking a scenario sends a synthetic opening line
  ("Hi, I'm ready to start.") to get the examiner talking first, but
  that line is never shown as something the student typed - only the
  reply appears. New `/speaking` route (`authGuard`), new "Speaking" navbar
  link for authenticated users (between Dashboard and Admin).
- **Product decision, made live in chat, not guessed**: Listening's
  audio generation will use `edge-tts` (already installed locally,
  Python 3.11) instead of either Groq's TTS (blocked on terms
  acceptance) or ElevenLabs (not installed) - see the updated Listening
  entry in `PENDING_IDEAS.md`. Not built yet; recorded here because it
  changes that item from blocked to ready-to-build.

Verified end-to-end in the Browser pane with the real dev servers and a
real Groq call: picked the Interview scenario, got an opening question
("Can you tell me where you were born...") without a synthetic user
bubble showing, replied with a real answer, and got a natural,
context-aware follow-up question back (not a canned response) - confirms
history is actually being threaded through, not just the latest message.
"Change scenario" correctly resets the chat and returns to the picker. No
console errors. Backend: `dotnet build` 0 warnings/0 errors; domain unit
tests still 10/10 green (this slice added no new domain logic - the
scenario list and prompts are static data, nothing here needed a unit
test beyond the build/integration verification above). Frontend:
`ng build` clean (only the two pre-existing, already-accepted budget
warnings).

## 2026-08-28 — Reading comprehension: the first new skill beyond Grammar/Vocabulary

Fourth item from the 2026-08-28 request batch, and the first of the two
explicitly-named new skills (Reading and Listening).

- `SkillArea` gained a third value, `Reading`. Since the app already
  serializes enums as strings (`JsonStringEnumConverter`, set up
  2026-08-27) and EF Core stores it as an int appended at the end, this
  needed no migration of its own and didn't touch any existing stored
  `Grammar`/`Vocabulary` rows.
- `Question.Passage` (nullable, new migration `AddQuestionPassage`) - a
  short paragraph shown above the question, used only by Reading.
  Threaded through `QuestionDto`, `AdminQuestionDto`, and
  `UpsertQuestionRequest`.
- 16 new hand-written questions (`ReadingBank`, `QuestionSeeder.cs`) - 4
  per CEFR level, each a short passage plus one comprehension question.
  Passages grow in register with level: simple present-tense narration
  at A2, up to dense academic-argument prose at C1. Kept as a *separate*
  array from the existing `Bank` rather than adding a `Passage` element
  to `Bank`'s tuple shape - that would have meant appending a trailing
  `null` to all 64 existing entries for a field only Reading uses.
- Frontend: `Quiz` renders the passage above the question text when
  present (own card style, `quiz-question__passage`). Admin's question
  editor dialog shows a Passage field that appears/disappears reactively
  as the Skill dropdown changes, and validates it's required (frontend
  and backend) when the skill is Reading. New skill icon
  (`skill-icons.ts`) - a document-with-lines glyph, distinct from
  Grammar's open book.
- **The placement test grew from 32 to 48 questions as a direct,
  intended consequence** - `GetPlacementQuestionsAsync` groups by
  `(Level, SkillArea)` and was already skill-agnostic, so it picked up
  the new cells automatically once seeded. Unlike the earlier
  question-bank-doubling entry (deliberately kept the test at a fixed
  length, since that was the *same* skills getting a bigger pool),
  this is a genuinely new skill joining the platform's assessed scope -
  a CEFR placement that ignored Reading entirely wouldn't reflect
  reading ability at all. Updated the "32 questions... ~15 minutes"
  intro copy to "48 questions... ~20 minutes" to match. About page's
  "Scope, honestly" section updated too (Reading dropped from the
  not-yet-assessed list; also corrected the now-stale "hand-written, not
  machine-generated" claim now that AI-generated reinforcement exists).
- **Known gap, not silently shipped**: the AI-generated-reinforcement
  feature (previous changelog entry) doesn't produce passages yet -
  `GroqQuestionGeneratorService`'s prompt only asks for a bare MCQ. Hid
  the "Practice different questions (AI-generated)" button specifically
  for Reading reinforcement rather than let it generate a passage-less
  "reading" question that wouldn't actually test reading comprehension.

Verified end-to-end for real: confirmed via the API that the placement
endpoint returns exactly 48 questions (32 with `passage: null`, 16 with
real passage text); loaded the actual placement test in the browser and
saw a passage rendered inside a question card; created a real Reading
question with a passage through the admin Content tab (count went
84→85), confirmed it saved and deleted it again; confirmed the
reinforcement page for `A2/Reading` shows 4 questions each with a
passage and no AI-generate button. Migration applied to the real shared
database. Backend build + 10/10 unit tests, frontend build, both clean.

## 2026-08-28 — Tutor-student assignment, with a seeded "AI Tutor" persona

Third item from the 2026-08-28 request batch. A student can now have an
assigned tutor, surfaced on their Dashboard.

- `ApplicationUser` gained `DisplayName` (nullable, for a friendlier
  label than a raw email) and a self-referencing optional `TutorId` -
  one tutor per student at a time, not a many-to-many junction table.
  Deliberately the simplest model that satisfies "Your tutor: X" - no
  co-tutoring use case exists yet to justify more. Migration
  `AddTutorAssignment`; the self-referencing FK uses
  `DeleteBehavior.Restrict` (SQL Server flatly rejects a cascading
  self-reference, and there's no delete-account feature yet for this to
  matter in practice).
- **Seeded "AI Tutor" persona** (`ai-tutor@certis.local`, `DisplayName =
  "AI Tutor"`, Tutor role) - explicitly requested: a real, assignable
  tutor without needing an actual human yet. It's a genuine
  `ApplicationUser` (reuses every bit of existing Tutor-role plumbing
  instead of special-casing "AI tutor" vs "real tutor" everywhere) but
  seeded with a random password nobody is ever given, so nothing can
  sign into it. Idempotent, looked up by its fixed email every startup.
  Also does the one-off assignment that was explicitly asked for: the
  requesting account (`Admin:Email`) gets this persona as its tutor if
  it doesn't have one yet.
- `AccountsController` gained `PUT /api/admin/accounts/{userId}/tutor`
  (Admin-only, validates the target actually holds the Tutor role,
  rejects self-assignment) and `GetAccounts`/`SetRoles` now resolve and
  return `tutorId`/`tutorLabel` per account.
- `/api/auth/login`, `/register`, and `/me` all now return `tutorLabel`
  (display name, falling back to email) - same pattern already
  established for `isAdmin`/`isTutor`, refreshed at login like those.
- Frontend: Admin Access tab gets a per-account tutor `<mat-select>`
  (options = accounts with the Tutor role, "No tutor" to clear).
  Dashboard shows a "Your tutor: X" pill at the top when one's assigned.

**A real bug caught and fixed before it shipped, not after**: initially
had `AccountsController.SetRoles` hardcode `tutorLabel: null` in its
response instead of resolving it - toggling Admin/Tutor for an account
that already had a tutor assigned would have silently blanked the label
in the UI on the next refresh. Pulled the resolution into a shared
`ResolveTutorLabel` helper used by both `SetRoles` and `Me()` instead.

**Another `dotnet ef` PATH gotcha** (same root cause as the previous
entry, worth confirming the fix generalizes): `PATH="/c/Users/pc/.dotnet:$PATH"`
prefixing worked again for both `migrations add` and `database update`.

Verified end-to-end for real: confirmed via the API that the AI Tutor
persona was seeded and `anthonysosa44@gmail.com` got auto-assigned to it
on startup; assigned a tutor to a test account through the actual Access
tab UI (`PUT .../tutor` → 200); logged out and back in and confirmed the
login response carried `tutorLabel: "AI Tutor"`; confirmed the Dashboard
rendered "Your tutor: AI Tutor" at the top. Migration applied to the
real shared database. Backend build + 10/10 unit tests, frontend build,
both clean.

## 2026-08-28 — AI-generated reinforcement practice (Groq)

Second item from the 2026-08-28 request batch. Lets a student practice a
weak (level, skill) area with freshly-generated questions instead of only
the fixed hand-written bank.

- `Question.IsAiGenerated` (new column, migration
  `AddQuestionIsAiGenerated`, `HasDefaultValue(false)` so the existing 64
  rows didn't need a separate backfill) marks which questions came from
  Groq vs the seeded bank. Everything else about an AI-generated question
  - grading, sampling, reinforcement pooling - works identically, since
  it's persisted as a real `Question` with a real `CorrectOptionId`, not
  a special-cased shape.
- `IAiQuestionGeneratorService` / `GroqQuestionGeneratorService`: calls
  Groq with `response_format: {"type": "json_object"}` - structured JSON
  output, confirmed against a real request before writing any C# (see
  the raw curl test in this session) - far more reliable than asking a
  reasoning model to "reply with JSON" in free text and hoping it isn't
  wrapped in markdown fences or padded with commentary. Validates the
  parsed result (exactly 4 options, index in range, non-empty fields)
  and retries once before giving up. The prompt is given the existing
  bank's question texts for that cell (plus this batch's own so-far
  generations) so it steers away from near-duplicates.
- New endpoint `POST /api/test/reinforcement/{level}/{skill}/generate`
  (count capped 1-6, defaults to 4 - each one is a real Groq call, not
  free). Returns 503 if literally zero generations came through; a
  partial batch (e.g. 3 of 4) still returns what succeeded rather than
  failing the whole request over one bad roll.
- Frontend: a "Practice different questions (AI-generated)" button on
  the reinforcement page swaps the fixed bank for a freshly-generated
  set on the same (level, skill), running through the exact same `Quiz`
  component unmodified. An "AI-generated practice" badge replaces the
  button once active. Admin Content tab shows a small "AI" badge per
  question so it's clear which are Groq-generated vs hand-written.

**A real PATH gotcha hit again, different symptom this time**: `dotnet
ef migrations add` failed with "No .NET SDKs were found" / "the
application 'msbuild' does not exist" even when invoked via the full
path to the per-user SDK's `dotnet.exe` - because `dotnet-ef` itself
shells out to a *child* `dotnet` process, and that child resolves
through PATH, which still has the machine-wide runtime-only install
first (see the earlier dotnet-SDK-path memory/feedback). Fixed by
prefixing PATH for just that command:
`PATH="/c/Users/pc/.dotnet:$PATH" dotnet ef migrations add ...` - calling
the full path alone wasn't enough this time since the *tool*, not my own
command, is what re-resolves `dotnet` internally.

Verified end-to-end for real, not just that it compiles: generated 4 new
A2 Grammar questions live via the actual UI button (all genuinely new,
distinct from the 8 already in that cell), confirmed the question bank
grew from 64 to 68 in the admin Content tab with the "AI" badge showing
on exactly those 4, and submitted a real answer set against them -
graded correctly (`score: 1, total: 4, grade: 2.5`) through the same
path as any other reinforcement attempt. Migration applied to the real
shared database (not just described). Backend build + 10/10 unit tests,
frontend build, both clean.

## 2026-08-28 — School-style 0-10 grading; pass threshold raised to 7/10

Fourth `/loop` batch, first item from the 2026-08-28 request (AI
exercises, new skills, tutor assignment - see the changelog entry for
the whole list). Picked this one first since several of the others
(AI-generated exercises, new skills) would otherwise need touching the
same scoring code again later.

Explicit decision from the user: base the early-warning system on a
school-style grade, below 7/10 gets priority - not the previous 60% pass
threshold.

- `PlacementScorer.PassThreshold`: `0.6` → `0.7` (single source of truth
  already - `AdminService`'s early-warning check reads this constant, so
  it picked up the new bar for free).
- `SkillBreakdown` (domain) gained a computed `Grade` property
  (`Correct/Total * 10`, rounded to 1 decimal) - threaded through
  `SkillBreakdownDto` and a new top-level `TestResultDto.Grade` so a
  grade is available per skill area *and* for the whole attempt.
- Frontend: `core/grading.ts` is the new single source for
  `PASS_THRESHOLD` and a `passed()` helper - pulled out because the old
  `0.6` was hardcoded in three places (`dashboard.ts`, `reinforcement.ts`,
  and implicitly the backend). Test-result breakdown cards now show an
  "X.X/10" grade badge next to each skill area, color-coded by level.
  About/Home copy that said "60%" now says "7/10" instead.
- Unit tests: renamed `SixtyPercentThreshold_IsAppliedPerCell` →
  `SeventyPercentThreshold_IsAppliedPerCell` (assertions didn't need to
  change - the 4-question test cells' fractions all fall clearly on one
  side of both 60% and 70%), added `Grade_IsOutOfTenRoundedToOneDecimal`.

Verified in-browser: breakdown cards render the grade badges correctly
(e.g. "5/10", "2.5/10"); About page reads "7/10" not "60%"; a real
reinforcement submit came back with `grade: 3.8` for `3/8` correct
(37.5%, correctly rounded) and `needsReinforcement: true` (37.5% < 70%).
Backend unit tests: 10/10 green (6 original + 4 new).

## 2026-08-27 — Toasts for lightweight confirmations (and another stale-docs fix)

Third autonomous `/loop` iteration through `PENDING_IDEAS.md`.

**Another stale line found**: "Dark mode toggle" had been sitting in the
open list since the Angular switch, but it's been fully built and in
active use the entire time (every dark-mode screenshot in this file used
it) - moved to Done with a note instead of redone.

**Real work**: `core/toast.ts` - a thin `Toast` service wrapping
`MatSnackBar`, styled in `styles.scss` to match the app's pill/rounded
language (`.app-toast*` classes) instead of Material's default squared
snackbar. `success()` uses the same green as a passed CEFR cell;
`error()` uses the Material system error color, both theme-aware without
a separate dark-mode override.

Wired into the three places in the app that already had a real
"lightweight confirmation" gap - actions that succeeded with zero
feedback beyond the underlying state changing:
- Admin Content tab: "Question added" / "Question updated" / "Question
  deleted" after the question-editor dialog or a delete.
- Admin Access tab: "Access updated for {email}" after a role toggle
  succeeds.

Deliberately did not touch anything already showing inline
success/error state (test submission, AI insight, etc.) - the pending
item's own framing was "modals reserved for anything more consequential",
which by extension means toasts are for gaps, not for wrapping feedback
that already exists.

Verified in-browser in both dark and light mode: added a real question
through the Content tab and watched the green "Question added" toast
render correctly (pill shape, right color, right message), confirmed the
count updated, deleted it and confirmed the count reverted, and confirmed
a role toggle round-trips successfully (network 200) through the same
code path. Frontend build clean.

## 2026-08-27 — Real auto-resume for the placement test (and a stale-docs fix)

Second autonomous `/loop` iteration through `PENDING_IDEAS.md`.

**Found already done, not built**: "How it works / methodology content
on the About page" turned out to be stale - the earlier About redesign
(icon cards, commit `d6f4a24`) already replaced the "Coming soon"
placeholder with real methodology content. No code changed; just moved
the line to Done with a note about why, instead of redoing work that
already shipped.

**Real work this iteration**: auto-resume, the last item in "Flagged for
a future design pass" that didn't actually need a product decision - the
direction was already fully specified ("answers and timer are preserved,
but the user has to click Retake the test once to see them restored").

- `shared/quiz/quiz.ts`'s persisted state now also stores the exact
  `questions` array shown for the attempt, not just selections/timer.
  Exported a new `readPersistedQuiz(storageKey)` helper so a parent page
  can peek at an in-progress attempt *before* Quiz ever mounts.
- `placement-test.ts`'s `ngOnInit` now calls that helper: if there's a
  persisted attempt with at least one answer, it restores the exact
  question set and jumps straight to the `taking` stage - skipping the
  intro screen and the "Retake the test" click entirely. A draft with no
  answers yet still shows the normal intro (a blank in-progress quiz
  isn't worth skipping the intro for).
- **This wasn't just a UX nicety - it fixed a real bug the previous
  iteration introduced.** Once `GetPlacementQuestionsAsync` started
  sampling 4-of-8 questions per cell (see the question-bank-doubling
  entry below), a reload that re-fetched fresh questions from the API
  could silently get a *different* random sample than the one the
  answers were keyed against - orphaning progress with no error, just a
  quiz that looked reset. Persisting and reusing the exact original
  question set fixes both problems with the same change.

Verified in-browser end-to-end: started a fresh attempt, answered two
questions (confirmed via `localStorage` which exact questions/options),
hard-reloaded `/test`, and confirmed it landed straight in the quiz with
the same 32 questions, the same 2 answers still visually selected on the
right questions, and the elapsed timer continuing (not reset). Confirmed
the normal intro screen still shows correctly when there's no in-progress
draft. Frontend build clean.

## 2026-08-27 — Doubled the question bank (32 → 64); placement stays 32 via sampling

First autonomous iteration of a `/loop`-scheduled pass through
`PENDING_IDEAS.md` (every 10 minutes, while the user was away) - picked
"Bigger question bank... thin for anything beyond a first estimate" as
the first unblocked item.

- 32 new hand-written questions added to `QuestionSeeder.Bank` - 4 more
  per (level, skill) cell, same style/quality bar as the original batch
  (a short rule-based `Explanation` on every one). Bank is now 8
  questions per cell instead of 4.
- `SeedAsync` changed from "insert only if the table is empty" to
  "insert whatever Bank entries aren't in the database yet, matched by
  Text" - the same match-by-Text approach `BackfillExplanationsAsync`
  already used, so the second batch could be added without clearing and
  re-seeding (which would have orphaned every `TestAttempt.QuestionId`
  already recorded, in dev *and* production - same DB).
- **Deliberately did not let the placement test grow to 64 questions.**
  A longer bank buys variety and a bigger reinforcement pool, not a
  longer test - doubling everyone's placement test from ~15 to ~30
  minutes as a side effect of a content addition felt like a real UX
  regression nobody asked for. Instead, `GetPlacementQuestionsAsync` now
  randomly samples 4 of the available questions per cell each time
  (`PlacementQuestionsPerCell = 4`), so the test stays a fixed 32
  questions but two attempts (or two different people) won't always see
  the exact same set. Reinforcement quizzes are unaffected - they still
  show every question in that cell (now 8 instead of 4), which is a
  pure improvement there since retrying is already expected to vary.
- **Real latent bug fixed while wiring the sampling in**: `Grade()`'s
  `TotalQuestions` was set from `questions.Count` - the full pool the
  caller loaded to grade against - not from how many questions the
  test-taker was actually shown and answered. This only "worked" before
  by coincidence (`GetPlacementQuestionsAsync` returned literally
  everything in the bank, so the two counts always matched). The moment
  placement started sampling a subset, this would have shown "8/64"
  instead of "8/32" on the result page. Fixed to use `testAnswers.Count`
  instead - also more correct for reinforcement if a submitted answer
  ever fails to resolve to a real question.

Verified via the API (not just reading the code): three separate calls
to `/api/test/placement/questions` all returned exactly 32; two
back-to-back calls overlapped on only 15 of 32 questions (real
variety, roughly what you'd expect sampling 4-of-8 twice); a full
submit round-trip correctly reported `totalQuestions: 32`. Confirmed
in-browser too - the placement intro page's "32 multiple-choice
questions" copy is still accurate, no text needed to change. Backend
unit tests (6/6) still green.

## 2026-08-27 — Admin/Tutor panel (`/admin`), question-bank CRUD, and an AI-generated personalized insight

Built while the user was away, following an explicit "sigue con todo" +
follow-up scope request: the account/performance panel from the original
notes ("una cuenta que pueda ver los rendimientos de los estudiantes...
alertas tempranas"), then a follow-up mid-session message asked for
**tutors** too, with the ability to view/edit and "crear módulos" (author
question content) - not just admins.

**Roles.** Added real ASP.NET Core Identity roles - `Admin` and `Tutor`
- on top of the `IdentityDbContext<..., IdentityRole<Guid>, Guid>` that
was already wired up (the role tables existed, just unused). Both roles
are seeded at startup if missing. There's deliberately no self-service
"become an admin" path: `Admin:Email` (a user-secret/app-setting, unset
by default) auto-promotes exactly one configured account on startup, and
from then on only an existing Admin can grant Admin or Tutor to anyone
else, via the new Access tab. JWTs now carry a `role` claim per role, and
`/api/auth/login`+`register` return `isAdmin`/`isTutor` so the frontend
doesn't have to decode the token to know what to show.

**`/admin` page**, guarded by a new `adminGuard` (signed-in and
Admin-or-Tutor, else redirected home), three tabs:
- **Students** - the read-only performance overview from the original
  ask, reusing `AdminService.GetStudentSummariesAsync()`: latest
  placement level, attempt count, and which (level, skill) areas are
  still weak - "weak" here specifically means the latest placement
  flagged it *and* no passing reinforcement attempt for that exact cell
  has landed since. Sorted early-warning-first.
- **Content** - question-bank CRUD (`ContentController`, `IContentService`
  Admin+Tutor authorized). A "module" is a (Level, SkillArea) cell;
  questions inside one can be added/edited/deleted through a
  `MatDialog`-based editor (radio-select the correct option, add/remove
  options, edit the explanation) instead of only ever coming from
  `QuestionSeeder`. This is now genuinely how you'd grow the question
  bank past the original 32, not just how it started.
- **Access** (Admin-only tab, hidden for Tutors) - toggle Admin/Tutor per
  registered account. Server-side guard against an admin removing their
  *own* Admin role (a real lockout risk on a 2-3 person platform with no
  recovery path) - returns a specific 400 message the UI surfaces inline
  without hiding the rest of the list.

**Real bug caught during verification, not before:** editing a question
through the Content tab 500'd every time with a
`DbUpdateConcurrencyException` ("expected 1 row, affected 0"). Root
cause: assigning a `List<QuestionOption>` of freshly-constructed entities
(client-generated Guid keys) to an already-tracked parent's navigation
property does **not** mark them `Added` in EF Core - because the key is
already non-default, EF's graph fixup assumes they exist and queues
no-op `UPDATE`s instead of `INSERT`s. Fixed with an explicit
`db.QuestionOptions.AddRange(newOptions)` alongside the navigation
assignment - see the comment in `ContentService.cs`. Caught by actually
clicking Edit → Save in the browser, not by reading the code.

**AI insight (Groq).** Where the Groq API key lives now:
`Groq:ApiKey` via `dotnet user-secrets` locally / an app setting in
production - same pattern as `Jwt:SigningKey`. `GroqOptions` was already
scaffolded as an empty slot in an earlier session specifically so this
didn't need new wiring once a real feature was defined. The feature: a
"Get AI feedback on this attempt" button on the test-result page
(on-demand only - never generated automatically, since it's a real
network call with real cost) that sends the attempt's skill breakdown
and missed questions to Groq and shows back a short personalized
diagnostic - the *why* behind the pattern of mistakes, not just the
score, which is what makes this "early warning" personalized instead of
the Students tab's generic threshold flag. New endpoint
`POST /api/test/results/{attemptId}/insight`; returns 503 (not 500) when
unconfigured or the upstream call fails, which the UI shows as "isn't
set up yet" rather than an error.

Two more real bugs found only by testing this end-to-end against the
live Groq API, not by reading the code:
- `llama-3.3-70b-versatile` (the obvious model choice) had already been
  retired from Groq's lineup - a 404. Switched to `openai/gpt-oss-20b`
  after confirming it's live via `GET /openai/v1/models`.
- Even after that, responses came back with empty `content` and
  `finish_reason: "length"`. `gpt-oss-20b` is a *reasoning* model - it
  spends completion tokens on hidden chain-of-thought before the actual
  answer, and the initial 220-token budget was going almost entirely to
  reasoning (218 of 220 tokens, confirmed via a raw curl call). Fixed by
  setting `reasoning_effort: "low"` (drops reasoning to single digits of
  tokens) and raising the budget to 350 tokens of headroom. Separately,
  the response DTOs weren't deserializing at all before this - System.Text.Json
  is case-sensitive by default and Groq returns lowercase JSON keys;
  fixed with `PropertyNameCaseInsensitive = true` on the read side (the
  request side had been working only because Groq's parser happens to be
  lenient about casing on the way in).

Verified in-browser end-to-end with a throwaway admin test account:
Students/Content/Access tabs, create/edit/delete a question, grant and
revoke Tutor, the self-demotion guard, and a real AI insight rendering
from the live Groq API. Backend unit tests (6/6) still green.

## 2026-08-27 — Student dashboard (`/dashboard`)

Self-directed next step after "seguir en local, algo nuevo" - the natural
continuation of the earlier request to have a student panel showing
grades/what's approved "como un instituto". Self-contained (no admin
roles, no new external dependency), and lays groundwork the admin panel
will reuse later.

- Backend: `ITestService.GetHistoryAsync(userId)` returns every completed
  `TestAttempt` (placement and reinforcement) for the user, newest first,
  reusing the existing `ToResultDto`/`BuildBreakdown`/`BuildMissedQuestions`
  pipeline instead of a parallel one. Batches the question lookup across
  all attempts (one `Questions` query, not N) to avoid N+1. New endpoint
  `GET /api/test/results/history`.
- Frontend: new `pages/dashboard/` page, routed at `/dashboard` behind
  `authGuard`. Splits history into two lists - Placement (level badge,
  date, score) and Reinforcement (skill icon, level+skill, pass/fail
  chip, date, score) - with an empty state pointing at the placement
  test when a user has no attempts yet.
- Pulled `LEVEL_CSS_VAR`/`LEVEL_NAME`/`SKILL_ICON_PATH` out of
  `test-result.ts` into shared `core/cefr.ts` and `core/skill-icons.ts`
  (this was the third place that needed the same level-color/name and
  skill-icon mapping, after `home.ts` and `test-result.ts` - not worth
  copy-pasting again). `test-result.ts` now imports from both instead of
  keeping its own copies.
- Navbar shows a "Dashboard" link only when signed in (`navItems` is now
  a `computed()` off `Auth.isAuthenticated()` instead of a static array).
- Verified in-browser end-to-end with a throwaway test account: empty
  state, populated placement history, populated reinforcement history
  (pass/fail chip), and dark mode - no overflow or contrast issues.

## 2026-08-27 — Picked the Open Book model; moved it into the hero itself

Follow-up to the entry right below: the Open Book was picked from the
three candidates. Two changes from that:

- Moved the model from its own section below "How it works" into the
  hero itself - `home__hero-row` is now a flex row with the headline/
  copy/CTA on the left and the rotating book on the right, filling the
  empty space next to the H1 instead of being buried below three other
  sections. Removed the prev/next/dot switcher entirely (`Home` no
  longer has `activeModelIndex`/`previousModel`/`nextModel`/
  `selectModel` - just a single fixed `heroModel`).
- `grad-cap.glb` and `globe.glb` stay in `public/models/` on purpose -
  not dead files, reserved for another spot or a loading screen later.
- Verified in a fresh browser tab (a stale tab from earlier in the
  session - degraded by the Poly Pizza download crashes noted below -
  gave a false "won't load" reading first; a new tab loaded it
  correctly, confirming that was tab state, not a real bug): the model
  loads, canvas renders real content, and the hero row wraps sanely on
  mobile (text first, book below).

## 2026-08-27 — 3D model picker in the empty Home hero space

The empty space below "How it works" on Home now holds a real,
interactive 3D preview - `<model-viewer>` (`@google/model-viewer`) with
prev/next arrows and dots, cycling through three candidate models so
the final pick can be made by looking at them live on the actual page
instead of on Poly Pizza's site. Not a final choice yet - `home.ts`'s
`MODEL_OPTIONS` array is deliberately still a list of three; trim it to
one (or restyle around the winner) once picked.

- Three CC0 (public domain) low-poly models downloaded from Poly Pizza
  and committed to `public/models/`: `open-book.glb` (35KB),
  `grad-cap.glb` (9KB), `globe.glb` (81KB) - all by Quaternius except
  the globe. Poly Pizza's own "Download" button triggers a real browser
  file download that the sandboxed browser tool used this session
  couldn't handle (crashed the tab twice) - worked around it by reading
  each model page's embedded `<model-viewer src="...">` tag for the
  direct `static.poly.pizza/*.glb` URL and fetching that with `curl`
  instead.
- `@google/model-viewer` is a heavy dependency (~900KB, bundles its own
  three.js-based renderer) - a static top-level import put that weight
  in the app's MAIN bundle, loaded on every single page, and pushed the
  initial bundle over Angular's 1MB hard error budget. Fixed with a
  dynamic `import('@google/model-viewer')` inside `Home.ngOnInit()`
  instead - Angular code-splits that into its own lazy chunk, fetched
  only when the Home page actually renders.
- Verified: the model loads (network request 200s, `model-viewer`'s own
  `loaded`/`modelIsVisible` flags true, canvas has real non-blank pixel
  data), and the prev/next/dot controls correctly swap the `src`
  attribute and label. Couldn't get a visual screenshot of the WebGL
  canvas itself through this session's screenshot tool (a tooling
  limitation, not a rendering bug) - confirmed via the canvas's own
  `toDataURL()` output instead.

## 2026-08-27 — About page redesigned: icon cards, scroll-reveal animation

Was a static stack of plain text sections - "métele más diseño, bloques
animados" was the ask. Rebuilt `pages/about/`:

- Hero with an eyebrow badge ("Methodology"), matching the pattern
  already used on Home.
- Each of the four sections (Format, How the level is calculated,
  Reinforcement, Scope honestly) is now an icon card - a distinct inline
  SVG per section instead of a bare heading, in the same card language as
  the placement-test breakdown cells.
- Real scroll animation, not just an on-load fade: `About` uses
  `IntersectionObserver` (`ngAfterViewInit`, `@ViewChildren('revealItem')`)
  to add an `is-visible` class as each block enters the viewport, with a
  small stagger per card (`nth-of-type` transition-delay) so they don't
  all pop in at once. Respects `prefers-reduced-motion: reduce`.
- Verified in-browser: hero animates in immediately, cards animate in as
  the page is scrolled.

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
