# Project naming

**Correction (2026-08-26):** the three names previously listed here
(ThriveCrate, StackWell, Fuelance) were for a fitness e-commerce project
that isn't this project — see
[errors/2026-08-26-scope-mixup.md](errors/2026-08-26-scope-mixup.md).
Discard them; they have nothing to do with this platform.

The folder is currently named `english-c1-platform` — a plain descriptive
placeholder, not a proposed brand name, chosen so the folder path itself
stops being actively wrong while a real name gets picked.

## Tentative candidates (light research only — confirm before committing)

Quick web searches only, no trademark/domain search. Offered as a
starting point for discussion, not a recommendation to act on immediately
— last time inventing branding ahead of the actual scope caused a lot of
rework.

1. **Fluentrack** — no exact collisions found. Close neighbors exist
   ("Fluently," "FluentU," both AI conversation-practice apps), so the
   full word is what protects it, not "fluent" alone.
2. **C1 Compass** — no exact collisions found. Reads clearly as
   Cambridge-C1-focused, which helps if this stays scoped to that exam
   specifically.
3. **LexiTrack** — no exact collisions found ("Lexiplore" is a similar
   neighbor, a different app).

## Recommendation

Hold off deciding until the feature scope (see
[PENDING_IDEAS.md](PENDING_IDEAS.md)) is clearer — a name that's
C1/Cambridge-specific (like *C1 Compass*) is a poor fit if this becomes
broader multi-level practice later, while a generic one (*Fluentrack*)
ages better if the scope grows. Once picked, do a real domain/trademark
check before committing, and then the codebase needs a rename pass:
folder name, `i18n` `brand.name` keys, `index.html` title, and the
backend's `EnglishC1.Client.*` namespaces (currently a descriptive
placeholder, not the wrong-project holdover it used to be — see
[STRUCTURE_CHANGELOG.md](STRUCTURE_CHANGELOG.md) — so this last rename is
lower-stakes than it was).
