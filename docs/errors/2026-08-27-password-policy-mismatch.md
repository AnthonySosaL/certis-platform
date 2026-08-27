# Password policy mismatch + misleading error message

**Date:** 2026-08-27
**Area:** client-backend (Identity config), client-frontend (Register)

## Symptom

The user tried to register with their real email and got: "Could not
create the account. The email may already be registered, or the password
is too weak." They hadn't registered before, so the message was
confusing and pointed at the wrong problem.

## Root cause

Two compounding issues:

1. **Backend/frontend password rules didn't match.** ASP.NET Core
   Identity's real defaults require uppercase + lowercase + digit +
   non-alphanumeric characters, in addition to a minimum length -
   `DependencyInjection.cs` only overrode `RequiredLength = 8` and left
   the other four `true`. The Angular Register form only validated
   `required` + `minLength(8)`, matching what was *intended*, not what
   Identity actually enforced. A perfectly reasonable 8+ character
   password that happened to be all-lowercase (or missing a symbol)
   passed client-side validation and then got rejected by the server.
2. **The frontend guessed at the error instead of showing the real one.**
   `register.ts`'s `catch` block used one fixed, vague message
   regardless of what the backend actually said - so a password-rule
   rejection displayed as "may already be registered," sending the user
   looking for a duplicate account that didn't exist.

Confirmed directly: registering the user's real email with a simple
8-character password succeeded once the policy was relaxed - the email
had never been taken in the first place.

## Fix

- `DependencyInjection.cs`: `RequireUppercase`, `RequireLowercase`,
  `RequireDigit`, `RequireNonAlphanumeric` all set to `false` - length-only
  (8+ chars), matching exactly what the frontend already validates.
- `register.ts`: added `extractIdentityErrors()`, which reads ASP.NET
  Core's `ValidationProblem()` response shape (`{ errors: { Code:
  [message, ...] } }`) and shows the actual message(s) instead of a
  guess. Verified against a real duplicate-email response:
  `{"errors":{"DuplicateEmail":["Email '...' is already taken."],...}}` -
  now displays that text directly.
- Cleaned up the accidental test account created under the user's real
  email during diagnosis (added a temporary `[HttpDelete]` cleanup
  endpoint, used it three times to remove that account and two earlier
  test accounts, then removed the endpoint again - it was never meant to
  be permanent).

## How to avoid it again

Whenever client-side validation and a backend framework's own validation
both exist for the same field (passwords, usernames, etc.), treat the
backend's actual configured rules as the source of truth and make the
frontend match them explicitly - don't assume a plausible-looking
frontend rule (`minLength(8)`) reflects what the framework defaults to.
And never show a guessed error message when the backend already returns
a specific one - surface the real reason.
