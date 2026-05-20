# Frontend Phase 0 — Foundation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Set up the Razor Pages + HTMX + Tailwind frontend foundation inside the existing `CoreX` web project, with localization (UA/EN) and a single styled landing page served end-to-end.

**Architecture:** Add a `Pages/` folder, `Resources/` (`.resx`), `Styles/site.css` (Tailwind input), and `tailwind.config.js` to the existing `CoreX` project. Wire Tailwind into MSBuild so `dotnet build` produces a working CSS bundle. Register Razor Pages, request localization, and three authorization policies in `Program.cs`. Ship one home page (`/`) with the brand wordmark, a hero, a language switcher, and a styled brand button — covered by an integration smoke test that asserts 200-OK and culture-correct content.

**Tech Stack:** ASP.NET Core 8 Razor Pages · Tailwind CSS (CLI) · HTMX (vendored, not used yet but loaded for later phases) · xUnit + `Microsoft.AspNetCore.Mvc.Testing` · EF Core InMemory (test override).

**Spec reference:** `docs/superpowers/specs/2026-05-20-frontend-design.md` — Phase 0 in §11.

---

## Prerequisites

- Node.js ≥ 18 and npm installed (verified: Node v24.15, npm 11.12 on this machine).
- .NET 8 SDK (already present — `dotnet build` succeeds on the existing solution).
- A working `appsettings.Development.json` with `Owner:Email` and `Owner:Password` set (already required by `IdentityInitializer.AddOwnerAsync`).

## File map

**New files (paths are relative to the repo root):**

| File | Responsibility |
|---|---|
| `CoreX/package.json` | npm devDeps for the Tailwind build. |
| `CoreX/tailwind.config.js` | Tailwind theme — colors, fonts, plugins. |
| `CoreX/Styles/site.css` | Tailwind input (`@tailwind` directives + `.btn-brand` component). |
| `CoreX/wwwroot/js/htmx.min.js` | Vendored HTMX 2.x. |
| `CoreX/wwwroot/js/site.js` | Small helpers (empty in Phase 0). |
| `CoreX/Pages/_ViewImports.cshtml` | Namespaces + tag helpers + `@inject IStringLocalizer<SharedResource>`. |
| `CoreX/Pages/_ViewStart.cshtml` | Default layout = `_Layout`. |
| `CoreX/Pages/Shared/_Layout.cshtml` | Top nav (brand, links, language switcher), HTMX antiforgery wiring, CSS/JS includes. |
| `CoreX/Pages/Shared/_ValidationScriptsPartial.cshtml` | jQuery-validate script tag (used in later phases; created now to match Razor Pages convention). |
| `CoreX/Pages/Index.cshtml` | Home: hero with localized headline + brand button. |
| `CoreX/Pages/Index.cshtml.cs` | Empty `PageModel`. |
| `CoreX/Pages/SetLanguage.cshtml` | View-less Razor Page for the language switcher. |
| `CoreX/Pages/SetLanguage.cshtml.cs` | `OnPost` writes culture cookie, redirects. |
| `CoreX/Resources/SharedResource.cs` | Marker class for `IStringLocalizer<SharedResource>`. |
| `CoreX/Resources/SharedResource.uk.resx` | Shared UA strings (nav, buttons, common labels). |
| `CoreX/Resources/SharedResource.en.resx` | Shared EN strings. |
| `CoreX/Resources/Pages/Index.uk.resx` | Index page UA strings. |
| `CoreX/Resources/Pages/Index.en.resx` | Index page EN strings. |
| `CoreX.UI.Tests/CoreX.UI.Tests.csproj` | xUnit test project. |
| `CoreX.UI.Tests/CoreXFactory.cs` | `WebApplicationFactory<Program>` with InMemory DbContext + test config. |
| `CoreX.UI.Tests/Pages/IndexTests.cs` | Smoke tests for `GET /` (UA + EN). |

**Modified files:**

| File | Change |
|---|---|
| `.gitignore` | Add `node_modules/`, `.superpowers/`, `CoreX/wwwroot/css/site.css` (generated). |
| `CoreX/CoreX.UI.csproj` | Add MSBuild target that runs the Tailwind CLI before `Build`. |
| `CoreX/Program.cs` | Register Razor Pages (with auth conventions), `AddAuthorization` policies, `AddLocalization`, `RequestLocalizationOptions`, `UseRequestLocalization`, `MapRazorPages`. |
| `CoreX.sln` | Add the new `CoreX.UI.Tests` project. |

**Out of scope for Phase 0 (handled in later phases):**

- Exception-handler pages (`/Error`, `/Error/{code}`) — Phase 7.
- Anti-forgery for HTMX is wired in `_Layout` but no HTMX call yet exercises it — exercised from Phase 2.
- `IBookingService.CreateAsync(Guid?, …)` signature change — Phase 3.
- `InformationMaterial.Locale` column — Phase 2.
- Account / Admin folders, policies on them are registered but the folders don't exist yet (Razor Pages ignores conventions targeting absent folders — verified safe).

---

## Task 1 — Repository housekeeping

**Files:**
- Modify: `.gitignore`

- [ ] **Step 1: Append ignore rules**

Append the following to `.gitignore` (preserve all existing content):

```gitignore

# Frontend tooling
node_modules/
CoreX/wwwroot/css/site.css

# Superpowers brainstorming sessions
.superpowers/
```

- [ ] **Step 2: Verify**

```bash
git check-ignore -v node_modules/ .superpowers/ CoreX/wwwroot/css/site.css
```

Expected: each path prints with the matching `.gitignore` rule.

- [ ] **Step 3: Commit**

```bash
git add .gitignore
git commit -m "Add frontend tooling and superpowers ignore rules"
```

---

## Task 2 — Tailwind config

**Files:**
- Create: `CoreX/package.json`
- Create: `CoreX/tailwind.config.js`
- Create: `CoreX/Styles/site.css`

- [ ] **Step 1: Create `CoreX/package.json`**

```json
{
  "name": "corex-ui",
  "private": true,
  "version": "0.0.0",
  "description": "Tailwind build pipeline for CoreX Razor Pages frontend",
  "scripts": {
    "build:css": "tailwindcss -i ./Styles/site.css -o ./wwwroot/css/site.css --minify",
    "watch:css": "tailwindcss -i ./Styles/site.css -o ./wwwroot/css/site.css --watch"
  },
  "devDependencies": {
    "tailwindcss": "^3.4.10",
    "@tailwindcss/forms": "^0.5.7"
  }
}
```

- [ ] **Step 2: Install dependencies**

Run from `CoreX/`:

```bash
cd CoreX && npm install
```

Expected: `node_modules/` populated, `package-lock.json` created. No vulnerabilities-blocking errors.

- [ ] **Step 3: Create `CoreX/tailwind.config.js`**

```js
/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './Pages/**/*.cshtml',
    './Pages/**/*.cshtml.cs',
    './wwwroot/js/**/*.js',
  ],
  theme: {
    extend: {
      colors: {
        brand: { '50': '#fff3ee', '300': '#ff8a00', '500': '#ff4d2e', '700': '#c43a18' },
        ink:   { '50': '#fafafa', '200': '#e6e6e6', '500': '#666666', '800': '#222222', '900': '#0d0f12' },
        success: '#1d6f3d',
        danger:  '#c43a18',
        warning: '#a86b00',
        info:    '#1c3f8a',
        owner:   '#6c2497',
      },
      fontFamily: {
        sans: ['Inter', 'system-ui', 'sans-serif'],
      },
      backgroundImage: {
        'flame': 'linear-gradient(135deg, #ff4d2e 0%, #ff8a00 100%)',
      },
      borderRadius: {
        'pill': '9999px',
        'card': '14px',
      },
      boxShadow: {
        'flame': '0 8px 24px -10px rgba(255,77,46,0.45)',
      },
    },
  },
  plugins: [require('@tailwindcss/forms')],
};
```

- [ ] **Step 4: Create `CoreX/Styles/site.css`**

```css
@tailwind base;
@tailwind components;
@tailwind utilities;

@layer base {
  body {
    @apply font-sans text-ink-800 bg-white antialiased;
  }
}

@layer components {
  .btn-brand {
    @apply inline-flex items-center justify-center bg-flame text-white font-bold tracking-wide
           px-6 py-3 rounded-pill shadow-flame transition hover:brightness-110
           focus:outline-none focus:ring-2 focus:ring-brand-500 focus:ring-offset-2;
  }
  .btn-ghost {
    @apply inline-flex items-center justify-center bg-transparent text-ink-900 font-bold tracking-wide
           px-5 py-2.5 rounded-pill border-2 border-ink-900 transition hover:bg-ink-900 hover:text-white;
  }
}
```

- [ ] **Step 5: Verify Tailwind builds**

Run from `CoreX/`:

```bash
cd CoreX && npx tailwindcss -i ./Styles/site.css -o ./wwwroot/css/site.css --minify
```

Expected: prints "Done in NNms.", `CoreX/wwwroot/css/site.css` is created and contains minified CSS (~10–20 KB).

- [ ] **Step 6: Commit**

```bash
git add CoreX/package.json CoreX/package-lock.json CoreX/tailwind.config.js CoreX/Styles/site.css
git commit -m "Set up Tailwind CSS with CoreX brand tokens"
```

(`wwwroot/css/site.css` is gitignored — not committed.)

---

## Task 3 — Wire Tailwind into MSBuild

**Files:**
- Modify: `CoreX/CoreX.UI.csproj`

- [ ] **Step 1: Add the MSBuild targets**

Edit `CoreX/CoreX.UI.csproj` and add two new `<Target>` elements before the closing `</Project>` tag:

```xml
  <Target Name="EnsureNpmInstall" BeforeTargets="BuildTailwind" Condition="!Exists('$(MSBuildProjectDirectory)/node_modules')">
    <Message Importance="high" Text="Installing npm dependencies for Tailwind..." />
    <Exec Command="npm install" WorkingDirectory="$(MSBuildProjectDirectory)" />
  </Target>

  <Target Name="BuildTailwind" BeforeTargets="Build" Inputs="Styles/site.css;tailwind.config.js;@(Compile);Pages/**/*.cshtml" Outputs="wwwroot/css/site.css">
    <Message Importance="high" Text="Building Tailwind CSS..." />
    <Exec Command="npx tailwindcss -i ./Styles/site.css -o ./wwwroot/css/site.css --minify" WorkingDirectory="$(MSBuildProjectDirectory)" />
  </Target>
```

- [ ] **Step 2: Delete the generated CSS and rebuild**

```bash
rm -f CoreX/wwwroot/css/site.css
dotnet build CoreX/CoreX.UI.csproj --nologo
```

Expected: build log shows "Installing npm dependencies" (only if `node_modules` was missing) and "Building Tailwind CSS...". Build succeeds. `CoreX/wwwroot/css/site.css` is re-created.

- [ ] **Step 3: Commit**

```bash
git add CoreX/CoreX.UI.csproj
git commit -m "Run Tailwind CLI from dotnet build"
```

---

## Task 4 — Vendor HTMX and create site.js

**Files:**
- Create: `CoreX/wwwroot/js/htmx.min.js`
- Create: `CoreX/wwwroot/js/site.js`

- [ ] **Step 1: Download HTMX**

```bash
mkdir -p CoreX/wwwroot/js
curl -sSL -o CoreX/wwwroot/js/htmx.min.js https://unpkg.com/htmx.org@2.0.4/dist/htmx.min.js
```

Verify the file is ~45 KB and starts with the htmx banner:

```bash
ls -l CoreX/wwwroot/js/htmx.min.js
head -c 200 CoreX/wwwroot/js/htmx.min.js
```

Expected: file size ~45 KB; first bytes contain the htmx version string.

- [ ] **Step 2: Create `CoreX/wwwroot/js/site.js`**

```js
// site.js — small client-side helpers for CoreX
// Phase 0: empty. Phase 2+ will add HTMX toast handlers and antiforgery hookup.
(function () {
  // intentionally empty
})();
```

- [ ] **Step 3: Commit**

```bash
git add CoreX/wwwroot/js/htmx.min.js CoreX/wwwroot/js/site.js
git commit -m "Vendor HTMX 2.0.4 and seed site.js"
```

---

## Task 5 — Configure Program.cs

**Files:**
- Modify: `CoreX/Program.cs`

- [ ] **Step 1: Add the using directives**

At the top of `CoreX/Program.cs`, after the existing `using` block, add:

```csharp
using Microsoft.AspNetCore.Localization;
using System.Globalization;
```

- [ ] **Step 2: Register Razor Pages, policies, and localization**

Locate the existing line (around line 78):

```csharp
            builder.Services.AddControllers(options =>
            {
                options.Filters.Add<GlobalExceptionFilter>();
            });
```

Immediately **after** the `AddControllers(...)` block, add:

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
```

- [ ] **Step 3: Add the middleware and MapRazorPages**

Locate the existing block (around line 116):

```csharp
            app.UseRouting();

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
```

Replace it with:

```csharp
            app.UseRouting();

            app.UseStaticFiles();

            app.UseRequestLocalization();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();
            app.MapRazorPages();

            app.Run();
```

- [ ] **Step 4: Build to verify**

```bash
dotnet build CoreX/CoreX.UI.csproj --nologo
```

Expected: succeeds with 0 errors (the existing nullability warning on `ApplicationUser.FullName` is unrelated and may remain).

- [ ] **Step 5: Commit**

```bash
git add CoreX/Program.cs
git commit -m "Register Razor Pages, auth policies, and request localization"
```

---

## Task 6 — Razor Pages infrastructure

**Files:**
- Create: `CoreX/Pages/_ViewImports.cshtml`
- Create: `CoreX/Pages/_ViewStart.cshtml`
- Create: `CoreX/Pages/Shared/_ValidationScriptsPartial.cshtml`
- Create: `CoreX/Resources/SharedResource.cs`

- [ ] **Step 1: Create `CoreX/Resources/SharedResource.cs`**

```csharp
namespace CoreX.Resources;

// Marker class for IStringLocalizer<SharedResource>.
// Strings are stored in Resources/SharedResource.{uk,en}.resx.
public class SharedResource
{
}
```

- [ ] **Step 2: Create `CoreX/Pages/_ViewImports.cshtml`**

```cshtml
@using CoreX
@using CoreX.Resources
@using Microsoft.AspNetCore.Mvc.Localization
@namespace CoreX.Pages
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
@inject IViewLocalizer L
@inject IHtmlLocalizer<SharedResource> S
```

- [ ] **Step 3: Create `CoreX/Pages/_ViewStart.cshtml`**

```cshtml
@{
    Layout = "_Layout";
}
```

- [ ] **Step 4: Create `CoreX/Pages/Shared/_ValidationScriptsPartial.cshtml`**

```cshtml
<script src="~/lib/jquery-validation/dist/jquery.validate.min.js"></script>
<script src="~/lib/jquery-validation-unobtrusive/jquery.validate.unobtrusive.min.js"></script>
```

(These scripts will be wired in Phase 1 when the first form ships. Phase 0 creates the partial only so its existence is in place; no page references it yet.)

- [ ] **Step 5: Build to verify nothing breaks**

```bash
dotnet build CoreX/CoreX.UI.csproj --nologo
```

Expected: succeeds.

- [ ] **Step 6: Commit**

```bash
git add CoreX/Pages/_ViewImports.cshtml CoreX/Pages/_ViewStart.cshtml \
        CoreX/Pages/Shared/_ValidationScriptsPartial.cshtml \
        CoreX/Resources/SharedResource.cs
git commit -m "Scaffold Razor Pages view imports and SharedResource"
```

---

## Task 7 — Resource files

**Files:**
- Create: `CoreX/Resources/SharedResource.uk.resx`
- Create: `CoreX/Resources/SharedResource.en.resx`
- Create: `CoreX/Resources/Pages/Index.uk.resx`
- Create: `CoreX/Resources/Pages/Index.en.resx`

Each `.resx` file uses the standard .NET XML schema. Identical key sets across cultures; only the `<value>` differs.

- [ ] **Step 1: Create `CoreX/Resources/SharedResource.uk.resx`**

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <resheader name="resmimetype"><value>text/microsoft-resx</value></resheader>
  <resheader name="version"><value>2.0</value></resheader>
  <resheader name="reader"><value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value></resheader>
  <resheader name="writer"><value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value></resheader>
  <data name="BrandName" xml:space="preserve"><value>CoreX</value></data>
  <data name="NavClubs" xml:space="preserve"><value>Клуби</value></data>
  <data name="NavMemberships" xml:space="preserve"><value>Абонементи</value></data>
  <data name="NavTrainers" xml:space="preserve"><value>Тренери</value></data>
  <data name="NavDiscounts" xml:space="preserve"><value>Акції</value></data>
  <data name="NavVacancies" xml:space="preserve"><value>Вакансії</value></data>
  <data name="NavTrainingPlan" xml:space="preserve"><value>План тренувань</value></data>
  <data name="SignIn" xml:space="preserve"><value>Увійти</value></data>
  <data name="Register" xml:space="preserve"><value>Реєстрація</value></data>
  <data name="LanguageUkrainian" xml:space="preserve"><value>Українська</value></data>
  <data name="LanguageEnglish" xml:space="preserve"><value>English</value></data>
  <data name="FooterCopyright" xml:space="preserve"><value>© {0} CoreX. Усі права захищено.</value></data>
</root>
```

- [ ] **Step 2: Create `CoreX/Resources/SharedResource.en.resx`**

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <resheader name="resmimetype"><value>text/microsoft-resx</value></resheader>
  <resheader name="version"><value>2.0</value></resheader>
  <resheader name="reader"><value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value></resheader>
  <resheader name="writer"><value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value></resheader>
  <data name="BrandName" xml:space="preserve"><value>CoreX</value></data>
  <data name="NavClubs" xml:space="preserve"><value>Clubs</value></data>
  <data name="NavMemberships" xml:space="preserve"><value>Memberships</value></data>
  <data name="NavTrainers" xml:space="preserve"><value>Trainers</value></data>
  <data name="NavDiscounts" xml:space="preserve"><value>Promotions</value></data>
  <data name="NavVacancies" xml:space="preserve"><value>Careers</value></data>
  <data name="NavTrainingPlan" xml:space="preserve"><value>Training Plan</value></data>
  <data name="SignIn" xml:space="preserve"><value>Sign in</value></data>
  <data name="Register" xml:space="preserve"><value>Register</value></data>
  <data name="LanguageUkrainian" xml:space="preserve"><value>Українська</value></data>
  <data name="LanguageEnglish" xml:space="preserve"><value>English</value></data>
  <data name="FooterCopyright" xml:space="preserve"><value>© {0} CoreX. All rights reserved.</value></data>
</root>
```

- [ ] **Step 3: Create `CoreX/Resources/Pages/Index.uk.resx`**

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <resheader name="resmimetype"><value>text/microsoft-resx</value></resheader>
  <resheader name="version"><value>2.0</value></resheader>
  <resheader name="reader"><value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value></resheader>
  <resheader name="writer"><value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value></resheader>
  <data name="Eyebrow" xml:space="preserve"><value>МЕРЕЖА ФІТНЕС-КЛУБІВ</value></data>
  <data name="HeadlineLine1" xml:space="preserve"><value>Перетни свою межу.</value></data>
  <data name="HeadlineLine2" xml:space="preserve"><value>Тренуйся в CoreX.</value></data>
  <data name="HeroSubtitle" xml:space="preserve"><value>Знайди клуб, тренера та абонемент, який підходить саме тобі.</value></data>
  <data name="HeroCta" xml:space="preserve"><value>Знайти клуб</value></data>
</root>
```

- [ ] **Step 4: Create `CoreX/Resources/Pages/Index.en.resx`**

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <resheader name="resmimetype"><value>text/microsoft-resx</value></resheader>
  <resheader name="version"><value>2.0</value></resheader>
  <resheader name="reader"><value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value></resheader>
  <resheader name="writer"><value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value></resheader>
  <data name="Eyebrow" xml:space="preserve"><value>FITNESS CLUB NETWORK</value></data>
  <data name="HeadlineLine1" xml:space="preserve"><value>Push your limit.</value></data>
  <data name="HeadlineLine2" xml:space="preserve"><value>Train at CoreX.</value></data>
  <data name="HeroSubtitle" xml:space="preserve"><value>Find a club, a trainer, and the membership that fits you.</value></data>
  <data name="HeroCta" xml:space="preserve"><value>Find a club</value></data>
</root>
```

- [ ] **Step 5: Build to verify resource files compile**

```bash
dotnet build CoreX/CoreX.UI.csproj --nologo
```

Expected: succeeds. `obj/.../*.resources` files are generated for each `.resx`.

- [ ] **Step 6: Commit**

```bash
git add CoreX/Resources/
git commit -m "Add UA/EN resource files for shared chrome and home page"
```

---

## Task 8 — Layout and language switcher

**Files:**
- Create: `CoreX/Pages/Shared/_Layout.cshtml`
- Create: `CoreX/Pages/SetLanguage.cshtml`
- Create: `CoreX/Pages/SetLanguage.cshtml.cs`

- [ ] **Step 1: Create `CoreX/Pages/Shared/_Layout.cshtml`**

```cshtml
@using Microsoft.AspNetCore.Antiforgery
@using Microsoft.AspNetCore.Localization
@inject IAntiforgery Antiforgery
@{
    var tokens = Antiforgery.GetAndStoreTokens(Context);
    var currentCulture = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
    var returnUrl = Context.Request.Path + Context.Request.QueryString;
}
<!DOCTYPE html>
<html lang="@currentCulture">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>@(ViewData["Title"]) · @S["BrandName"]</title>
    <link rel="preconnect" href="https://fonts.googleapis.com">
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@@400;500;600;700;800;900&display=swap" rel="stylesheet">
    <link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
</head>
<body hx-headers='{"@tokens.HeaderName":"@tokens.RequestToken"}'>

    <header class="border-b border-ink-200 bg-white">
        <div class="max-w-6xl mx-auto px-4 py-4 flex items-center justify-between gap-6">
            <a href="/" class="text-2xl font-black tracking-tight text-ink-900">
                Core<span class="text-brand-500">X</span>
            </a>
            <nav class="hidden md:flex items-center gap-6 text-sm font-semibold text-ink-800">
                <a href="/Clubs">@S["NavClubs"]</a>
                <a href="/Memberships">@S["NavMemberships"]</a>
                <a href="/Trainers">@S["NavTrainers"]</a>
                <a href="/Discounts">@S["NavDiscounts"]</a>
                <a href="/Vacancies">@S["NavVacancies"]</a>
                <a href="/TrainingPlan">@S["NavTrainingPlan"]</a>
            </nav>
            <div class="flex items-center gap-3">
                <form method="post" action="/SetLanguage" class="flex items-center">
                    <input type="hidden" name="returnUrl" value="@returnUrl" />
                    <select name="culture" onchange="this.form.submit()"
                            class="text-sm border border-ink-200 rounded-pill px-3 py-1 bg-white focus:outline-none focus:ring-2 focus:ring-brand-500"
                            aria-label="Language">
                        <option value="uk" selected="@(currentCulture == "uk")">@S["LanguageUkrainian"]</option>
                        <option value="en" selected="@(currentCulture == "en")">@S["LanguageEnglish"]</option>
                    </select>
                </form>
                <a href="/Account/Login" class="btn-ghost text-sm">@S["SignIn"]</a>
                <a href="/Account/Register" class="btn-brand text-sm">@S["Register"]</a>
            </div>
        </div>
    </header>

    <main>
        @RenderBody()
    </main>

    <footer class="border-t border-ink-200 bg-ink-50 mt-16">
        <div class="max-w-6xl mx-auto px-4 py-8 text-sm text-ink-500">
            @string.Format(S["FooterCopyright"].Value, DateTime.UtcNow.Year)
        </div>
    </footer>

    <script src="~/js/htmx.min.js" asp-append-version="true"></script>
    <script src="~/js/site.js" asp-append-version="true"></script>
</body>
</html>
```

Notes:
- The `@` in `Inter:wght@400` must be escaped as `@@` in Razor — handled above.
- Nav links point to routes that don't exist yet; they 404 in Phase 0. Subsequent phases populate them.
- The `Account/Login` / `Account/Register` links also 404 until Phase 1.

- [ ] **Step 2: Create `CoreX/Pages/SetLanguage.cshtml`**

```cshtml
@page
@model CoreX.Pages.SetLanguageModel
@{
    Layout = null;
}
```

(View-less page — `OnPost` always returns a redirect, the view is never rendered.)

- [ ] **Step 3: Create `CoreX/Pages/SetLanguage.cshtml.cs`**

```csharp
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages;

public class SetLanguageModel : PageModel
{
    public IActionResult OnPost(string culture, string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(culture))
        {
            return LocalRedirect("/");
        }

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                SameSite = SameSiteMode.Lax
            });

        var target = !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? returnUrl
            : "/";

        return LocalRedirect(target);
    }
}
```

- [ ] **Step 4: Build to verify**

```bash
dotnet build CoreX/CoreX.UI.csproj --nologo
```

Expected: succeeds.

- [ ] **Step 5: Commit**

```bash
git add CoreX/Pages/Shared/_Layout.cshtml \
        CoreX/Pages/SetLanguage.cshtml \
        CoreX/Pages/SetLanguage.cshtml.cs
git commit -m "Add public layout and language switcher"
```

---

## Task 9 — Test project

**Files:**
- Create: `CoreX.UI.Tests/CoreX.UI.Tests.csproj`
- Create: `CoreX.UI.Tests/CoreXFactory.cs`
- Modify: `CoreX.sln`

- [ ] **Step 1: Create `CoreX.UI.Tests/CoreX.UI.Tests.csproj`**

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.10" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.10" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.10.0" />
    <PackageReference Include="xunit" Version="2.9.0" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\CoreX\CoreX.UI.csproj" />
  </ItemGroup>

</Project>
```

- [ ] **Step 2: Create `CoreX.UI.Tests/CoreXFactory.cs`**

```csharp
using System.Linq;
using CoreX.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreX.UI.Tests;

public class CoreXFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Owner:Email"] = "owner@corex.test",
                ["Owner:Password"] = "TestOwnerPass1!",
                ["ConnectionStrings:DatabaseConnectionString"] = "ignored-by-tests"
            });
        });

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<ApplicationDbContext>(o =>
                o.UseInMemoryDatabase($"CoreXTests-{Guid.NewGuid()}"));
        });
    }
}
```

- [ ] **Step 3: Add the project to the solution**

```bash
dotnet sln CoreX.sln add CoreX.UI.Tests/CoreX.UI.Tests.csproj
```

Expected: prints "Project ... added to the solution."

- [ ] **Step 4: Build the solution**

```bash
dotnet build CoreX.sln --nologo
```

Expected: succeeds with 0 errors (the existing nullability warning remains).

- [ ] **Step 5: Commit**

```bash
git add CoreX.UI.Tests/ CoreX.sln
git commit -m "Add CoreX.UI.Tests project with WebApplicationFactory"
```

---

## Task 10 — TDD the home page

**Files:**
- Create: `CoreX.UI.Tests/Pages/IndexTests.cs`
- Create: `CoreX/Pages/Index.cshtml`
- Create: `CoreX/Pages/Index.cshtml.cs`

- [ ] **Step 1: Write the failing tests**

Create `CoreX.UI.Tests/Pages/IndexTests.cs`:

```csharp
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CoreX.UI.Tests.Pages;

public class IndexTests : IClassFixture<CoreXFactory>
{
    private readonly CoreXFactory _factory;

    public IndexTests(CoreXFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_Index_ReturnsOk_AndUkrainianHeadline()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Перетни свою межу.", body);
        Assert.Contains("Знайти клуб", body);
        Assert.DoesNotContain("Push your limit.", body);
    }

    [Fact]
    public async Task Get_Index_WithEnglishCulture_ReturnsEnglishHeadline()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var request = new HttpRequestMessage(HttpMethod.Get, "/");
        request.Headers.Add("Cookie", $"{Microsoft.AspNetCore.Localization.CookieRequestCultureProvider.DefaultCookieName}=c=en|uic=en");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Push your limit.", body);
        Assert.Contains("Find a club", body);
        Assert.DoesNotContain("Перетни свою межу.", body);
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

```bash
dotnet test CoreX.UI.Tests/CoreX.UI.Tests.csproj --nologo
```

Expected: both tests FAIL. The first is likely a `404 Not Found` because `Pages/Index.cshtml` doesn't exist yet.

- [ ] **Step 3: Create `CoreX/Pages/Index.cshtml.cs`**

```csharp
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages;

public class IndexModel : PageModel
{
}
```

- [ ] **Step 4: Create `CoreX/Pages/Index.cshtml`**

```cshtml
@page
@model CoreX.Pages.IndexModel
@{
    ViewData["Title"] = L["HeadlineLine1"].Value;
}

<section class="bg-flame text-white">
    <div class="max-w-6xl mx-auto px-4 py-20 md:py-28">
        <p class="text-xs font-semibold tracking-[0.2em] uppercase opacity-80">
            @L["Eyebrow"]
        </p>
        <h1 class="mt-3 text-4xl md:text-6xl font-black leading-[1.05] tracking-tight uppercase">
            @L["HeadlineLine1"]<br />
            @L["HeadlineLine2"]
        </h1>
        <p class="mt-5 max-w-xl text-base md:text-lg opacity-90">
            @L["HeroSubtitle"]
        </p>
        <a href="/Clubs" class="btn-brand mt-8 bg-ink-900 hover:bg-ink-800 shadow-none">
            @L["HeroCta"] →
        </a>
    </div>
</section>
```

Note: the CTA button uses `btn-brand` for shape/typography but overrides the gradient with the dark `ink-900` background — the hero already has the flame gradient, so the CTA reverses contrast for visibility.

- [ ] **Step 5: Run tests to verify they pass**

```bash
dotnet test CoreX.UI.Tests/CoreX.UI.Tests.csproj --nologo
```

Expected: both tests PASS.

- [ ] **Step 6: Commit**

```bash
git add CoreX.UI.Tests/Pages/IndexTests.cs CoreX/Pages/Index.cshtml CoreX/Pages/Index.cshtml.cs
git commit -m "Add Index page with hero, localized in UA/EN"
```

---

## Task 11 — End-to-end smoke and final cleanup

**Files:**
- (no new files; manual verification + final commit if any drift)

- [ ] **Step 1: Run the full solution build**

```bash
dotnet build CoreX.sln --nologo
```

Expected: 0 errors (1 unrelated nullability warning permitted).

- [ ] **Step 2: Run all tests**

```bash
dotnet test CoreX.sln --nologo
```

Expected: 2 tests pass, 0 fail.

- [ ] **Step 3: Smoke-test the running app**

Start the app:

```bash
dotnet run --project CoreX/CoreX.UI.csproj --no-build
```

In a browser (or with curl from another shell), verify:

```bash
curl -s -o /dev/null -w "%{http_code}\n" http://localhost:5000/
curl -s http://localhost:5000/ | grep -o "Перетни свою межу" | head -1
```

Expected: HTTP 200, body contains the UA headline.

Then in a browser, open `http://localhost:5000/`, verify:
- Hero renders with the flame-gradient background, white uppercase headline, and orange-dark CTA.
- Inter font loads (visual check).
- The language switcher dropdown is visible top-right.
- Switching to **English** reloads the page in English; the cookie persists across reloads.
- Brand wordmark renders with the orange `X`.

Stop the app (Ctrl+C).

- [ ] **Step 4: Verify generated CSS bundle size is sane**

```bash
ls -lh CoreX/wwwroot/css/site.css
```

Expected: a few KB to ~20 KB (Tailwind's tree-shaking strips unused classes).

- [ ] **Step 5: Verify .gitignore is doing its job**

```bash
git status
```

Expected: clean working tree. No `node_modules/`, no `wwwroot/css/site.css`, no `.superpowers/` listed.

- [ ] **Step 6: Phase 0 closing commit (if there is any uncommitted drift)**

If any files changed during smoke-testing (none expected), commit them:

```bash
git status
# only commit if there's something to commit:
# git add … && git commit -m "Phase 0 smoke-test fixups"
```

Otherwise this step is a no-op.

---

## Phase 0 exit checklist

- [ ] `dotnet build CoreX.sln` returns 0 errors.
- [ ] `dotnet test CoreX.sln` shows 2 passing tests.
- [ ] `dotnet run` serves `/` returning HTTP 200 with the Ukrainian headline.
- [ ] Language switcher visibly toggles UA ↔ EN; English shows "Push your limit." headline.
- [ ] No tracked changes in `node_modules/`, `wwwroot/css/site.css`, or `.superpowers/`.
- [ ] Tailwind brand button (`btn-brand`) and ghost button (`btn-ghost`) render correctly.

**Next phase:** `Phase 1 — Auth + Account` (Login, Register, Logout, Profile, MyBookings, role-boundary integration tests). Lands the auth UX layer on top of the existing Identity setup.
