# Project naming

Working title used across the codebase right now: **NutriBoost** (the name
from the original idea notes). It is a placeholder, not a final decision —
"nutriboost" as a brand is generic enough that it very likely collides with
existing supplement sellers, so treat every occurrence of it in code as a
find-and-replace waiting to happen once a final name is picked.

## Candidates researched (2026-08-26)

Quick web searches only — **not** a formal trademark or domain search. Verify
on a registrar (e.g. Namecheap/Porkbun) and INESE/SENADI (Ecuador trademark
office) before committing.

1. **ThriveCrate** — no exact collisions found. "Thrive" alone is a very
   common wellness-brand prefix (Thrive Market, Thrive Fitness, etc.), so the
   full compound is what protects it, not the word "Thrive" by itself.
2. **StackWell** — no exact collisions found. Reads naturally in the
   supplement space ("stack" = supplement stack), which is a plus for SEO but
   means double-check App Store / Play Store naming too before shipping a
   mobile app under it.
3. **Fuelance** — no collisions found at all; the most distinctive/brandable
   of the three, but also the most invented (less immediately obvious what
   the store sells from the name alone).

## Rejected during research

- **FitForge** — taken (multiple apps + a registered trademark for exercise
  equipment).
- **IronCrate** — taken (`ironcrate.co`, an existing fitness subscription
  box).
- **PeakRep** — taken (`gopeakrep.com`, an existing fitness gear brand).
- **VaultFit** — too close to `vault.fit` (an existing Pilates studio chain).
- **SculptWell** — too close to an existing local fitness facility
  ("Sculpt Well Co.").
- **Nutryve** — taken (existing supplement drink-mix brand on Amazon).
- **PeakVault** — taken (a wellness site and an unrelated crypto platform).

## Recommendation

Lean toward **ThriveCrate** or **StackWell** — both read clearly as a fitness
nutrition store in English, which matters since the platform's UI language is
English. Confirm the final pick, then this file plus every `NutriBoost` /
`nutriboost` string in the codebase (package names, `index.html` title, i18n
`brand.name` keys, namespaces in the backend) needs a rename pass — log that
pass in [STRUCTURE_CHANGELOG.md](STRUCTURE_CHANGELOG.md) when it happens.
