# Git workflow

## Branching

- `main` is always deployable / reviewed. Nothing gets pushed to it
  directly.
- Every module-level change goes on its own branch:
  `feature/<area>-<short-description>` (e.g. `feature/client-navbar`,
  `feature/backend-auth`), or `fix/<short-description>` for bug fixes.
- Open a PR into `main` when a branch is ready. Review it yourself (or with
  Fable 5 / a code-review skill pass) before merging — this is what gives
  you the "track and don't lose big changes" safety net the notes asked
  for, instead of squashing history away.

## Before every push — sensitive-data check

Ask explicitly, every time, before pushing: **is there anything sensitive
in this diff?**

- API keys, Stripe secret keys, database connection strings, JWT signing
  secrets, `.env` files → must never be committed. Use `dotnet user-secrets`
  for backend local secrets and a git-ignored `.env.local` for the
  frontend; see the root `.gitignore`.
- Real customer data (even test exports) → never committed.
- If something sensitive was already committed: don't just delete it in a
  new commit (it stays in history) — stop and rotate the credential, then
  ask how to scrub history if needed.

## Dev vs. prod credentials

Two separate credential sets from day one (Stripe test keys vs. live keys,
separate DB connection strings): a "developer" profile for local/staging
work and a "prod" profile for the real deployment, so testing never risks
touching production data or real charges. Keep both out of git regardless
of environment.
