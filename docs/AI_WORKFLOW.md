# AI workflow — how this actually maps to the 3-brain plan

The original notes describe three "brains": your own notes, a "Fable 5"
brain for ideas/audits, and an "Opus 5" brain for coding guidance, with
"Sonnet 4.6" doing the actual coding and "Sonnet 5" only for touch-up edits.
Two corrections, so the plan stays workable instead of quietly failing later:

1. **There is no "Sonnet 4.6" model.** The Claude model lineup available
   right now is Fable 5, Opus 5, Sonnet 5, and Haiku 4.5. This session (the
   one that scaffolded this repo) is **Sonnet 5**, not Fable 5 — Claude Code
   sessions don't self-identify as a specific brain, they run whatever model
   you pick for that chat/session. There's no separate "4.6" coding-specific
   model to route to.
2. **This session is not "Fable 5-only."** The instruction "este chat no se
   usa mas que fable 5" doesn't apply here — this chat did the actual
   scaffolding (folders, code, config), which is exactly the kind of task
   the original notes wanted reserved for a coding-focused session.

## A version that keeps the spirit of the plan

- **Coding work** (this repo, features, fixes): use a **Sonnet 5** session —
  that's what actually writes and edits code well. This chat is one example.
- **Ideation / high-stakes review** (e.g. how AI-graded writing/speaking
  should work, privacy for two people's practice data, anything that needs
  real thought before code gets written): open a **separate chat on
  Fable 5** and paste in the relevant `docs/*.md` file
  for context. Save its output back into a numbered doc under `docs/ideas/`
  (see the note in [PENDING_IDEAS.md](PENDING_IDEAS.md)) rather than pasting
  the conversation back into the coding chat — that keeps the coding
  session's context focused on code, per the original goal of not
  "saturating" it.
- **Third brain**: instead of a separate "Opus 5 coding-patterns brain," use
  the `anthropic-skills:dev-engineering-rules` skill (already active in this
  session — see the rules it applied while building this scaffold: protect
  existing logic, one layer per change, reuse before creating, 200-line file
  cap, surgical edits only). That's a durable, reusable substitute for "a
  brain that tells the coder how to write good code" — it applies
  automatically to every coding session rather than living in a chat you
  have to remember to open.

## Practical rule going forward

Before asking a Fable 5 chat for an idea, skim `docs/ARCHITECTURE.md` and
`docs/PENDING_IDEAS.md` first so you're not re-deciding something already
settled. After it responds, the coding session's job is to implement the
decision and log it (`STRUCTURE_CHANGELOG.md`, `DEPENDENCIES.md`, or a new
`docs/ideas/NN-topic.md`, depending on what changed) — not to re-litigate it.

## Reference material vs. the spec

When a request says "take patterns from this other project's notes into
account" for building a *different* project, the notes are reference
material for methodology (architecture, tooling, workflow), not the new
project's literal spec — see
[errors/2026-08-26-scope-mixup.md](errors/2026-08-26-scope-mixup.md) for
what happened when that distinction got lost (an entire e-commerce
platform got scaffolded instead of this English-practice tool). If a
notes file's subject matter doesn't match the project just described,
that mismatch is worth flagging before building a full domain model
around it.
