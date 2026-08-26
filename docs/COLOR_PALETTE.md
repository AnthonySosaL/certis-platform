# Color palette

Source of truth: `client-frontend/src/index.css` (CSS variables, OKLCH color
space — shadcn's default). This file exists so every color addition gets
logged instead of getting silently added inline somewhere.

Light mode is the default and only mode currently exposed in the UI. Dark
mode values already exist (`.dark` class) and stay in sync whenever a light
value changes below — update both when you touch a color.

## Base (shadcn "neutral" preset, unmodified)

Background, foreground, card, popover, secondary, muted, border, input —
left at shadcn's default neutral OKLCH scale. No brand meaning attached to
these; they're UI chrome.

## Brand color: primary / accent / ring

| Token | Light | Dark | Used for |
|---|---|---|---|
| `--primary` | `oklch(0.62 0.17 150)` | `oklch(0.72 0.17 150)` | Primary buttons, active nav state background |
| `--primary-foreground` | `oklch(0.99 0 0)` | `oklch(0.145 0 0)` | Text/icons on top of `--primary` |
| `--accent` | `oklch(0.94 0.05 150)` | `oklch(0.28 0.06 150)` | Hover/highlight backgrounds |
| `--accent-foreground` | `oklch(0.3 0.1 150)` | `oklch(0.9 0.05 150)` | Text on top of `--accent` |
| `--ring` | `oklch(0.62 0.17 150)` | `oklch(0.72 0.17 150)` | Focus ring, matches primary |

Hue `150` is a green — a neutral placeholder choice (calm, legible, not
tied to any brand direction) rather than a deliberate pick for this
platform specifically. This is a first pass, not a final brand decision;
treat it as easy to swap (it's 5 variables, not scattered hex codes) once
there's a real brand direction — see [NAMING.md](NAMING.md), still open.

## Adding or changing a color

1. Edit the variable in `client-frontend/src/index.css`, both the `:root`
   block (light) and the `.dark` block (dark) — never just one.
2. Update the table above in the same change.
3. If it's a genuinely new token (not an existing one being retuned), add a
   row and say what it's for and why the old palette didn't already cover
   it.
4. Log the change in [STRUCTURE_CHANGELOG.md](STRUCTURE_CHANGELOG.md) if it
   affects components broadly (e.g. a full re-hue), skip the changelog for
   a minor shade tweak.
