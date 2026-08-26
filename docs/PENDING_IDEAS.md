# Pending ideas / not forgotten, just not built yet

A running list so nothing from a brainstorm gets lost while other things
get built first — per the notes' explicit request to always be reminded of
parked ideas. Move an item to "Done" (with a date and a link to the
changelog entry) instead of deleting it.

## Blocked on you / needs a decision

- [ ] **Final project name.** Three candidates researched in
      [NAMING.md](NAMING.md) — pick one (or reject all three and ask for
      another round) before it spreads further through code and configs.
- [ ] **MonsterASP.NET vs. staying flexible.** [HOSTING.md](HOSTING.md)
      leans toward it for the backend, but confirm before any production
      config gets built around it specifically.

## Scaffolded structurally, not implemented

- [ ] `client-backend` — layered scaffold exists, builds, and is verified
      end-to-end against local Postgres (see STRUCTURE_CHANGELOG.md), but
      it's still one thin vertical slice (`Product` + `GET /api/products`,
      no seed data). No auth, cart, order, or checkout logic yet.
- [ ] `admin-frontend` + `admin-backend` — fully isolated admin app (own
      React frontend, own ASP.NET Core backend), per note 14. Deferred
      to keep this session's scope to the customer-facing MVP + navbar
      that was explicitly asked for first.
- [ ] Spanish language switcher UI — resources already exist in
      `client-frontend/src/i18n/locales/es`, just not exposed. See
      [ARCHITECTURE.md](ARCHITECTURE.md#cross-cutting-concerns-staged-for-later-not-built-yet).
- [ ] Dark mode toggle — fully wired in `ThemeProvider`, light stays the
      default until you say otherwise.

## Needs a Fable 5 pass before implementation (per AI_WORKFLOW.md)

- [ ] **Stock concurrency** — prevent two buyers both "winning" the last
      unit. Direction sketched in ARCHITECTURE.md (EF Core optimistic
      concurrency token + re-check at commit), needs an audit before it's
      trusted with real money.
- [ ] **Invoicing** — edit/delete rules (soft-delete only), Ecuador legal
      requirements for what must be on an invoice, whether an invoice is
      legally mandatory even for a guest checkout.
- [ ] **Guest checkout data model** — what's the minimum data collectible
      from a non-registered buyer while still producing a valid Ecuador
      invoice if they want one.
- [ ] **Timezone handling for registration/orders** — notes flag this as
      essential for a real Ecuador storefront (Ecuador is UTC-5, no DST,
      but don't hardcode that — store everything in UTC and convert at the
      edges).
- [ ] **Stripe refund/cancellation flow** — avoid Ecuador legal exposure;
      needs research into what Stripe actually supports vs. what Ecuadorian
      consumer law requires.
- [ ] **Google + email/password auth flow**, including password recovery —
      security-sensitive, notes explicitly ask for a Fable 5 review before
      it's built.
- [ ] **URL structure / routing scheme** — notes ask for this to be planned
      deliberately (SEO-friendly slugs, canonical structure) rather than
      grown ad hoc as pages get added.

## Nice-to-haves flagged in the notes, not urgent

- [ ] Hero `.glb` (3D model) + scroll-triggered section animations on the
      home page.
- [ ] Carousels on the storefront landing page.
- [ ] Admin dashboard charting library (for order/revenue graphs) — needs
      picking once `admin-frontend` exists.
- [ ] Toasts for lightweight confirmations, modals reserved for
      purchase-critical confirmations only (per note 28).
- [ ] Product image hosting strategy — needs a free/cheap option with fast
      global delivery; not decided yet.
- [ ] Errors log folder (`docs/errors/`) exists but is still empty — start
      filling it in the first time something nontrivial breaks and gets
      fixed, so it isn't relearned.

## Done

_(nothing yet)_
