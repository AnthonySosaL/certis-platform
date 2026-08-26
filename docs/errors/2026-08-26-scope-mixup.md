# Built the wrong project: an e-commerce reference doc got treated as the spec

**Date:** 2026-08-26
**Area:** whole project (scope/planning, not code)

## Symptom

An entire fitness-nutrition e-commerce storefront ("NutriBoost") got
scaffolded — domain entities, e-commerce copy, hosting research framed
around "a real Ecuador storefront," three brand-name candidates for a
supplement shop — none of which is the actual project.

## Root cause

The request included a `.txt` file of 39 numbered notes describing an
e-commerce platform's architecture and methodology (shadcn, layered
backend, admin/client isolation, Stripe, Ecuador invoicing law, stock
concurrency, etc.). The instruction was to take the *methodology and
architecture patterns* from those notes into account for a **different**
project — a platform to practice and evaluate English toward Cambridge
C1 — not to build the e-commerce platform the notes describe. That other
project (matching the notes closely: admin/client isolation, its own
Docker Postgres, etc.) already exists elsewhere on this machine.

The request also said, of C#: "he visto que mucho usan eso" (seen a lot of
people use it) and specifically "es para aprender ingles" — the C# choice
was for **this** project, for learning purposes, not inherited from the
e-commerce notes. That distinction got lost, and the notes' *domain*
(products, stock, checkout) got built as if it were the target, not just
its *patterns* (layered architecture, docs-driven traceability, git
workflow, dev tooling).

## Fix

- Removed the e-commerce domain: `Product` entity, `ProductsController`,
  `IProductRepository`/`ProductRepository`, the `InitialCreate` migration
  (rolled back and dropped from the database).
- Stripped "NutriBoost" branding: navbar copy, page copy, i18n strings,
  `index.html` title, the Postgres container/env names in
  `docker-compose.yml`, the root project folder itself (renamed
  `NutriBoost` → `english-c1-platform`, with the Desktop shortcuts,
  `.claude/launch.json`, and dev scripts updated to match).
- Kept everything that's genuinely reusable methodology, not tied to the
  e-commerce domain: the C# .NET + React + shadcn stack, the layered
  backend architecture, the docs-driven traceability system itself (this
  file included), the git branching workflow, the dev environment tooling
  (Desktop shortcuts, task widget, Docker Postgres).
- Left as a known holdover, not yet fixed: the backend's C# namespaces and
  project files are still named `NutriBoost.Client.*`. Rewriting every
  `.csproj`/`.sln`/`namespace` is a bigger mechanical pass, better done
  once alongside adding the real domain model (once the feature scope is
  decided) than twice.

## How to avoid it again

When a request says "take patterns from this other project's notes into
account" for building project B, treat the notes as **reference
material for methodology**, not as project B's literal spec — especially
if the notes describe a different domain (e-commerce vs. an unrelated
tool) than what was just described as the actual goal. If a `.txt`/notes
file's subject matter doesn't match the stated project's subject matter,
that mismatch is the signal to ask before scaffolding a full domain model
around it, not to plow ahead and take "most of it into account."
