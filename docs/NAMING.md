# Project naming

**Decided (2026-08-27): the brand name is Certis.**

Picked from four recommendations (Nivelo, Bandly, Fluentia, Certis) -
evokes "certify"/"certificate", short and professional, a good fit if
this ever sells to institutions around actual certification prep. The
wordmark replaces the "C" with a seal-and-ribbon icon (a certification
badge) instead of a literal letterform - see
`client-frontend/src/app/layout/navbar/navbar.html`
(`.navbar__brand-mark`) for the SVG.

Applied so far (visible branding only):
- `core/brand.ts` — single `BRAND_NAME` constant, used by the navbar and
  footer.
- `index.html` `<title>`.
- Navbar wordmark (seal-and-ribbon "C" + "ertis").

**Not yet done (technical rename, deliberately deferred):** the backend
namespace (`EnglishC1.Client.*`), the repo folder
(`english-c1-platform`), and the two live subdomains
(`english-c1-api.runasp.net`, `english-c1.runasp.net`) still use the old
placeholder name. None of that is user-visible, and re-pointing the
live subdomains means re-provisioning HTTPS certs and CORS again (see
`docs/HOSTING.md` for how much friction that was the first two times).
Worth doing for consistency, but it's a deliberate, separate pass - not
done as a side effect of picking the name.

---

<details>
<summary>History (superseded)</summary>

**Correction (2026-08-26):** the three names previously listed here
(ThriveCrate, StackWell, Fuelance) were for a fitness e-commerce project
that isn't this project — see
[errors/2026-08-26-scope-mixup.md](errors/2026-08-26-scope-mixup.md).

Tentative candidates researched 2026-08-26 (Fluentrack, C1 Compass,
LexiTrack) were superseded by the 2026-08-27 shortlist above once the
"letter replaced by a logo" branding direction was decided.

</details>
