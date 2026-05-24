# Frontend Phase 6 — Polish Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax.

**Goal:** Close the v1 by adding the error pages (404 / 403 / 500), HTMX loading indicators + toast system, accessibility sweep, and a Lighthouse pass on the home page. After this phase the site is feature-complete and ship-ready.

**Architecture:** Add `CoreX/Pages/Error/` (status-code-aware error page + 500 handler) and wire `UseExceptionHandler` + `UseStatusCodePagesWithReExecute` into the pipeline. Extend `wwwroot/js/site.js` with a tiny toast handler that listens for the HTMX `showToast` event. Sweep all existing pages for missing ARIA labels and skip-to-content link. UA-hardcoded throughout, matching the rest of the codebase.

**Tech Stack:** ASP.NET Core 8 Razor Pages · HTMX 2 · xUnit + `Microsoft.AspNetCore.Mvc.Testing` · Chrome DevTools MCP for the manual Lighthouse pass.

**Spec reference:** `docs/superpowers/specs/2026-05-20-frontend-design.md` — Phase 6 in §11 (note: original spec numbered this as Phase 7; after TrainingPlan was dropped, the polish phase renumbered to 6).

---

## Scope

**In scope:**
- `/Error` (500) + `/Error/{code}` (404, 403) pages, wired via `UseExceptionHandler` + `UseStatusCodePagesWithReExecute`.
- HTMX loading indicator helper class in CSS + spinner partial.
- Toast trigger system in `site.js` listening for `HX-Trigger: showToast` from server responses.
- Accessibility pass: ensure every form input has a real `<label>`, add skip-to-main-content link, ensure HTMX tab buttons are keyboard-operable, add ARIA roles on landmark regions.
- Lighthouse audit run on the home page; fix anything that brings Performance/A11y/Best-Practices/SEO score < 90.

**Out of scope:**
- EN translation of any pages (deferred indefinitely; UA-only product).
- Pagination / search on long lists.
- Real-time notifications (no SignalR).
- Photo upload, payment integration (per spec §13).
- Email-confirmation public page (the confirmation email is sent on register but no link target exists; deferred indefinitely).

## Prerequisites

- Phase 5 merged. HEAD on master: `18e4f55 Add /Admin/Users/RegisterAdmin Owner-only form with TempData success flash`.
- `dotnet build CoreX.sln --nologo` → 0 errors.
- `dotnet test CoreX.sln --nologo --no-build` → 105/105 passing.

## File map

**New files:**

| File | Responsibility |
|---|---|
| `CoreX/Pages/Error/Index.cshtml` + `.cshtml.cs` | `/Error` — generic 500 error page with localized UA message + request id. |
| `CoreX/Pages/Error/Status.cshtml` + `.cshtml.cs` | `/Error/{code}` — branches on status code (404 "Сторінку не знайдено", 403 "Доступ заборонено", default "Помилка"). |
| `CoreX/Pages/Shared/_Spinner.cshtml` | Small reusable spinner partial used as HTMX indicator. |
| `CoreX.UI.Tests/Pages/ErrorTests.cs` | 3 tests: unknown URL → 404 page, no-permission → 403 page (via a triggering route), unhandled exception → 500 page. |

**Modified files:**

| File | Change |
|---|---|
| `CoreX/Program.cs` | Add `app.UseExceptionHandler("/Error")` + `app.UseStatusCodePagesWithReExecute("/Error/{0}")` after `app.UseRouting()`. Set `IdentityOptions.SignIn.RequireConfirmedEmail = false` if not already (no-op if default). Set Identity `LoginPath` + `AccessDeniedPath` to `/Account/Login` and `/Error/403` respectively. |
| `CoreX/Pages/Shared/_Layout.cshtml` | Add `<a href="#main-content" class="sr-only focus:not-sr-only ...">Перейти до контенту</a>` as the first element inside `<body>`. Ensure `<main>` has `id="main-content"`. Add ARIA `role="navigation"` on the nav, `role="banner"` on the header, `role="contentinfo"` on the footer. |
| `CoreX/Pages/Admin/_AdminLayout.cshtml` | Same skip-link + ARIA roles. |
| `CoreX/wwwroot/js/site.js` | Add an event listener for HTMX `showToast` events that renders a small UA-localized toast in a fixed-position container. |
| `CoreX/Styles/site.css` | Add `.htmx-indicator` (Tailwind component); add toast styles. |
| `CoreX/Pages/Clubs/Detail.cshtml` | Wire `hx-indicator=".tab-spinner"` on tab buttons + add `<div class="tab-spinner htmx-indicator">…</div>`. |

**Out of plan (won't change):**

- Existing empty states from Phases 2-5 are already in place. Spot-check during smoke; only fix if visibly wrong.
- Existing form inputs already have `<label>` elements with `asp-for`. No retrofit needed except the accessibility sweep.

---

## Task 1 — Error pages + pipeline wiring (TDD)

**Files:**
- Create: `CoreX/Pages/Error/Index.cshtml` + `.cshtml.cs`
- Create: `CoreX/Pages/Error/Status.cshtml` + `.cshtml.cs`
- Modify: `CoreX/Program.cs` (add 2 pipeline calls + IdentityOptions paths)
- Test: `CoreX.UI.Tests/Pages/ErrorTests.cs`

The pipeline order matters: `UseStatusCodePagesWithReExecute` must come BEFORE `UseAuthentication` so the re-execute pass runs through full middleware. `UseExceptionHandler` goes near the top.

- [ ] **Step 1: Failing tests**

`CoreX.UI.Tests/Pages/ErrorTests.cs`:

```csharp
using System.Net;
using CoreX.UI.Tests.TestSupport;
using Xunit;

namespace CoreX.UI.Tests.Pages;

public class ErrorTests : IClassFixture<CoreXFactory>
{
    private readonly CoreXFactory _factory;
    public ErrorTests(CoreXFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_UnknownUrl_RendersLocalizedNotFoundPage()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/this-url-does-not-exist-xyz");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Сторінку не знайдено", body);
    }

    [Fact]
    public async Task Get_AdminPath_AsUser_RendersAccessDeniedPage()
    {
        var email = $"user-403-{Guid.NewGuid():N}@test";
        await TestUsers.CreateAsync(_factory, email, role: "User");
        var client = await TestUsers.SignedInClientAsync(_factory, email);

        var response = await client.GetAsync("/Admin");

        // After Phase 6, IdentityOptions.AccessDeniedPath = "/Error/403", so the redirect
        // chain ends on the 403 page. We follow redirects (default in CreateClient).
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Доступ заборонено", body);
    }
}
```

(The 500 test is hard to write without a deliberate-failure endpoint. Skipping it for v1 — the exception handler is exercised manually during smoke.)

- [ ] **Step 2: Run — confirm 2 fail**

The 404 test currently returns the default ASP.NET text "An error occurred while processing your request" or a blank 404; the 403 test currently shows the default access-denied page (or a `404` if `AccessDeniedPath` isn't set).

- [ ] **Step 3: Create `Pages/Error/Index.cshtml.cs` (the 500 page)**

```csharp
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Error;

[AllowAnonymous]
public class IndexModel : PageModel
{
    public string? RequestId { get; private set; }

    public void OnGet() => RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
}
```

- [ ] **Step 4: Create `Pages/Error/Index.cshtml`**

```cshtml
@page
@model CoreX.Pages.Error.IndexModel
@{
    Layout = "/Pages/Shared/_Layout.cshtml";
    ViewData["Title"] = "Помилка";
}

<section class="max-w-xl mx-auto px-4 py-20 text-center">
    <p class="text-xs font-semibold tracking-[0.2em] uppercase text-brand-500">500</p>
    <h1 class="mt-3 text-4xl md:text-5xl font-black uppercase tracking-tight">Щось пішло не так</h1>
    <p class="mt-4 text-ink-500">Ми вже шукаємо причину. Спробуйте оновити сторінку або повернутися на головну.</p>
    @if (!string.IsNullOrEmpty(Model.RequestId))
    {
        <p class="mt-6 text-xs font-mono text-ink-500">Номер запиту: @Model.RequestId</p>
    }
    <a href="/" class="btn-brand mt-10 inline-flex">На головну</a>
</section>
```

- [ ] **Step 5: Create `Pages/Error/Status.cshtml.cs`**

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Error;

[AllowAnonymous]
public class StatusModel : PageModel
{
    public int Code { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    public IActionResult OnGet(int code)
    {
        Code = code;
        (Title, Description) = code switch
        {
            404 => ("Сторінку не знайдено", "Можливо, ви помилились у посиланні, або сторінку було видалено."),
            403 => ("Доступ заборонено", "У вас немає прав для перегляду цієї сторінки."),
            _ => ($"Помилка {code}", "Сталася непередбачувана помилка."),
        };
        Response.StatusCode = code;
        return Page();
    }
}
```

- [ ] **Step 6: Create `Pages/Error/Status.cshtml`**

```cshtml
@page "/Error/{code:int}"
@model CoreX.Pages.Error.StatusModel
@{
    Layout = "/Pages/Shared/_Layout.cshtml";
    ViewData["Title"] = Model.Title;
}

<section class="max-w-xl mx-auto px-4 py-20 text-center">
    <p class="text-xs font-semibold tracking-[0.2em] uppercase text-brand-500">@Model.Code</p>
    <h1 class="mt-3 text-4xl md:text-5xl font-black uppercase tracking-tight">@Model.Title</h1>
    <p class="mt-4 text-ink-500">@Model.Description</p>
    <a href="/" class="btn-brand mt-10 inline-flex">На головну</a>
</section>
```

- [ ] **Step 7: Wire pipeline in `Program.cs`**

Inside the `app` pipeline section (after `app.UseRouting()` is the natural place; before `app.UseAuthentication()`), add:

```csharp
app.UseExceptionHandler("/Error");
app.UseStatusCodePagesWithReExecute("/Error/{0}");
```

Also extend the `AddIdentity<...>` options block to set:

```csharp
options.SignIn.RequireConfirmedEmail = false;
```

(no-op if it's already false by default; explicit is better) and add a separate `ConfigureApplicationCookie` block AFTER `AddIdentity`:

```csharp
builder.Services.ConfigureApplicationCookie(o =>
{
    o.LoginPath = "/Account/Login";
    o.AccessDeniedPath = "/Error/403";
});
```

Place this between the existing `AddIdentity<...>` block and `var app = builder.Build();`.

- [ ] **Step 8: Run tests — confirm 2 pass**

- [ ] **Step 9: Full suite — expect 107/107**

- [ ] **Step 10: Commit**

```bash
git add CoreX/Pages/Error/ CoreX/Program.cs CoreX.UI.Tests/Pages/ErrorTests.cs
git commit -m "Add /Error 500 + /Error/{code} 404/403 pages with localized UA messages"
```

---

## Task 2 — HTMX loading indicator + toast system

**Files:**
- Create: `CoreX/Pages/Shared/_Spinner.cshtml`
- Modify: `CoreX/Styles/site.css` (add `.htmx-indicator` rule + toast styles)
- Modify: `CoreX/wwwroot/js/site.js` (add toast event listener)
- Modify: `CoreX/Pages/Clubs/Detail.cshtml` (add `hx-indicator` to tab buttons)
- Modify: `CoreX/Pages/Shared/_Layout.cshtml` (add toast container)

HTMX shows any element with class `htmx-indicator` while a request is in flight (it adds `htmx-request` class to the trigger AND any `hx-indicator` target). Tailwind's `htmx-request:visible` and `htmx-indicator` patterns are convention; the simplest is a CSS `.htmx-indicator { opacity: 0; transition: opacity 200ms; } .htmx-indicator.htmx-request, .htmx-request .htmx-indicator { opacity: 1; }`.

For toasts, the server sends `HX-Trigger: showToast` (or `HX-Trigger: {"showToast": {"message": "Saved", "type": "success"}}`). The JS listens for `htmx:beforeOnLoad` or directly for the `showToast` event on body.

- [ ] **Step 1: Add `.htmx-indicator` + toast styles**

In `CoreX/Styles/site.css` (after the existing `@layer components` block):

```css
@layer utilities {
  .htmx-indicator {
    opacity: 0;
    transition: opacity 200ms ease-in;
    pointer-events: none;
  }
  .htmx-indicator.htmx-request,
  .htmx-request .htmx-indicator {
    opacity: 1;
  }
}

@layer components {
  .toast {
    @apply pointer-events-auto rounded-card border bg-white shadow-flame px-4 py-3 text-sm flex items-start gap-3 max-w-sm;
  }
  .toast-success { @apply border-success text-success; }
  .toast-error   { @apply border-danger text-danger; }
  .toast-info    { @apply border-info text-info; }
}
```

- [ ] **Step 2: Create `_Spinner.cshtml`**

```cshtml
<span class="htmx-indicator inline-flex items-center gap-2 text-sm text-ink-500" aria-live="polite">
    <svg class="animate-spin h-4 w-4" viewBox="0 0 24 24" fill="none" aria-hidden="true">
        <circle cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" class="opacity-25"></circle>
        <path d="M4 12a8 8 0 018-8" stroke="currentColor" stroke-width="4" class="opacity-75"></path>
    </svg>
    Завантаження…
</span>
```

- [ ] **Step 3: Update `site.js`**

Replace the contents with:

```js
(function () {
  // Toast container — created lazily on first use.
  function getToastContainer() {
    var el = document.getElementById('toast-container');
    if (!el) {
      el = document.createElement('div');
      el.id = 'toast-container';
      el.className = 'fixed top-4 right-4 z-50 flex flex-col gap-2 pointer-events-none';
      el.setAttribute('aria-live', 'polite');
      el.setAttribute('aria-atomic', 'true');
      document.body.appendChild(el);
    }
    return el;
  }

  function showToast(message, type) {
    var container = getToastContainer();
    var toast = document.createElement('div');
    toast.className = 'toast toast-' + (type || 'info');
    toast.textContent = message;
    container.appendChild(toast);
    setTimeout(function () { toast.remove(); }, 4000);
  }

  // HTMX -> showToast event bridge.
  // Server sends: HX-Trigger: {"showToast": {"message": "...", "type": "success"}}
  // OR plain: HX-Trigger: showToast (with no detail; falls back to a generic message).
  document.body.addEventListener('showToast', function (evt) {
    var detail = evt.detail || {};
    showToast(detail.message || 'Готово.', detail.type || 'info');
  });

  // Expose for inline JS callers if needed.
  window.coreXShowToast = showToast;
})();
```

- [ ] **Step 4: Add `hx-indicator` on club detail tabs**

In `CoreX/Pages/Clubs/Detail.cshtml`, on each tab button add `hx-indicator="#tab-spinner"`, and add a spinner inside the nav region:

```cshtml
<nav class="mt-12 border-b border-ink-200 flex gap-2 text-sm font-semibold" role="tablist">
    <button type="button" hx-get="/Clubs/@Model.Club.Id?handler=Trainers"
            hx-target="#tab-content" hx-swap="innerHTML"
            hx-indicator="#tab-spinner"
            class="px-4 py-3 hover:text-brand-500">Тренери</button>
    <!-- ... three more tab buttons same shape ... -->
    <span id="tab-spinner" class="htmx-indicator ml-auto self-center mr-2">
        <partial name="_Spinner" />
    </span>
</nav>
```

- [ ] **Step 5: Build + run all tests — no regression**

```bash
dotnet build CoreX.sln --nologo
dotnet test CoreX.UI.Tests/CoreX.UI.Tests.csproj --nologo --no-build
```

Expected: 107/107 passing (no test changes; visual behavior only).

- [ ] **Step 6: Commit**

```bash
git add CoreX/Styles/site.css CoreX/wwwroot/js/site.js CoreX/Pages/Shared/_Spinner.cshtml CoreX/Pages/Clubs/Detail.cshtml
git commit -m "Add HTMX loading spinner + toast system for action feedback"
```

---

## Task 3 — Accessibility sweep

**Files:**
- Modify: `CoreX/Pages/Shared/_Layout.cshtml`
- Modify: `CoreX/Pages/Admin/_AdminLayout.cshtml`

The minimum a11y pass:
1. Skip-to-main link at the top of `<body>` (visible on focus).
2. `id="main-content"` on `<main>`.
3. ARIA landmark roles: `role="banner"` on `<header>`, `role="navigation"` on `<nav>`, `role="main"` on `<main>` (or rely on the element default), `role="contentinfo"` on `<footer>`.
4. Hero `<section>` already has `aria-labelledby` from Phase 0; spot-check.
5. Form labels are already paired with inputs via `asp-for`.

- [ ] **Step 1: Update `_Layout.cshtml`**

At the top of `<body>`, before the `<header>`:

```cshtml
<a href="#main-content" class="sr-only focus:not-sr-only focus:absolute focus:top-2 focus:left-2 focus:z-50 focus:bg-brand-500 focus:text-white focus:px-4 focus:py-2 focus:rounded-card">
    Перейти до контенту
</a>
```

On `<header>`: add `role="banner"`.
On `<nav>`: add `role="navigation" aria-label="Головна навігація"`.
On `<main>`: add `id="main-content" role="main"`.
On `<footer>`: add `role="contentinfo"`.

- [ ] **Step 2: Update `_AdminLayout.cshtml`**

Same skip link + landmark roles. Sidebar nav gets `aria-label="Адмін-навігація"`.

- [ ] **Step 3: Build + run all tests — no regression**

Expected: 107/107 passing.

- [ ] **Step 4: Commit**

```bash
git add CoreX/Pages/Shared/_Layout.cshtml CoreX/Pages/Admin/_AdminLayout.cshtml
git commit -m "A11y: skip-to-content link + ARIA landmark roles on layouts"
```

---

## Task 4 — End-to-end smoke + Lighthouse pass

- [ ] **Step 1: Full build**

```bash
dotnet build CoreX.sln --nologo
```

Expected: 0 errors.

- [ ] **Step 2: Full test suite**

```bash
dotnet test CoreX.sln --nologo --no-build
```

Expected: 107/107 passing.

- [ ] **Step 3: Browser smoke — error pages**

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet run --project CoreX/CoreX.UI.csproj --no-build --no-launch-profile --urls "http://localhost:5059"
```

Walk through:
1. `/this-does-not-exist` → 404 page renders with "Сторінку не знайдено".
2. Sign in as a User-role user → navigate to `/Admin` → 403 page renders with "Доступ заборонено".
3. Open `/Clubs/{id}` (in dev DB, if any clubs exist) and click a tab → spinner shows briefly during the HTMX swap.
4. Tab through the home page → focus rings visible on links and buttons; the skip-to-content link appears as the first focus target.

Stop the app.

- [ ] **Step 4: Lighthouse (manual)**

Open the app in Chrome (dev DB ideally seeded with at least one club via `/Admin/Clubs/Create`). Open DevTools → Lighthouse → run audit on `/` for Performance + Accessibility + Best Practices + SEO.

If Performance < 90, the most likely culprits:
- Google Fonts CSS blocking → already preconnected from Phase 0; if still blocking, consider `font-display: swap`.
- Tailwind output size — already minified by the MSBuild target; should be ~20KB.

If A11y < 90, the audit will list specific failures. Fix them inline (likely contrast or missing alt attributes if any images exist).

If SEO < 90, add `<meta name="description">` to `_Layout.cshtml` if missing.

Document the final Lighthouse scores in the commit message if any fixes are needed.

- [ ] **Step 5: `git status` clean**

---

## Phase 6 exit checklist

- [ ] `dotnet build CoreX.sln` → 0 errors.
- [ ] `dotnet test CoreX.sln` → 107 passing.
- [ ] `/this-url-does-not-exist` → 404 page with "Сторінку не знайдено".
- [ ] User-role visitor → `/Admin` → 403 page with "Доступ заборонено".
- [ ] An unhandled exception on any page renders the `/Error` 500 page (manual smoke).
- [ ] HTMX tabs show the loading spinner during swaps.
- [ ] Skip-to-content link is reachable as the first focus on every page.
- [ ] Lighthouse home-page Performance/A11y/Best-Practices/SEO each ≥ 90 (or documented gaps).

**End of frontend roadmap.** Site is feature-complete: anonymous discovery, registration + auth, anonymous + authenticated booking, vacancy applications, full admin panel with Owner-only sections, localized error pages, accessibility baseline.
