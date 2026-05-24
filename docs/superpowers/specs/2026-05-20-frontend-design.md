# CoreX Frontend — Design Spec

**Date:** 2026-05-20
**Status:** Approved for implementation planning
**Scope:** First implementation of the CoreX customer-facing site and admin panel. Backend (`CoreX` Web API, .NET 8) is already in place and audited.

---

## 1. Context

CoreX is a multi-club fitness chain management system. The .NET 8 backend exposes a JSON API (Identity cookie auth; roles **Owner / Admin / User**) for: Clubs, Trainers, Vacancies, VacancyApplications, Subscriptions (membership catalog), Memberships (active enrolments), Bookings (membership purchase requests), Discounts, GroupClasses, InformationMaterials, TrainingPlan, Users.

The product backlog (`docs/Додаток Б. Product Backlog.pdf`, Ukrainian) defines the visible flows: city → club → trainers/classes/vacancies/memberships, anonymous membership booking, vacancy application, training-plan generator, plus an admin panel mirroring those resources.

No frontend code currently exists — `wwwroot/` is empty.

## 2. Decisions

| Topic | Decision |
|---|---|
| App shape | **One app, role-based views.** Single Razor Pages app inside `CoreX.UI`. Public site for anyone; admin routes appear after login for Admins/Owners. |
| Stack | **Razor Pages** (page-per-route), inside the existing `CoreX` web project. |
| Interactivity | **HTMX + small vanilla JS.** Server returns HTML fragments for dynamic bits; no JSON/SPA layer. |
| Styling | **Tailwind CSS**, built via CLI watcher into `wwwroot/css/site.css`. |
| Localization | **Ukrainian + English**, UA default, cookie-persisted switcher. |
| Visual direction | **Style A — Energetic / Sporty.** Orange→red `flame` gradient, Inter, uppercase headlines, commercial-gym voice. |
| Auth | Cookie auth (existing Identity setup, untouched). Folder-level `[Authorize]` policies. |
| Existing API controllers | Untouched. Razor Pages call services directly via DI. |

## 3. Architecture

**Architecture pattern:** Razor Pages with services-direct data access and HTMX swap targets implemented as page handlers returning partials.

```
CoreX/                              (existing Web project)
├── Pages/                          (new)
│   ├── Shared/
│   │   ├── _Layout.cshtml          public layout (nav, footer, language switcher)
│   │   ├── _AdminLayout.cshtml     admin layout (sidebar)
│   │   ├── _ValidationScripts.cshtml
│   │   └── Partials/               cross-cutting (alerts, pagination, toast trigger)
│   ├── _ViewImports.cshtml         @using, @addTagHelper, @inject IStringLocalizer
│   ├── _ViewStart.cshtml
│   ├── Index.cshtml                home (city picker, featured clubs)
│   ├── Clubs/
│   ├── Trainers/
│   ├── Memberships/                Subscription catalog + Booking form
│   ├── GroupClasses/
│   ├── Vacancies/
│   ├── Discounts/                  "Promotions" public page
│   ├── InformationMaterials/
│   ├── Account/                    Login, Register, Profile, MyBookings
│   ├── Admin/                      role-protected
│   └── Error.cshtml + Error/{code}.cshtml
├── Resources/                      (new) .resx for IStringLocalizer
├── Styles/site.css                 (new) Tailwind input
├── wwwroot/
│   ├── css/site.css                Tailwind output (generated)
│   ├── js/htmx.min.js              vendored
│   ├── js/site.js                  HTMX toast handler, antiforgery hookup, small helpers
│   └── images/
├── tailwind.config.js              (new)
├── package.json                    (new — devDep: tailwindcss, @tailwindcss/forms)
└── Program.cs                      (modified — Razor Pages, localization, policies)
```

### 3.1 `Program.cs` additions (illustrative)

```csharp
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin", "AdminOrOwner");
    options.Conventions.AuthorizeFolder("/Account", "AuthenticatedOnly");
    options.Conventions.AllowAnonymousToPage("/Account/Login");
    options.Conventions.AllowAnonymousToPage("/Account/Register");
    options.Conventions.AuthorizePage("/Admin/Subscriptions/Index", "OwnerOnly");
    options.Conventions.AuthorizePage("/Admin/Discounts/Index", "OwnerOnly");
    options.Conventions.AuthorizePage("/Admin/Users/RegisterAdmin", "OwnerOnly");
    // plus Create / Edit children of the owner-only resources
});

builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("AdminOrOwner",      p => p.RequireRole("Admin", "Owner"));
    o.AddPolicy("OwnerOnly",         p => p.RequireRole("Owner"));
    o.AddPolicy("AuthenticatedOnly", p => p.RequireAuthenticatedUser());
});

builder.Services.AddLocalization(o => o.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(o =>
{
    var supported = new[] { new CultureInfo("uk"), new CultureInfo("en") };
    o.DefaultRequestCulture = new RequestCulture("uk");
    o.SupportedCultures = supported;
    o.SupportedUICultures = supported;
});

// post-build():
app.UseExceptionHandler("/Error");
app.UseStatusCodePagesWithReExecute("/Error/{0}");
app.UseRequestLocalization();
app.MapRazorPages();   // alongside existing app.MapControllers();
```

### 3.2 Toolchain

- **Tailwind:** `npx tailwindcss -i ./Styles/site.css -o ./wwwroot/css/site.css --watch` during dev. MSBuild target runs it once before `Build` so `dotnet build` and `dotnet publish` produce a working bundle. Tailwind content scan covers `./Pages/**/*.cshtml`, `./Pages/**/*.cshtml.cs`, `./wwwroot/js/**/*.js`.
- **HTMX:** vendored under `wwwroot/js/htmx.min.js`. Loaded once in `_Layout`. No bundler.
- **Node:** required only for the Tailwind CLI. `package.json` lists exactly one devDependency.

## 4. Routing & page map

### Public

| Route | Auth | Backing service |
|---|---|---|
| `/` | anon | — (home: city picker, featured clubs) |
| `/Clubs?city=…` | anon | `IClubService.GetByCity` |
| `/Clubs/{id}` | anon | `IClubService.GetById` |
| `/Clubs/{id}?handler=Trainers` *(htmx)* | anon | `ITrainerService.GetByClubId` |
| `/Clubs/{id}?handler=GroupClasses` *(htmx)* | anon | `IGroupClassService.GetByClubId` |
| `/Clubs/{id}?handler=Vacancies` *(htmx)* | anon | `IVacancyService.GetByClubId` |
| `/Clubs/{id}?handler=Memberships` *(htmx)* | anon | `ISubscriptionService.GetByClubId` |
| `/Memberships?clubId=…` | anon | `ISubscriptionService` |
| `/Memberships/{subId}/Book` | anon | `IBookingService.Create` |
| `/Vacancies/{id}` | anon | `IVacancyService.GetById` |
| `/Vacancies/{id}/Apply` | anon | `IVacancyApplicationService.Create` |
| `/Discounts` | anon | `IDiscountService.GetActive` |
| `/InformationMaterials` | anon | `IInformationMaterialService` |
| `/Account/Login` | anon | `IUserService.SignIn` |
| `/Account/Register` | anon | `IUserService.UserRegister` |
| `/Account/Logout` | user | `IUserService.SignOut` |
| `/Account/Profile` | user | `IUserService.GetCurrent` |
| `/Account/MyBookings` | user | `IBookingService.GetByUserId` |
| `POST /SetLanguage?culture=…&returnUrl=…` | anon | language switcher handler (writes culture cookie, 302 to `returnUrl`) |

### Admin (folder-protected `AdminOrOwner` unless noted)

| Route | Auth | Backing service |
|---|---|---|
| `/Admin` | admin | — (dashboard: counts, recent activity) |
| `/Admin/Clubs` | admin | `IClubService.GetAll` |
| `/Admin/Clubs/Create`, `/Admin/Clubs/{id}/Edit` | admin | `IClubService.Create / Update` |
| `/Admin/Clubs/{id}/Trainers` | admin | `ITrainerService` CRUD (htmx tables) |
| `/Admin/Clubs/{id}/GroupClasses` | admin | `IGroupClassService` CRUD |
| `/Admin/Clubs/{id}/Vacancies` | admin | `IVacancyService` CRUD |
| `/Admin/VacancyApplications` | admin | `IVacancyApplicationService.GetAll` |
| `/Admin/VacancyApplications/{id}` *(htmx accept/reject)* | admin | `.Accept / .Reject` |
| `/Admin/Bookings` | admin | `IBookingService.GetAll` |
| `/Admin/Bookings/{id}` *(htmx confirm/cancel)* | admin | `.Confirm / .Cancel` |
| `/Admin/Subscriptions` | **owner** | `ISubscriptionService` CRUD |
| `/Admin/Discounts` | **owner** | `IDiscountService` CRUD |
| `/Admin/InformationMaterials` | admin | `IInformationMaterialService` CRUD |
| `/Admin/Users/RegisterAdmin` | **owner** | `IUserService.AdminRegister` |

**Structural notes:**
- Club detail (`/Clubs/{id}`) is a single page with tabs (Overview / Trainers / GroupClasses / Vacancies / Memberships) swapped via HTMX. One URL per club.
- Booking is anonymous-friendly. The form collects name/phone/email even when not logged in; logged-in user's identity is captured automatically alongside contact fields.

## 5. Authentication & authorization

- **Cookie auth, existing Identity setup unchanged.** Identity lockout (5 attempts / 15 min, `Program.cs:90-92`) satisfies the brute-force protection the backlog requires.
- **Policies** (Section 3.1): `AdminOrOwner`, `OwnerOnly`, `AuthenticatedOnly`.
- **Login / Register pages** call `IUserService` directly (no HTTP self-call). The existing `UsersController` remains as a JSON API.
- **Sign-in error UX:** generic credentials error (no enumeration); distinct lockout message. Both localized.
- **Antiforgery:** Razor Pages emits `__RequestVerificationToken` automatically inside `<form>`. For HTMX requests outside a form, the layout injects a body-level `hx-headers` carrying the antiforgery header — every HTMX request inherits it.

### Existing-code change required

The backlog allows **anonymous** membership booking. Current `IBookingService.CreateAsync(Guid userId, CreateBookingDto)` assumes a known user; `BookingsController.Create` is `[Authorize]` and reads `User.FindFirstValue(ClaimTypes.NameIdentifier)!`.

**Resolution:** change `IBookingService.CreateAsync` first parameter to `Guid? userId`. The Booking entity already stores contact fields independent of `UserId`, so the model supports this. The API controller continues to read the claim and passes a non-null Guid — **existing API behaviour preserved**. The Razor Page route passes `null` when the caller is anonymous. This change is part of Phase 3 and must be reflected in the implementation plan.

## 6. HTMX interaction patterns

### Wiring conventions

| Concern | Convention |
|---|---|
| HTMX detection | Server reads `HX-Request: true`. Helper: `IsHtmx(this HttpRequest)`. |
| Handler return | PageModel branches: full request → `Page()`; HTMX → `Partial("_Section", vm)`. |
| Active state on tabs | `hx-swap-oob="true"` on the active-tab nav node; server returns swapped content + swapped nav. |
| Validation errors | Re-render the form partial with ModelState errors (HTTP 200 — HTMX needs 200 to swap). |
| Toasts | Server adds `HX-Trigger: showToast` with JSON detail; `site.js` listens and renders. |
| Redirects after action | `HX-Redirect: /...` (e.g. session expired → Login). |
| Loading state | `hx-indicator=".spinner"` per action; Tailwind `htmx-request:visible` toggles. |

### Where HTMX is used vs not

- **Used:** club detail tabs, admin row actions (Confirm/Cancel/Accept/Reject), city filter dropdown on `/Clubs`, "Load more" pagination.
- **Not used:** Login/Register (plain form POST), CRUD Create/Edit pages (full page reload), top-level navigation.

### Error handling

- Network / 500 → toast "Щось пішло не так / Something went wrong"; form/row stays as-is.
- 401 mid-action → `HX-Redirect` to `/Account/Login?returnUrl=…`.
- 403 (lost permissions) → toast + page reload.
- Validation → form partial with ModelState errors.

## 7. Localization

### Scope

- **Localized:** all UI chrome — page titles, buttons, labels, nav, validation, toasts, page error messages, emails (eventually). Every string typed into a `.cshtml` or page model goes through `IStringLocalizer`.
- **Not localized:** user-generated content — club names, descriptions, trainer bios, vacancy text, discount descriptions. The entities have one column per text field; admins enter content in whichever language they prefer.
- **InformationMaterial** gets a `Locale` column (`uk`/`en`) added — admins create paired records; the page filters by current culture. This is a small backend change (single column on one entity); part of the implementation plan.

### Layout

```
Resources/
  Pages/
    Index.uk.resx
    Index.en.resx
    Clubs/Index.uk.resx, Clubs/Index.en.resx
    Clubs/Detail.uk.resx, Clubs/Detail.en.resx
    …
  SharedResource.uk.resx     (nav, buttons, common labels)
  SharedResource.en.resx
  ValidationMessages.uk.resx  (DataAnnotations overrides)
  ValidationMessages.en.resx
```

### Culture detection chain

1. Cookie `.AspNetCore.Culture` (set by language switcher).
2. Query `?ui-culture=uk` (sharable / debugging).
3. `Accept-Language` header.
4. Fallback: `uk`.

### Switcher

Small dropdown in nav. POST to `/set-language?culture=…&returnUrl=…` writes the culture cookie via `CookieRequestCultureProvider.MakeCookieValue(...)` and 302s back. Implemented as a tiny Razor Page handler.

### Validation messages

`[Required]`, `[StringLength]`, etc. use `ErrorMessageResourceName` + `ErrorMessageResourceType = typeof(ValidationMessages)` for both server and (via a small `IValidationAttributeAdapterProvider`) client-side messages.

### Pluralization

Ukrainian plurals are non-trivial (1 клуб / 2 клуби / 5 клубів). For the handful of count strings we use, a small `Plural(L, count, "One", "Few", "Many")` helper covers it. No ICU library needed.

### Dates / numbers

`CultureInfo.CurrentCulture` from the request drives `.ToString("d")` / `.ToString("C0")` formatting. Times stored UTC, rendered local.

## 8. Visual design system (Style A — Energetic / Sporty)

### Tokens

**Brand:**
- `flame` gradient: `linear-gradient(135deg, #ff4d2e 0%, #ff8a00 100%)`
- `brand-50`: `#fff3ee` · `brand-300`: `#ff8a00` · `brand-500`: `#ff4d2e` · `brand-700`: `#c43a18`

**Neutrals (`ink-*`):** `50: #fafafa`, `200: #e6e6e6`, `500: #666`, `800: #222`, `900: #0d0f12`.

**Semantic:** `success: #1d6f3d`, `danger: #c43a18`, `warning: #a86b00`, `info: #1c3f8a`, `owner: #6c2497`.

**Typography:** Inter (variable). Six steps:
- Display 36/900 letter-spacing −1
- H2 24/800 letter-spacing −0.5
- H3 18/700
- Body 14/500 line-height 1.55
- Eyebrow 12/600 uppercase tracking 1.2
- Mono 11px (IDs, codes)

**Radii:** `pill: 9999px`, `card: 14px`.
**Shadow:** `flame: 0 8px 24px -10px rgba(255,77,46,0.45)`.

### Components

Everything is Tailwind utilities inside Razor partials. **One** class is promoted to CSS via `@apply` because it repeats ~40 places and combines the gradient + shadow:

```css
/* Styles/site.css */
@layer components {
  .btn-brand {
    @apply bg-flame text-white font-bold tracking-wide px-6 py-3 rounded-pill shadow-flame
           hover:brightness-110 transition;
  }
}
```

Reusable patterns:
- **Card** — used for clubs / vacancies / subscriptions / trainers. One partial parameterized by props.
- **Status badge** — `badge-new` (orange tint), `badge-confirmed` (green), `badge-cancelled` (grey), `badge-owner` (purple). Used for booking and application statuses.
- **Form input** — bordered, focus ring in brand color, paired with a small `label-text` block.
- **Nav** — sticky top bar, brand wordmark with `X` highlighted in `brand-500`, language switcher right-aligned.

### `tailwind.config.js` (excerpt)

```js
module.exports = {
  content: ['./Pages/**/*.cshtml', './Pages/**/*.cshtml.cs', './wwwroot/js/**/*.js'],
  theme: {
    extend: {
      colors: {
        brand: { '50':'#fff3ee', '300':'#ff8a00', '500':'#ff4d2e', '700':'#c43a18' },
        ink:   { '50':'#fafafa', '200':'#e6e6e6', '500':'#666', '800':'#222', '900':'#0d0f12' },
        success: '#1d6f3d', danger: '#c43a18', warning: '#a86b00', info: '#1c3f8a', owner: '#6c2497',
      },
      fontFamily: { sans: ['Inter', 'system-ui', 'sans-serif'] },
      backgroundImage: { 'flame': 'linear-gradient(135deg, #ff4d2e 0%, #ff8a00 100%)' },
      borderRadius: { 'pill': '9999px', 'card': '14px' },
      boxShadow: { 'flame': '0 8px 24px -10px rgba(255,77,46,0.45)' },
    },
  },
  plugins: [require('@tailwindcss/forms')],
};
```

## 9. Validation & error handling

### Three validation layers (existing — Razor Pages plug in)

1. **Domain entity** — `Booking`, `Subscription`, etc. throw on invalid input in constructors. Authoritative.
2. **Service** — throws on not-found / not-allowed.
3. **PageModel / DataAnnotations** — `[Required]`, `[EmailAddress]`, etc. on input models. Drives client-side hints and server-side `ModelState.IsValid`. Messages from `ValidationMessages.*.resx`.

### Page POST pattern

```csharp
public async Task<IActionResult> OnPostAsync()
{
    if (!ModelState.IsValid)
        return Request.IsHtmx() ? Partial("_Form", this) : Page();

    try { await _service.DoThing(Input); }
    catch (ArgumentException ex)         { ModelState.AddModelError(string.Empty, ex.Message); return Page(); }
    catch (InvalidOperationException ex) { ModelState.AddModelError(string.Empty, ex.Message); return Page(); }

    return RedirectToPage("./Index");
}
```

### Exception strategy

| Surface | Handler |
|---|---|
| API controllers (`/clubs`, `/bookings`, …) | Existing `GlobalExceptionFilter` → `ProblemDetails`. **Unchanged.** |
| Razor Pages — full page | `app.UseExceptionHandler("/Error")` → `Pages/Error.cshtml`. Logs with request id. |
| Razor Pages — HTMX swap | Small middleware: detects `HX-Request: true`, on exception returns 200 with an error-fragment + `HX-Trigger: showToast`. Avoids replacing the user's page with a server error page. |

`app.UseStatusCodePagesWithReExecute("/Error/{0}")` covers 404 / 403; 401 is handled by Identity's `LoginPath`.

### Logging

Built-in `ILogger<T>` everywhere; default console logger for v1. One log line per unhandled exception (request id, route, user id if available).

## 10. Testing strategy

| Layer | Test? | Rationale |
|---|---|---|
| Razor markup | No | Razor compiler type-checks at build. Manual review + e2e covers behaviour. |
| PageModel handlers | Selectively | Cover non-trivial logic (anonymous booking, tab-handler pages). Skip pure pass-throughs. |
| Service / domain | Already covered | Out of frontend scope. |
| Auth policies | Yes, one per role boundary | `WebApplicationFactory` integration test per protected endpoint × role matrix. |
| HTMX swap shape | Yes, one per endpoint | Integration test confirming an `HX-Request: true` request returns a partial (no `<html>`/`<body>`). |
| Localization | One smoke test | Hit a page with `?ui-culture=en` and confirm a known UA string is absent. |
| End-to-end (Playwright) | Skip for v1 | High setup cost; manual click-through covers MVP. Revisit later. |

**Test project to add:** `CoreX.UI.Tests` (xUnit + `Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program>`), in-memory database via DI override. Helpers: `LoginAs(role)`, `Hx()`. Target ~30–50 tests for v1, weighted to auth + HTMX shape.

## 11. Phased build sequence

Each phase ships as its own PR.

| # | Phase | Output | Exit criteria |
|---|---|---|---|
| 0 | Foundation | `Pages/`, `Resources/`, Tailwind, language switcher, one styled home page | Stack is alive end-to-end on one page; `dotnet run` shows UA/EN switching working |
| 1 | Auth + account | Login, Register, Logout, Profile, MyBookings + role-boundary integration tests | A user can register, log in, see their profile, log out |
| 2 | Public discovery | Home, `/Clubs`, club detail (tabbed via HTMX), `/Trainers/{id}`, Discounts, InformationMaterials, city filter | A visitor can browse clubs end to end |
| 3 | Memberships + booking | Subscriptions catalog, booking form (anonymous-friendly), `IBookingService.CreateAsync(Guid?, …)` change | Anyone can book a subscription; logged-in users see it on `/Account/MyBookings` |
| 4 | Vacancies + applications | `/Vacancies/{id}`, `/Vacancies/{id}/Apply`, email hook via existing `IEmailSender` | Anyone can apply to a vacancy; admin sees the queue |
| 5 | Admin panel | Admin layout + dashboard + CRUD pages + booking/application review (htmx) + Owner-only Subscriptions/Discounts/RegisterAdmin | An Owner can run the whole business through the panel |
| 6 | Polish | 404/500/403 pages, empty states, loading indicators, toasts, a11y / Lighthouse pass | All error routes (404, 403, 500) render with localized messages; manual keyboard navigation works on HTMX tabs and admin row actions; Lighthouse score ≥ 90 on the home page (Performance, A11y, Best Practices, SEO) |

Phases 0–3 are the critical path to a demo-able public site.

## 12. Backend changes required by this spec

These are the only backend changes this frontend work introduces — they are listed here so the implementation plan can sequence them properly.

1. **`IBookingService.CreateAsync` signature** — first parameter becomes `Guid? userId` (currently `Guid userId`). API controller behaviour preserved (Section 5). Lands in Phase 3.
2. **`InformationMaterial.Locale` column** — new column on the entity + EF migration + filter on the public page. Lands in Phase 2.

No other backend code is modified.

## 13. Out of scope (v1)

- Multilingual admin-entered content (club names, descriptions, vacancy text, etc.) — single-language as typed.
- Real email backend — `ConsoleEmailSender` (already DI-registered) is enough for v1.
- Photo upload UI for clubs (the backlog mentions optional photos; deferred — admins can paste an image URL field for v1).
- Playwright / browser e2e suite.
- Serilog / structured logging upgrade.
- Real-time notifications (no SignalR).
- Payment integration on booking (booking remains a confirmation request reviewed by admin, per the backlog).

---

*This spec captures the design only. The implementation plan, including file-by-file changes and tests, is the next deliverable (`writing-plans` skill).*
