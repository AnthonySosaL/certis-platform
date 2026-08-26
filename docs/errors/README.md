# Errors log

One file per nontrivial bug that took real effort to track down — not every
typo. Name files `YYYY-MM-DD-short-slug.md`. Purpose: stop re-solving the
same problem twice, per the notes' explicit request.

Template for each entry:

```markdown
# <short title>

**Date:** YYYY-MM-DD
**Area:** client-frontend | client-backend | admin-frontend | admin-backend | infra

## Symptom
What broke, what you saw.

## Root cause
What actually caused it.

## Fix
What changed to resolve it.

## How to avoid it again
The rule/check that prevents a repeat.
```
