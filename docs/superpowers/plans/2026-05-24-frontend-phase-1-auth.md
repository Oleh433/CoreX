# Frontend Phase 1 — Auth + Account Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Ship the user-facing auth flow (Login, Register, Logout) and authenticated Account pages (Profile, MyBookings) on top of the existing Identity cookie setup, with localized validation, a role-boundary integration test matrix, and a layout that shows authenticated state.

**Architecture:** Add four Razor Pages under `CoreX/Pages/Account/` calling `IUserService` directly via DI (no HTTP self-call to the existing `UsersController`). Localize all form labels and validation messages via `IStringLocalizer<SharedResource>` — `AddDataAnnotationsLocalization` is configured so that `[Required(ErrorMessage = "Required")]` on input models resolves the key against `SharedResource.{uk,en}.resx`. Extend the public layout to render a user menu when authenticated. Cover every protected page with a role × HTTP-verb integration test matrix using `WebApplicationFactory<Program>` from Phase 0.

**Tech Stack:** ASP.NET Core 8 Razor Pages · Identity cookie auth (existing) · `AddDataAnnotationsLocalization` against `SharedResource` · xUnit + `Microsoft.AspNetCore.Mvc.Testing` · EF Core InMemory (test override from Phase 0).

**Spec reference:** `docs/superpowers/specs/2026-05-20-frontend-design.md` — Phase 1 in §11, auth specifics in §5, validation pattern in §9, testing strategy in §10.

---

## Prerequisites

- Phase 0 merged to `master` (HEAD on `master` is `e4b89aa Improve Index hero accessibility`).
- `dotnet build CoreX.sln` returns 0 errors (1 unrelated `CS8618` warning).
- `dotnet test CoreX.sln` shows 2 passing tests from `CoreX.UI.Tests`.
- `appsettings.Development.json` has `Owner:Email` and `Owner:Password` set (required by `IdentityInitializer.AddOwnerAsync`).

## Backend surface used by this phase (verified at master HEAD)

| Surface | Where | Notes |
|---|---|---|
| `IUserService.UserRegisterAsync(UserRegisterRequest)` | `CoreX.Application/ServiceInterfaces/IUserService.cs` | Throws `InvalidOperationException("Passwords do not match.")`, `InvalidOperationException("You must accept the terms of use.")`, `InvalidOperationException("A user with this email already exists.")`, and `InvalidOperationException` for Identity errors. Sends confirmation email on success. **Does not auto-sign-in.** |
| `IUserService.SignInAsync(UserSignInRequest)` | same | Uses `SignInManager.PasswordSignInAsync(..., lockoutOnFailure: true)`. Throws `UnauthorizedAccessException("Account is temporarily locked due to multiple failed sign-in attempts.")`, `UnauthorizedAccessException("Account is not allowed to sign in.")`, `UnauthorizedAccessException("Invalid email or password.")`. |
| `IUserService.SignOutAsync()` | same | Calls `SignInManager.SignOutAsync()`. |
| `UserRegisterRequest` | `CoreX.Application/DTO/UserRegisterRequest.cs` | `required` strings + `bool TermsAccepted`. Annotations: `[Required]`, `[EmailAddress]`, `[StringLength(100, MinimumLength=3/8)]`, `[Compare(nameof(Password))]`, `[Range(typeof(bool), "true", "true")]`. Messages are English-only. |
| `UserSignInRequest` | `CoreX.Application/DTO/UserSignInRequest.cs` | Two strings, no annotations. |
| `ApplicationUser` | `CoreX.Domain/IdentityEntities/ApplicationUser.cs` | `IdentityUser<Guid>` + `string FullName`. |
| `UserManager<ApplicationUser>` | DI | Used by pages to look up the current user. |
| `IBookingService.GetByUserIdAsync(Guid)` | `CoreX.Application/ServiceInterfaces/IBookingService.cs` | Returns `List<BookingResponseDto>` (empty if none). DTO has IDs only — no club/subscription names. |
| `IClubService.GetByIdAsync(Guid)`, `ISubscriptionService.GetByIdAsync(Guid)` | services | Used by MyBookings to resolve club + subscription names. |
| Razor Pages auth conventions | `CoreX/Program.cs:85-95` | `AuthorizeFolder("/Account", "AuthenticatedOnly")`, `AllowAnonymousToPage("/Account/Login")`, `AllowAnonymousToPage("/Account/Register")`. Already in place from Phase 0. **The Phase 1 prerequisite commit `a1dd016` chains `.AddDataAnnotationsLocalization(o => o.DataAnnotationLocalizerProvider = (_, factory) => factory.Create(typeof(SharedResource)))` after `.AddViewLocalization()`.** |

## File map

**New files (paths relative to repo root):**

| File | Responsibility |
|---|---|
| `CoreX/Pages/Account/Models/LoginInput.cs` | Razor Pages input model for `/Account/Login`. |
| `CoreX/Pages/Account/Models/RegisterInput.cs` | Razor Pages input model for `/Account/Register`. |
| `CoreX/Pages/Account/Login.cshtml` | Login page markup. |
| `CoreX/Pages/Account/Login.cshtml.cs` | `LoginModel`: GET renders form, POST calls `IUserService.SignInAsync`, maps `UnauthorizedAccessException` to localized form error, redirects on success. |
| `CoreX/Pages/Account/Register.cshtml` | Register page markup. |
| `CoreX/Pages/Account/Register.cshtml.cs` | `RegisterModel`: GET renders form, POST calls `IUserService.UserRegisterAsync` then `SignInAsync` (auto-login), maps `InvalidOperationException` to localized form error. |
| `CoreX/Pages/Account/Logout.cshtml` | View-less page; layout renders nothing (handler returns redirect). |
| `CoreX/Pages/Account/Logout.cshtml.cs` | `LogoutModel`: POST calls `IUserService.SignOutAsync` and redirects to `/`. GET returns 405 (POST-only). |
| `CoreX/Pages/Account/Profile.cshtml` | Read-only profile display. |
| `CoreX/Pages/Account/Profile.cshtml.cs` | `ProfileModel`: GET loads `ApplicationUser` via `UserManager.GetUserAsync(User)`. |
| `CoreX/Pages/Account/MyBookings.cshtml` | List of the current user's bookings. |
| `CoreX/Pages/Account/MyBookings.cshtml.cs` | `MyBookingsModel`: GET fetches `IBookingService.GetByUserIdAsync(userId)`, joins club + subscription names, exposes `IReadOnlyList<MyBookingRow>` to the view. |
| `CoreX/Resources/Pages/Account/Login.uk.resx` | Login UA strings. |
| `CoreX/Resources/Pages/Account/Login.en.resx` | Login EN strings. |
| `CoreX/Resources/Pages/Account/Register.uk.resx` | Register UA strings. |
| `CoreX/Resources/Pages/Account/Register.en.resx` | Register EN strings. |
| `CoreX/Resources/Pages/Account/Profile.uk.resx` | Profile UA strings. |
| `CoreX/Resources/Pages/Account/Profile.en.resx` | Profile EN strings. |
| `CoreX/Resources/Pages/Account/MyBookings.uk.resx` | MyBookings UA strings. |
| `CoreX/Resources/Pages/Account/MyBookings.en.resx` | MyBookings EN strings. |
| `CoreX.UI.Tests/TestSupport/TestUsers.cs` | Scope-resolved helpers: create users with role, build authenticated `HttpClient`. |
| `CoreX.UI.Tests/TestSupport/AntiforgeryClient.cs` | Helpers that fetch a Razor Pages form and replay the `__RequestVerificationToken` on POST. |
| `CoreX.UI.Tests/Pages/Account/LoginTests.cs` | TDD tests for Login. |
| `CoreX.UI.Tests/Pages/Account/RegisterTests.cs` | TDD tests for Register. |
| `CoreX.UI.Tests/Pages/Account/LogoutTests.cs` | TDD tests for Logout. |
| `CoreX.UI.Tests/Pages/Account/ProfileTests.cs` | TDD tests for Profile. |
| `CoreX.UI.Tests/Pages/Account/MyBookingsTests.cs` | TDD tests for MyBookings (empty + populated). |
| `CoreX.UI.Tests/Pages/Account/AuthPolicyMatrixTests.cs` | Role × verb × page matrix (Theory-driven). |

**Modified files:**

| File | Change |
|---|---|
| `CoreX/Pages/Shared/_Layout.cshtml` | Drop `/TrainingPlan` nav link (left over from Phase 0). Add user menu: when `User.Identity!.IsAuthenticated`, show "Привіт, {FullName}" + Profile + MyBookings + Logout (POST form); otherwise show the existing Sign in / Register links. |
| `CoreX/Resources/SharedResource.uk.resx` | Drop `NavTrainingPlan`. Add `Logout`, `Profile`, `MyBookings`, `WelcomeGreeting` (format string with `{0}`), `OrSeparator`, `AltLoginCta`, `AltRegisterCta`. |
| `CoreX/Resources/SharedResource.en.resx` | Same key set, English values. |

**Out of scope for Phase 1 (handled in later phases):**

- Forgot-password / email-confirmation flow (existing `ConsoleEmailSender` is enough; the link inside the email is created by `IUserService.UserRegisterAsync` but no public page consumes it yet — Phase 6 polish).
- 2FA, external logins, account deletion.
- Editable profile (read-only in Phase 1).
- TrainingPlan-related pages: out of frontend scope entirely (backend service stays untouched).
- `IBookingService.CreateAsync` signature change: Phase 3.
- `/Account/MyBookings` row actions (cancel / re-book): just the list in Phase 1.

---

## Task 1 — Clean up TrainingPlan from Phase 0

**Files:**
- Modify: `CoreX/Pages/Shared/_Layout.cshtml`
- Modify: `CoreX/Resources/SharedResource.uk.resx`
- Modify: `CoreX/Resources/SharedResource.en.resx`

Phase 0 added a `/TrainingPlan` nav link before the scope change to drop TrainingPlan from the frontend. Remove the link and the corresponding resource key now so the rest of Phase 1 doesn't carry a dead reference.

- [ ] **Step 1: Remove the nav link**

In `CoreX/Pages/Shared/_Layout.cshtml`, locate the nav block and delete the line:

```html
<a href="/TrainingPlan">@S["NavTrainingPlan"]</a>
```

(Leave neighbouring nav links intact.)

- [ ] **Step 2: Drop the resource key — UA**

In `CoreX/Resources/SharedResource.uk.resx`, delete the entire `<data name="NavTrainingPlan" xml:space="preserve">...</data>` block.

- [ ] **Step 3: Drop the resource key — EN**

Same edit in `CoreX/Resources/SharedResource.en.resx`.

- [ ] **Step 4: Verify build is still clean**

```bash
dotnet build CoreX.sln --nologo
```

Expected: 0 errors. (Existing CS8618 warning is unrelated.)

- [ ] **Step 5: Verify existing Phase 0 tests still pass**

```bash
dotnet test CoreX.UI.Tests/CoreX.UI.Tests.csproj --nologo --no-build
```

Expected: 2 passing, 0 failing.

- [ ] **Step 6: Commit**

```bash
git add CoreX/Pages/Shared/_Layout.cshtml CoreX/Resources/SharedResource.uk.resx CoreX/Resources/SharedResource.en.resx
git commit -m "Drop TrainingPlan nav link (out of frontend scope)"
```

---

## Task 2 — Shared auth chrome keys + localized validation strategy (already implemented)

**Already landed in commits `764f761` + `a1dd016`.** Captured here so the rest of the plan reads end-to-end.

The original plan stored validation messages in a separate `ValidationMessages` resource type with the marker-class pattern. That pattern requires generated static properties on the marker (the `PublicResXFileCodeGenerator` workflow); the empty marker we created throws at first DataAnnotation use. The fix-forward switched to ASP.NET Core 8's idiomatic pattern:

1. **Validation message keys live in `SharedResource.{uk,en}.resx`** alongside the chrome keys, in one resource type. The 6 validation keys (`Required`, `EmailInvalid`, `PasswordTooShort`, `FullNameLength`, `PasswordsDoNotMatch`, `TermsRequired`) were merged in. The 9 auth-chrome keys (`Logout`, `Profile`, `MyBookings`, `WelcomeGreeting`, `OrSeparator`, `GenericError`, `LockoutError`, `InvalidCredentials`, `EmailAlreadyTaken`) were appended.

2. **`CoreX/Program.cs` chains `.AddDataAnnotationsLocalization`** onto `AddRazorPages().AddViewLocalization()`:

   ```csharp
   .AddDataAnnotationsLocalization(o =>
       o.DataAnnotationLocalizerProvider = (_, factory) =>
           factory.Create(typeof(CoreX.Resources.SharedResource)));
   ```

   With this in place, `[Required(ErrorMessage = "Required")]` on an input model resolves `"Required"` against `SharedResource.{uk,en}.resx` per current culture.

3. **No `ValidationMessages.cs` or `ValidationMessages.{uk,en}.resx`.** Tasks 4 / 5 below use `ErrorMessage = "<key>"` directly — not `ErrorMessageResourceType` / `ErrorMessageResourceName`.

If you arrive at this task during a fresh plan replay, the merged set of `SharedResource` keys (15 entries) is on the branch already; nothing to add. Verify with:

```bash
grep -c "<data name=" CoreX/Resources/SharedResource.uk.resx
```

Expected: 26 (the 11 original Phase 0 keys + the 15 added across this revision and the earlier shared additions).

---

## Task 3 — Test infrastructure for authenticated requests

**Files:**
- Create: `CoreX.UI.Tests/TestSupport/AntiforgeryClient.cs`
- Create: `CoreX.UI.Tests/TestSupport/TestUsers.cs`

Phase 1 tests need to (1) sign users into a real cookie session via `/Account/Login` and (2) replay the antiforgery token returned by the GET form. Both helpers live here so each test stays focused on its assertion.

- [ ] **Step 1: Create `CoreX.UI.Tests/TestSupport/AntiforgeryClient.cs`**

```csharp
using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace CoreX.UI.Tests.TestSupport;

// Helpers that round-trip Razor Pages antiforgery: GET a page, scrape the
// __RequestVerificationToken hidden input + cookie, then POST the form with
// both the body field and the cookie attached.
public static class AntiforgeryClient
{
    private static readonly Regex TokenRegex =
        new(@"name=""__RequestVerificationToken""[^>]*value=""(?<token>[^""]+)""",
            RegexOptions.Compiled);

    public static async Task<(string Token, string Cookie)> FetchAsync(HttpClient client, string url)
    {
        var get = await client.GetAsync(url);
        var html = await get.Content.ReadAsStringAsync();
        var match = TokenRegex.Match(html);
        if (!match.Success)
            throw new InvalidOperationException($"No antiforgery token found at {url}.");

        var cookies = get.Headers.TryGetValues("Set-Cookie", out var values) ? values : Array.Empty<string>();
        var afCookie = cookies.FirstOrDefault(c => c.StartsWith(".AspNetCore.Antiforgery", StringComparison.Ordinal))
            ?? throw new InvalidOperationException($"No antiforgery cookie at {url}.");
        return (match.Groups["token"].Value, afCookie.Split(';')[0]);
    }

    public static HttpRequestMessage BuildPost(
        string url,
        IEnumerable<KeyValuePair<string, string>> form,
        string antiforgeryToken,
        string antiforgeryCookie,
        string? extraCookie = null)
    {
        var fields = form.Append(new("__RequestVerificationToken", antiforgeryToken));
        var req = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new FormUrlEncodedContent(fields),
        };
        var cookieHeader = extraCookie is null ? antiforgeryCookie : $"{antiforgeryCookie}; {extraCookie}";
        req.Headers.Add("Cookie", cookieHeader);
        return req;
    }
}
```

- [ ] **Step 2: Create `CoreX.UI.Tests/TestSupport/TestUsers.cs`**

```csharp
using CoreX.Domain.IdentityEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace CoreX.UI.Tests.TestSupport;

public static class TestUsers
{
    public const string DefaultPassword = "TestUserPass1!";

    // Creates an ApplicationUser via UserManager in the factory's service scope and
    // assigns the given role (creating the role if missing — the IdentityInitializer
    // runs at startup so default roles already exist).
    public static async Task<ApplicationUser> CreateAsync(
        CoreXFactory factory,
        string email,
        string role,
        string fullName = "Test User",
        string password = DefaultPassword)
    {
        using var scope = factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var user = new ApplicationUser
        {
            Email = email,
            UserName = email,
            FullName = fullName,
            EmailConfirmed = true,
        };

        var createResult = await users.CreateAsync(user, password);
        if (!createResult.Succeeded)
            throw new InvalidOperationException(
                $"CreateAsync failed: {string.Join("; ", createResult.Errors.Select(e => e.Description))}");

        var roleResult = await users.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
            throw new InvalidOperationException(
                $"AddToRoleAsync failed: {string.Join("; ", roleResult.Errors.Select(e => e.Description))}");

        return user;
    }

    // Returns an HttpClient that has signed in via /Account/Login. The
    // factory's HttpClient handler keeps cookies between requests when
    // HandleCookies = true (the WebApplicationFactory default).
    public static async Task<HttpClient> SignedInClientAsync(
        CoreXFactory factory,
        string email,
        string password = DefaultPassword)
    {
        var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, "/Account/Login");
        var post = AntiforgeryClient.BuildPost(
            "/Account/Login",
            new Dictionary<string, string>
            {
                ["Input.Email"] = email,
                ["Input.Password"] = password,
            },
            token,
            afCookie);

        var response = await client.SendAsync(post);
        if (response.StatusCode != System.Net.HttpStatusCode.Redirect &&
            response.StatusCode != System.Net.HttpStatusCode.Found)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Sign-in did not redirect (status {(int)response.StatusCode}). Body:\n{body}");
        }

        // CreateClient gives an HttpClient with a handler that captures cookies between
        // requests; the auth cookie set by the POST is now part of its store.
        return client;
    }
}
```

- [ ] **Step 3: Build to make sure these compile against the Phase 0 factory**

```bash
dotnet build CoreX.UI.Tests/CoreX.UI.Tests.csproj --nologo
```

Expected: 0 errors. (`Login.cshtml` doesn't exist yet, so the runtime call to `/Account/Login` will fail; that's fine — these helpers are only invoked after Task 4 lands the page.)

- [ ] **Step 4: Commit**

```bash
git add CoreX.UI.Tests/TestSupport/AntiforgeryClient.cs CoreX.UI.Tests/TestSupport/TestUsers.cs
git commit -m "Add antiforgery + signed-in test client helpers"
```

---

## Task 4 — Login page (TDD)

**Files:**
- Create: `CoreX/Pages/Account/Models/LoginInput.cs`
- Create: `CoreX/Pages/Account/Login.cshtml`
- Create: `CoreX/Pages/Account/Login.cshtml.cs`
- Create: `CoreX/Resources/Pages/Account/Login.uk.resx`
- Create: `CoreX/Resources/Pages/Account/Login.en.resx`
- Test: `CoreX.UI.Tests/Pages/Account/LoginTests.cs`

- [ ] **Step 1: Write the failing tests**

`CoreX.UI.Tests/Pages/Account/LoginTests.cs`:

```csharp
using System.Net;
using CoreX.UI.Tests.TestSupport;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;

namespace CoreX.UI.Tests.Pages.Account;

public class LoginTests : IClassFixture<CoreXFactory>
{
    private readonly CoreXFactory _factory;

    public LoginTests(CoreXFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_Login_ReturnsOk_AndRendersForm()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/Account/Login");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("name=\"Input.Email\"", body);
        Assert.Contains("name=\"Input.Password\"", body);
        Assert.Contains("__RequestVerificationToken", body);
    }

    [Fact]
    public async Task Post_Login_WithValidCredentials_RedirectsAndSetsAuthCookie()
    {
        var email = $"login-ok-{Guid.NewGuid():N}@test";
        await TestUsers.CreateAsync(_factory, email, role: "User");

        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, "/Account/Login");
        var post = AntiforgeryClient.BuildPost(
            "/Account/Login",
            new Dictionary<string, string>
            {
                ["Input.Email"] = email,
                ["Input.Password"] = TestUsers.DefaultPassword,
            },
            token, afCookie);

        var response = await client.SendAsync(post);

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Equal("/", response.Headers.Location?.OriginalString);
        Assert.Contains(response.Headers.GetValues("Set-Cookie"),
            c => c.StartsWith(".AspNetCore.Identity.Application", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Post_Login_WithInvalidPassword_ReturnsForm_WithLocalizedError()
    {
        var email = $"login-bad-{Guid.NewGuid():N}@test";
        await TestUsers.CreateAsync(_factory, email, role: "User");

        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, "/Account/Login");
        var post = AntiforgeryClient.BuildPost(
            "/Account/Login",
            new Dictionary<string, string>
            {
                ["Input.Email"] = email,
                ["Input.Password"] = "WRONG-password-1",
            },
            token, afCookie);

        var response = await client.SendAsync(post);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Невірна електронна адреса або пароль.", body);
        Assert.DoesNotContain(response.Headers.GetValues("Set-Cookie") ?? Array.Empty<string>(),
            c => c.StartsWith(".AspNetCore.Identity.Application", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Post_Login_WithReturnUrl_RedirectsToReturnUrl()
    {
        var email = $"login-ret-{Guid.NewGuid():N}@test";
        await TestUsers.CreateAsync(_factory, email, role: "User");

        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, "/Account/Login?ReturnUrl=%2FAccount%2FProfile");
        var post = AntiforgeryClient.BuildPost(
            "/Account/Login?ReturnUrl=%2FAccount%2FProfile",
            new Dictionary<string, string>
            {
                ["Input.Email"] = email,
                ["Input.Password"] = TestUsers.DefaultPassword,
            },
            token, afCookie);

        var response = await client.SendAsync(post);

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Equal("/Account/Profile", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_Login_WithEnglishCulture_ShowsEnglishLabels()
    {
        var client = _factory.CreateClient();
        var req = new HttpRequestMessage(HttpMethod.Get, "/Account/Login");
        req.Headers.Add("Cookie",
            $"{CookieRequestCultureProvider.DefaultCookieName}=c=en|uic=en");

        var response = await client.SendAsync(req);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Sign in", body);
        Assert.DoesNotContain("Увійти", body);
    }
}
```

- [ ] **Step 2: Run the tests — confirm they fail**

```bash
dotnet test CoreX.UI.Tests/CoreX.UI.Tests.csproj --nologo --filter "FullyQualifiedName~LoginTests"
```

Expected: all 5 fail (Login page doesn't exist; GETs return 404; antiforgery scrape fails).

- [ ] **Step 3: Create the input model**

`CoreX/Pages/Account/Models/LoginInput.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace CoreX.Pages.Account.Models;

public class LoginInput
{
    [Required(ErrorMessage = "Required")]
    [EmailAddress(ErrorMessage = "EmailInvalid")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Required")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
```

The `ErrorMessage` strings are keys looked up against `SharedResource.{uk,en}.resx` via the `AddDataAnnotationsLocalization` wiring from the prerequisite commit.

- [ ] **Step 4: Create the PageModel**

`CoreX/Pages/Account/Login.cshtml.cs`:

```csharp
using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using CoreX.Pages.Account.Models;
using CoreX.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace CoreX.Pages.Account;

public class LoginModel : PageModel
{
    private readonly IUserService _users;
    private readonly IStringLocalizer<SharedResource> _shared;

    public LoginModel(IUserService users, IStringLocalizer<SharedResource> shared)
    {
        _users = users;
        _shared = shared;
    }

    [BindProperty]
    public LoginInput Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    public void OnGet(string? returnUrl = null) => ReturnUrl = returnUrl;

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;

        if (!ModelState.IsValid)
            return Page();

        try
        {
            await _users.SignInAsync(new UserSignInRequest
            {
                Email = Input.Email,
                Password = Input.Password,
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            ModelState.AddModelError(string.Empty, MapSignInError(ex.Message));
            return Page();
        }

        return LocalRedirect(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);
    }

    private string MapSignInError(string serviceMessage) => serviceMessage switch
    {
        "Account is temporarily locked due to multiple failed sign-in attempts."
            => _shared["LockoutError"],
        "Account is not allowed to sign in."
            => _shared["InvalidCredentials"],
        _ => _shared["InvalidCredentials"],
    };
}
```

- [ ] **Step 5: Create the view**

`CoreX/Pages/Account/Login.cshtml`:

```cshtml
@page
@model CoreX.Pages.Account.LoginModel
@{
    ViewData["Title"] = L["Title"].Value;
}

<section class="max-w-md mx-auto px-4 py-12 md:py-16">
    <h1 class="text-3xl font-black uppercase tracking-tight">@L["Title"]</h1>
    <p class="mt-3 text-ink-500">@L["Subtitle"]</p>

    <form method="post" class="mt-8 space-y-5"
          asp-route-returnUrl="@Model.ReturnUrl"
          novalidate>
        <div asp-validation-summary="ModelOnly" class="rounded-card border border-danger bg-danger/5 text-danger px-4 py-3 text-sm"></div>

        <div>
            <label asp-for="Input.Email" class="block text-xs font-semibold uppercase tracking-wide text-ink-800">
                @L["EmailLabel"]
            </label>
            <input asp-for="Input.Email" autocomplete="email" required
                   class="mt-1 block w-full rounded-card border-ink-200 focus:border-brand-500 focus:ring-brand-500" />
            <span asp-validation-for="Input.Email" class="mt-1 block text-sm text-danger"></span>
        </div>

        <div>
            <label asp-for="Input.Password" class="block text-xs font-semibold uppercase tracking-wide text-ink-800">
                @L["PasswordLabel"]
            </label>
            <input asp-for="Input.Password" type="password" autocomplete="current-password" required
                   class="mt-1 block w-full rounded-card border-ink-200 focus:border-brand-500 focus:ring-brand-500" />
            <span asp-validation-for="Input.Password" class="mt-1 block text-sm text-danger"></span>
        </div>

        <button type="submit" class="btn-brand w-full">@L["Submit"]</button>
    </form>

    <p class="mt-6 text-sm text-ink-500 text-center">
        @L["NoAccountPrompt"]
        <a asp-page="/Account/Register" class="font-semibold text-brand-500 hover:underline">@L["RegisterCta"]</a>
    </p>
</section>
```

- [ ] **Step 6: Create the UA resources**

`CoreX/Resources/Pages/Account/Login.uk.resx`:

| Name | Value |
|---|---|
| `Title` | `Увійти` |
| `Subtitle` | `Раді знову бачити.` |
| `EmailLabel` | `Електронна пошта` |
| `PasswordLabel` | `Пароль` |
| `Submit` | `Увійти` |
| `NoAccountPrompt` | `Ще немає акаунту?` |
| `RegisterCta` | `Зареєструватися` |

- [ ] **Step 7: Create the EN resources**

`CoreX/Resources/Pages/Account/Login.en.resx`:

| Name | Value |
|---|---|
| `Title` | `Sign in` |
| `Subtitle` | `Welcome back.` |
| `EmailLabel` | `Email` |
| `PasswordLabel` | `Password` |
| `Submit` | `Sign in` |
| `NoAccountPrompt` | `No account yet?` |
| `RegisterCta` | `Register` |

- [ ] **Step 8: Run the tests — confirm they pass**

```bash
dotnet test CoreX.UI.Tests/CoreX.UI.Tests.csproj --nologo --filter "FullyQualifiedName~LoginTests"
```

Expected: 5 passing, 0 failing.

- [ ] **Step 9: Commit**

```bash
git add CoreX/Pages/Account/Models/LoginInput.cs CoreX/Pages/Account/Login.cshtml CoreX/Pages/Account/Login.cshtml.cs CoreX/Resources/Pages/Account/Login.uk.resx CoreX/Resources/Pages/Account/Login.en.resx CoreX.UI.Tests/Pages/Account/LoginTests.cs
git commit -m "Add /Account/Login with localized validation + TDD"
```

---

## Task 5 — Register page (TDD)

**Files:**
- Create: `CoreX/Pages/Account/Models/RegisterInput.cs`
- Create: `CoreX/Pages/Account/Register.cshtml`
- Create: `CoreX/Pages/Account/Register.cshtml.cs`
- Create: `CoreX/Resources/Pages/Account/Register.uk.resx`
- Create: `CoreX/Resources/Pages/Account/Register.en.resx`
- Test: `CoreX.UI.Tests/Pages/Account/RegisterTests.cs`

- [ ] **Step 1: Write the failing tests**

`CoreX.UI.Tests/Pages/Account/RegisterTests.cs`:

```csharp
using System.Net;
using CoreX.UI.Tests.TestSupport;

namespace CoreX.UI.Tests.Pages.Account;

public class RegisterTests : IClassFixture<CoreXFactory>
{
    private readonly CoreXFactory _factory;

    public RegisterTests(CoreXFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_Register_ReturnsOk_AndRendersForm()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/Account/Register");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("name=\"Input.FullName\"", body);
        Assert.Contains("name=\"Input.Email\"", body);
        Assert.Contains("name=\"Input.Password\"", body);
        Assert.Contains("name=\"Input.ConfirmPassword\"", body);
        Assert.Contains("name=\"Input.TermsAccepted\"", body);
    }

    [Fact]
    public async Task Post_Register_WithValidInput_CreatesUserAndAutoLogsIn()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        var email = $"reg-{Guid.NewGuid():N}@test";
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, "/Account/Register");
        var post = AntiforgeryClient.BuildPost(
            "/Account/Register",
            new Dictionary<string, string>
            {
                ["Input.FullName"] = "Test Person",
                ["Input.Email"] = email,
                ["Input.Password"] = "ValidPass1!",
                ["Input.ConfirmPassword"] = "ValidPass1!",
                ["Input.TermsAccepted"] = "true",
            },
            token, afCookie);

        var response = await client.SendAsync(post);

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Equal("/", response.Headers.Location?.OriginalString);
        Assert.Contains(response.Headers.GetValues("Set-Cookie"),
            c => c.StartsWith(".AspNetCore.Identity.Application", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Post_Register_WithMismatchedPasswords_ReturnsForm_WithLocalizedError()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, "/Account/Register");
        var post = AntiforgeryClient.BuildPost(
            "/Account/Register",
            new Dictionary<string, string>
            {
                ["Input.FullName"] = "Test Person",
                ["Input.Email"] = $"reg-mm-{Guid.NewGuid():N}@test",
                ["Input.Password"] = "ValidPass1!",
                ["Input.ConfirmPassword"] = "DifferentPass1!",
                ["Input.TermsAccepted"] = "true",
            },
            token, afCookie);

        var response = await client.SendAsync(post);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Паролі не співпадають.", body);
    }

    [Fact]
    public async Task Post_Register_WithTermsNotAccepted_ReturnsForm_WithLocalizedError()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, "/Account/Register");
        var post = AntiforgeryClient.BuildPost(
            "/Account/Register",
            new Dictionary<string, string>
            {
                ["Input.FullName"] = "Test Person",
                ["Input.Email"] = $"reg-terms-{Guid.NewGuid():N}@test",
                ["Input.Password"] = "ValidPass1!",
                ["Input.ConfirmPassword"] = "ValidPass1!",
                ["Input.TermsAccepted"] = "false",
            },
            token, afCookie);

        var response = await client.SendAsync(post);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Потрібно прийняти умови використання.", body);
    }

    [Fact]
    public async Task Post_Register_WithDuplicateEmail_ReturnsForm_WithLocalizedError()
    {
        var email = $"reg-dup-{Guid.NewGuid():N}@test";
        await TestUsers.CreateAsync(_factory, email, role: "User");

        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, "/Account/Register");
        var post = AntiforgeryClient.BuildPost(
            "/Account/Register",
            new Dictionary<string, string>
            {
                ["Input.FullName"] = "Other Person",
                ["Input.Email"] = email,
                ["Input.Password"] = "ValidPass1!",
                ["Input.ConfirmPassword"] = "ValidPass1!",
                ["Input.TermsAccepted"] = "true",
            },
            token, afCookie);

        var response = await client.SendAsync(post);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Користувач з такою електронною адресою вже існує.", body);
    }
}
```

- [ ] **Step 2: Run the tests — confirm they fail**

```bash
dotnet test CoreX.UI.Tests/CoreX.UI.Tests.csproj --nologo --filter "FullyQualifiedName~RegisterTests"
```

Expected: 5 failing (page doesn't exist).

- [ ] **Step 3: Create the input model**

`CoreX/Pages/Account/Models/RegisterInput.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace CoreX.Pages.Account.Models;

public class RegisterInput
{
    [Required(ErrorMessage = "Required")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "FullNameLength")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Required")]
    [EmailAddress(ErrorMessage = "EmailInvalid")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "PasswordTooShort")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Required")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "PasswordsDoNotMatch")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Range(typeof(bool), "true", "true", ErrorMessage = "TermsRequired")]
    public bool TermsAccepted { get; set; }
}
```

`ErrorMessage` strings are keys resolved against `SharedResource.{uk,en}.resx`.

- [ ] **Step 4: Create the PageModel**

`CoreX/Pages/Account/Register.cshtml.cs`:

```csharp
using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using CoreX.Pages.Account.Models;
using CoreX.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace CoreX.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly IUserService _users;
    private readonly IStringLocalizer<SharedResource> _shared;

    public RegisterModel(IUserService users, IStringLocalizer<SharedResource> shared)
    {
        _users = users;
        _shared = shared;
    }

    [BindProperty]
    public RegisterInput Input { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        try
        {
            await _users.UserRegisterAsync(new UserRegisterRequest
            {
                FullName = Input.FullName,
                Email = Input.Email,
                Password = Input.Password,
                ConfirmPassword = Input.ConfirmPassword,
                TermsAccepted = Input.TermsAccepted,
            });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, MapRegisterError(ex.Message));
            return Page();
        }

        try
        {
            await _users.SignInAsync(new UserSignInRequest
            {
                Email = Input.Email,
                Password = Input.Password,
            });
        }
        catch (UnauthorizedAccessException)
        {
            // Registration succeeded but auto-login didn't (lockout, etc.).
            // Send the user to the login page instead of dead-ending here.
            return RedirectToPage("/Account/Login");
        }

        return LocalRedirect("/");
    }

    private string MapRegisterError(string serviceMessage) => serviceMessage switch
    {
        "A user with this email already exists."
            => _shared["EmailAlreadyTaken"],
        // Passwords do not match / Terms not accepted are blocked by DataAnnotations first;
        // any other service error is shown verbatim once (it's an English string from the
        // service layer — acceptable for v1, can be expanded later as more keys are added).
        _ => serviceMessage,
    };
}
```

- [ ] **Step 5: Create the view**

`CoreX/Pages/Account/Register.cshtml`:

```cshtml
@page
@model CoreX.Pages.Account.RegisterModel
@{
    ViewData["Title"] = L["Title"].Value;
}

<section class="max-w-md mx-auto px-4 py-12 md:py-16">
    <h1 class="text-3xl font-black uppercase tracking-tight">@L["Title"]</h1>
    <p class="mt-3 text-ink-500">@L["Subtitle"]</p>

    <form method="post" class="mt-8 space-y-5" novalidate>
        <div asp-validation-summary="ModelOnly" class="rounded-card border border-danger bg-danger/5 text-danger px-4 py-3 text-sm"></div>

        <div>
            <label asp-for="Input.FullName" class="block text-xs font-semibold uppercase tracking-wide text-ink-800">
                @L["FullNameLabel"]
            </label>
            <input asp-for="Input.FullName" autocomplete="name" required
                   class="mt-1 block w-full rounded-card border-ink-200 focus:border-brand-500 focus:ring-brand-500" />
            <span asp-validation-for="Input.FullName" class="mt-1 block text-sm text-danger"></span>
        </div>

        <div>
            <label asp-for="Input.Email" class="block text-xs font-semibold uppercase tracking-wide text-ink-800">
                @L["EmailLabel"]
            </label>
            <input asp-for="Input.Email" autocomplete="email" required
                   class="mt-1 block w-full rounded-card border-ink-200 focus:border-brand-500 focus:ring-brand-500" />
            <span asp-validation-for="Input.Email" class="mt-1 block text-sm text-danger"></span>
        </div>

        <div>
            <label asp-for="Input.Password" class="block text-xs font-semibold uppercase tracking-wide text-ink-800">
                @L["PasswordLabel"]
            </label>
            <input asp-for="Input.Password" type="password" autocomplete="new-password" required
                   class="mt-1 block w-full rounded-card border-ink-200 focus:border-brand-500 focus:ring-brand-500" />
            <span asp-validation-for="Input.Password" class="mt-1 block text-sm text-danger"></span>
        </div>

        <div>
            <label asp-for="Input.ConfirmPassword" class="block text-xs font-semibold uppercase tracking-wide text-ink-800">
                @L["ConfirmPasswordLabel"]
            </label>
            <input asp-for="Input.ConfirmPassword" type="password" autocomplete="new-password" required
                   class="mt-1 block w-full rounded-card border-ink-200 focus:border-brand-500 focus:ring-brand-500" />
            <span asp-validation-for="Input.ConfirmPassword" class="mt-1 block text-sm text-danger"></span>
        </div>

        <label class="flex items-start gap-3 text-sm text-ink-800">
            <input asp-for="Input.TermsAccepted" type="checkbox" class="mt-1 rounded border-ink-200 text-brand-500 focus:ring-brand-500" />
            <span>@L["TermsLabel"]</span>
        </label>
        <span asp-validation-for="Input.TermsAccepted" class="block text-sm text-danger"></span>

        <button type="submit" class="btn-brand w-full">@L["Submit"]</button>
    </form>

    <p class="mt-6 text-sm text-ink-500 text-center">
        @L["HaveAccountPrompt"]
        <a asp-page="/Account/Login" class="font-semibold text-brand-500 hover:underline">@L["LoginCta"]</a>
    </p>
</section>
```

- [ ] **Step 6: Create the UA resources**

`CoreX/Resources/Pages/Account/Register.uk.resx`:

| Name | Value |
|---|---|
| `Title` | `Реєстрація` |
| `Subtitle` | `Перетни свою межу разом з CoreX.` |
| `FullNameLabel` | `Повне ім'я` |
| `EmailLabel` | `Електронна пошта` |
| `PasswordLabel` | `Пароль` |
| `ConfirmPasswordLabel` | `Підтвердження пароля` |
| `TermsLabel` | `Я приймаю умови використання та політику конфіденційності.` |
| `Submit` | `Зареєструватися` |
| `HaveAccountPrompt` | `Вже маєте акаунт?` |
| `LoginCta` | `Увійти` |

- [ ] **Step 7: Create the EN resources**

`CoreX/Resources/Pages/Account/Register.en.resx`:

| Name | Value |
|---|---|
| `Title` | `Register` |
| `Subtitle` | `Push your limit with CoreX.` |
| `FullNameLabel` | `Full name` |
| `EmailLabel` | `Email` |
| `PasswordLabel` | `Password` |
| `ConfirmPasswordLabel` | `Confirm password` |
| `TermsLabel` | `I accept the terms of use and privacy policy.` |
| `Submit` | `Register` |
| `HaveAccountPrompt` | `Already have an account?` |
| `LoginCta` | `Sign in` |

- [ ] **Step 8: Run the tests — confirm they pass**

```bash
dotnet test CoreX.UI.Tests/CoreX.UI.Tests.csproj --nologo --filter "FullyQualifiedName~RegisterTests"
```

Expected: 5 passing.

- [ ] **Step 9: Commit**

```bash
git add CoreX/Pages/Account/Models/RegisterInput.cs CoreX/Pages/Account/Register.cshtml CoreX/Pages/Account/Register.cshtml.cs CoreX/Resources/Pages/Account/Register.uk.resx CoreX/Resources/Pages/Account/Register.en.resx CoreX.UI.Tests/Pages/Account/RegisterTests.cs
git commit -m "Add /Account/Register with auto-login on success + TDD"
```

---

## Task 6 — Logout page (TDD)

**Files:**
- Create: `CoreX/Pages/Account/Logout.cshtml`
- Create: `CoreX/Pages/Account/Logout.cshtml.cs`
- Test: `CoreX.UI.Tests/Pages/Account/LogoutTests.cs`

Logout is a POST-only action triggered from the layout's user menu (Task 9). The `.cshtml` exists only so the page is discoverable by the Razor Pages routing; no body is rendered.

- [ ] **Step 1: Write the failing tests**

`CoreX.UI.Tests/Pages/Account/LogoutTests.cs`:

```csharp
using System.Net;
using CoreX.UI.Tests.TestSupport;

namespace CoreX.UI.Tests.Pages.Account;

public class LogoutTests : IClassFixture<CoreXFactory>
{
    private readonly CoreXFactory _factory;

    public LogoutTests(CoreXFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_Logout_Anonymous_RedirectsToLogin()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        var response = await client.GetAsync("/Account/Logout");

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.StartsWith("/Account/Login", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_Logout_Authenticated_RedirectsHome_AndClearsAuthCookie()
    {
        var email = $"logout-{Guid.NewGuid():N}@test";
        await TestUsers.CreateAsync(_factory, email, role: "User");
        var client = await TestUsers.SignedInClientAsync(_factory, email);

        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, "/Account/Profile");
        // The Profile page also emits a fresh antiforgery token; reuse it for the logout POST.
        var post = AntiforgeryClient.BuildPost(
            "/Account/Logout",
            Array.Empty<KeyValuePair<string, string>>(),
            token,
            afCookie);
        var response = await client.SendAsync(post);

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Equal("/", response.Headers.Location?.OriginalString);

        // Identity emits a Set-Cookie that clears the auth cookie by setting its
        // value to empty + an expired Expires attribute.
        Assert.Contains(response.Headers.GetValues("Set-Cookie"),
            c => c.StartsWith(".AspNetCore.Identity.Application=;", StringComparison.Ordinal));
    }
}
```

- [ ] **Step 2: Run the tests — confirm they fail**

Expected: both tests fail (no page yet).

- [ ] **Step 3: Create the PageModel**

`CoreX/Pages/Account/Logout.cshtml.cs`:

```csharp
using CoreX.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Account;

public class LogoutModel : PageModel
{
    private readonly IUserService _users;

    public LogoutModel(IUserService users) => _users = users;

    public IActionResult OnGet() => RedirectToPage("/Account/Login");

    public async Task<IActionResult> OnPostAsync()
    {
        await _users.SignOutAsync();
        return LocalRedirect("/");
    }
}
```

- [ ] **Step 4: Create the view (empty)**

`CoreX/Pages/Account/Logout.cshtml`:

```cshtml
@page
@model CoreX.Pages.Account.LogoutModel
```

- [ ] **Step 5: Run the tests — confirm they pass**

Expected: 2 passing.

- [ ] **Step 6: Commit**

```bash
git add CoreX/Pages/Account/Logout.cshtml CoreX/Pages/Account/Logout.cshtml.cs CoreX.UI.Tests/Pages/Account/LogoutTests.cs
git commit -m "Add /Account/Logout (POST-only) + TDD"
```

---

## Task 7 — Profile page (TDD)

**Files:**
- Create: `CoreX/Pages/Account/Profile.cshtml`
- Create: `CoreX/Pages/Account/Profile.cshtml.cs`
- Create: `CoreX/Resources/Pages/Account/Profile.uk.resx`
- Create: `CoreX/Resources/Pages/Account/Profile.en.resx`
- Test: `CoreX.UI.Tests/Pages/Account/ProfileTests.cs`

- [ ] **Step 1: Write the failing tests**

`CoreX.UI.Tests/Pages/Account/ProfileTests.cs`:

```csharp
using System.Net;
using CoreX.UI.Tests.TestSupport;

namespace CoreX.UI.Tests.Pages.Account;

public class ProfileTests : IClassFixture<CoreXFactory>
{
    private readonly CoreXFactory _factory;

    public ProfileTests(CoreXFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_Profile_Anonymous_RedirectsToLogin()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
        var response = await client.GetAsync("/Account/Profile");

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.StartsWith("/Account/Login", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_Profile_Authenticated_ShowsFullNameAndEmail()
    {
        var email = $"profile-{Guid.NewGuid():N}@test";
        await TestUsers.CreateAsync(_factory, email, role: "User", fullName: "Olha Bilash");
        var client = await TestUsers.SignedInClientAsync(_factory, email);

        var response = await client.GetAsync("/Account/Profile");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Olha Bilash", body);
        Assert.Contains(email, body);
    }
}
```

- [ ] **Step 2: Run — confirm failure**

Expected: 2 failing.

- [ ] **Step 3: Create the PageModel**

`CoreX/Pages/Account/Profile.cshtml.cs`:

```csharp
using CoreX.Domain.IdentityEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Account;

public class ProfileModel : PageModel
{
    private readonly UserManager<ApplicationUser> _users;

    public ProfileModel(UserManager<ApplicationUser> users) => _users = users;

    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _users.GetUserAsync(User);
        if (user is null)
            return RedirectToPage("/Account/Login");

        FullName = user.FullName;
        Email = user.Email ?? string.Empty;
        return Page();
    }
}
```

- [ ] **Step 4: Create the view**

`CoreX/Pages/Account/Profile.cshtml`:

```cshtml
@page
@model CoreX.Pages.Account.ProfileModel
@{
    ViewData["Title"] = L["Title"].Value;
}

<section class="max-w-2xl mx-auto px-4 py-12 md:py-16">
    <p class="text-xs font-semibold tracking-[0.2em] uppercase text-brand-500">@L["Eyebrow"]</p>
    <h1 class="mt-2 text-3xl md:text-4xl font-black uppercase tracking-tight">@Model.FullName</h1>

    <dl class="mt-8 grid grid-cols-1 md:grid-cols-2 gap-6 text-sm">
        <div>
            <dt class="text-ink-500 uppercase tracking-wide text-xs font-semibold">@L["EmailLabel"]</dt>
            <dd class="mt-1 text-ink-900 break-all">@Model.Email</dd>
        </div>
    </dl>

    <div class="mt-10 flex gap-3">
        <a asp-page="/Account/MyBookings" class="btn-brand">@S["MyBookings"]</a>
    </div>
</section>
```

- [ ] **Step 5: Create the UA resources**

`CoreX/Resources/Pages/Account/Profile.uk.resx`:

| Name | Value |
|---|---|
| `Title` | `Профіль` |
| `Eyebrow` | `Ваш акаунт` |
| `EmailLabel` | `Електронна пошта` |

- [ ] **Step 6: Create the EN resources**

`CoreX/Resources/Pages/Account/Profile.en.resx`:

| Name | Value |
|---|---|
| `Title` | `Profile` |
| `Eyebrow` | `Your account` |
| `EmailLabel` | `Email` |

- [ ] **Step 7: Run — confirm pass**

Expected: 2 passing.

- [ ] **Step 8: Commit**

```bash
git add CoreX/Pages/Account/Profile.cshtml CoreX/Pages/Account/Profile.cshtml.cs CoreX/Resources/Pages/Account/Profile.uk.resx CoreX/Resources/Pages/Account/Profile.en.resx CoreX.UI.Tests/Pages/Account/ProfileTests.cs
git commit -m "Add /Account/Profile (read-only) + TDD"
```

---

## Task 8 — MyBookings page (TDD)

**Files:**
- Create: `CoreX/Pages/Account/MyBookings.cshtml`
- Create: `CoreX/Pages/Account/MyBookings.cshtml.cs`
- Create: `CoreX/Resources/Pages/Account/MyBookings.uk.resx`
- Create: `CoreX/Resources/Pages/Account/MyBookings.en.resx`
- Test: `CoreX.UI.Tests/Pages/Account/MyBookingsTests.cs`

The page joins each `BookingResponseDto` with `IClubService.GetByIdAsync` + `ISubscriptionService.GetByIdAsync` to surface human-readable club and subscription names — the DTO only carries IDs.

- [ ] **Step 1: Write the failing tests**

`CoreX.UI.Tests/Pages/Account/MyBookingsTests.cs`:

```csharp
using System.Net;
using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using CoreX.Domain.Entities;
using CoreX.UI.Tests.TestSupport;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using CoreX.Domain.IdentityEntities;

namespace CoreX.UI.Tests.Pages.Account;

public class MyBookingsTests : IClassFixture<CoreXFactory>
{
    private readonly CoreXFactory _factory;

    public MyBookingsTests(CoreXFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_MyBookings_Anonymous_RedirectsToLogin()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
        var response = await client.GetAsync("/Account/MyBookings");

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.StartsWith("/Account/Login", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_MyBookings_AuthenticatedWithNoBookings_RendersEmptyState()
    {
        var email = $"mb-empty-{Guid.NewGuid():N}@test";
        await TestUsers.CreateAsync(_factory, email, role: "User");
        var client = await TestUsers.SignedInClientAsync(_factory, email);

        var response = await client.GetAsync("/Account/MyBookings");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Поки що бронювань немає.", body);
    }
}
```

(A test covering "with bookings" requires standing up a Club + Subscription + Booking in the in-memory DB via the appropriate domain entities. Add it as part of Phase 3 when bookings get a creation flow — for Phase 1 the empty-state test plus the auth redirect cover the surface this page introduces.)

- [ ] **Step 2: Run — confirm failure**

Expected: 2 failing.

- [ ] **Step 3: Create the PageModel**

`CoreX/Pages/Account/MyBookings.cshtml.cs`:

```csharp
using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using CoreX.Domain.IdentityEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Account;

public class MyBookingsModel : PageModel
{
    private readonly IBookingService _bookings;
    private readonly IClubService _clubs;
    private readonly ISubscriptionService _subscriptions;
    private readonly UserManager<ApplicationUser> _users;

    public MyBookingsModel(
        IBookingService bookings,
        IClubService clubs,
        ISubscriptionService subscriptions,
        UserManager<ApplicationUser> users)
    {
        _bookings = bookings;
        _clubs = clubs;
        _subscriptions = subscriptions;
        _users = users;
    }

    public IReadOnlyList<MyBookingRow> Rows { get; private set; } = Array.Empty<MyBookingRow>();

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _users.GetUserAsync(User);
        if (user is null)
            return RedirectToPage("/Account/Login");

        var bookings = await _bookings.GetByUserIdAsync(user.Id);

        var rows = new List<MyBookingRow>(bookings.Count);
        foreach (var b in bookings)
        {
            var club = await _clubs.GetByIdAsync(b.ClubId);
            var sub = await _subscriptions.GetByIdAsync(b.SubscriptionId);
            rows.Add(new MyBookingRow(
                b.Id,
                club?.Name ?? "—",
                sub?.Title ?? "—",
                b.Status,
                b.CreatedAt));
        }

        Rows = rows;
        return Page();
    }

    public sealed record MyBookingRow(
        Guid Id,
        string ClubName,
        string SubscriptionName,
        string Status,
        DateTime CreatedAt);
}
```

Verified at master HEAD:
- `IClubService.GetByIdAsync(Guid) → Task<ClubResponseDto?>` — `ClubResponseDto.Name` is a non-null string.
- `ISubscriptionService.GetByIdAsync(Guid) → Task<SubscriptionResponseDto?>` — note the display field is `Title`, not `Name`.

- [ ] **Step 4: Create the view**

`CoreX/Pages/Account/MyBookings.cshtml`:

```cshtml
@page
@model CoreX.Pages.Account.MyBookingsModel
@{
    ViewData["Title"] = L["Title"].Value;
}

<section class="max-w-4xl mx-auto px-4 py-12 md:py-16">
    <h1 class="text-3xl md:text-4xl font-black uppercase tracking-tight">@L["Title"]</h1>

    @if (Model.Rows.Count == 0)
    {
        <p class="mt-8 text-ink-500">@L["EmptyState"]</p>
    }
    else
    {
        <ul class="mt-8 divide-y divide-ink-200">
            @foreach (var row in Model.Rows)
            {
                <li class="py-5 flex flex-col md:flex-row md:items-center md:justify-between gap-2">
                    <div>
                        <p class="font-semibold text-ink-900">@row.SubscriptionName</p>
                        <p class="text-sm text-ink-500">@row.ClubName · @row.CreatedAt.ToString("d")</p>
                    </div>
                    <span class="text-xs font-semibold uppercase tracking-wide rounded-pill px-3 py-1 bg-brand-50 text-brand-700">
                        @row.Status
                    </span>
                </li>
            }
        </ul>
    }
</section>
```

- [ ] **Step 5: Create the UA resources**

`CoreX/Resources/Pages/Account/MyBookings.uk.resx`:

| Name | Value |
|---|---|
| `Title` | `Мої бронювання` |
| `EmptyState` | `Поки що бронювань немає.` |

- [ ] **Step 6: Create the EN resources**

`CoreX/Resources/Pages/Account/MyBookings.en.resx`:

| Name | Value |
|---|---|
| `Title` | `My bookings` |
| `EmptyState` | `No bookings yet.` |

- [ ] **Step 7: Run — confirm pass**

Expected: 2 passing.

- [ ] **Step 8: Commit**

```bash
git add CoreX/Pages/Account/MyBookings.cshtml CoreX/Pages/Account/MyBookings.cshtml.cs CoreX/Resources/Pages/Account/MyBookings.uk.resx CoreX/Resources/Pages/Account/MyBookings.en.resx CoreX.UI.Tests/Pages/Account/MyBookingsTests.cs
git commit -m "Add /Account/MyBookings (list + empty state) + TDD"
```

---

## Task 9 — Layout user menu

**Files:**
- Modify: `CoreX/Pages/Shared/_Layout.cshtml`

Replace the static "Sign in / Register" pair with a conditional user menu: greeting + Profile + MyBookings + Logout (POST form) when authenticated; the existing Sign in / Register links otherwise.

- [ ] **Step 1: Locate the existing auth links in `_Layout.cshtml`**

They look approximately like:

```html
<a asp-page="/Account/Login" class="btn-ghost">@S["SignIn"]</a>
<a asp-page="/Account/Register" class="btn-brand">@S["Register"]</a>
```

(The exact surrounding HTML stays as-is — only the auth-action cluster changes.)

- [ ] **Step 2: Replace with the conditional menu**

```cshtml
@if (User.Identity?.IsAuthenticated == true)
{
    <div class="flex items-center gap-3">
        <span class="text-sm text-ink-500 hidden md:inline">@string.Format(S["WelcomeGreeting"].Value, User.Identity!.Name)</span>
        <a asp-page="/Account/Profile" class="btn-ghost">@S["Profile"]</a>
        <a asp-page="/Account/MyBookings" class="btn-ghost">@S["MyBookings"]</a>
        <form method="post" asp-page="/Account/Logout" class="inline">
            <button type="submit" class="btn-ghost">@S["Logout"]</button>
        </form>
    </div>
}
else
{
    <div class="flex items-center gap-3">
        <a asp-page="/Account/Login" class="btn-ghost">@S["SignIn"]</a>
        <a asp-page="/Account/Register" class="btn-brand">@S["Register"]</a>
    </div>
}
```

Note: `User.Identity!.Name` will be the user's email (the Identity setup uses `UserName = email`). For a friendlier display name we'd need to populate a claim with `FullName` at sign-in — out of scope for Phase 1.

- [ ] **Step 3: Run the full Phase 0 + Phase 1 test suite**

```bash
dotnet test CoreX.UI.Tests/CoreX.UI.Tests.csproj --nologo
```

Expected: all tests pass (Phase 0's 2 + Phase 1's ~14 so far). Note: `Get_Index_ReturnsOk_AndUkrainianHeadline` still hits the home page anonymously; the layout's else-branch is what renders. The test does NOT assert on auth chrome, so this change is safe.

- [ ] **Step 4: Commit**

```bash
git add CoreX/Pages/Shared/_Layout.cshtml
git commit -m "Show user menu (Profile, MyBookings, Logout) when authenticated"
```

---

## Task 10 — Role-boundary integration matrix

**Files:**
- Create: `CoreX.UI.Tests/Pages/Account/AuthPolicyMatrixTests.cs`

One xUnit `[Theory]` that drives every (role, page) pair for the Account folder + asserts the right response — 200 OK if allowed, 302 redirect to `/Account/Login` for anonymous, 200 OK for authenticated regardless of role (all Account pages require only `AuthenticatedOnly`).

- [ ] **Step 1: Create the matrix test**

`CoreX.UI.Tests/Pages/Account/AuthPolicyMatrixTests.cs`:

```csharp
using System.Net;
using CoreX.UI.Tests.TestSupport;

namespace CoreX.UI.Tests.Pages.Account;

public class AuthPolicyMatrixTests : IClassFixture<CoreXFactory>
{
    private readonly CoreXFactory _factory;

    public AuthPolicyMatrixTests(CoreXFactory factory) => _factory = factory;

    public static IEnumerable<object[]> AnonymousCases() =>
        new[]
        {
            new object[] { "/Account/Login",      HttpStatusCode.OK },
            new object[] { "/Account/Register",   HttpStatusCode.OK },
            new object[] { "/Account/Profile",    HttpStatusCode.Found },
            new object[] { "/Account/MyBookings", HttpStatusCode.Found },
            new object[] { "/Account/Logout",     HttpStatusCode.Found },
        };

    [Theory]
    [MemberData(nameof(AnonymousCases))]
    public async Task Anonymous_AccessToAccountPages_BehavesAsExpected(string url, HttpStatusCode expected)
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
        var response = await client.GetAsync(url);

        Assert.Equal(expected, response.StatusCode);
        if (expected == HttpStatusCode.Found)
            Assert.StartsWith("/Account/Login", response.Headers.Location?.OriginalString);
    }

    public static IEnumerable<object[]> AuthenticatedPages() =>
        new[]
        {
            new object[] { "/Account/Profile" },
            new object[] { "/Account/MyBookings" },
        };

    [Theory]
    [InlineData("User")]
    [InlineData("Admin")]
    [InlineData("Owner")]
    public async Task AuthenticatedUser_CanLoadOwnAccountPages(string role)
    {
        var email = $"matrix-{role.ToLowerInvariant()}-{Guid.NewGuid():N}@test";
        await TestUsers.CreateAsync(_factory, email, role: role);
        var client = await TestUsers.SignedInClientAsync(_factory, email);

        foreach (var page in new[] { "/Account/Profile", "/Account/MyBookings" })
        {
            var response = await client.GetAsync(page);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
```

- [ ] **Step 2: Run the matrix tests**

```bash
dotnet test CoreX.UI.Tests/CoreX.UI.Tests.csproj --nologo --filter "FullyQualifiedName~AuthPolicyMatrixTests"
```

Expected: 5 anonymous cases pass + 3 role cases pass = 8 test runs total.

- [ ] **Step 3: Commit**

```bash
git add CoreX.UI.Tests/Pages/Account/AuthPolicyMatrixTests.cs
git commit -m "Add role-boundary integration matrix for /Account pages"
```

---

## Task 11 — End-to-end smoke + final cleanup

**Files:**
- (no new files — manual verification + final commit if drift)

- [ ] **Step 1: Run the full solution build**

```bash
dotnet build CoreX.sln --nologo
```

Expected: 0 errors (1 pre-existing CS8618 warning permitted).

- [ ] **Step 2: Run all tests**

```bash
dotnet test CoreX.sln --nologo --no-build
```

Expected: every test passes. Phase 0 (2) + Phase 1 (~22 across Login/Register/Logout/Profile/MyBookings/Matrix) = ~24 tests.

- [ ] **Step 3: Smoke-test in browser**

Start the app:

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet run --project CoreX/CoreX.UI.csproj --no-build --no-launch-profile --urls "http://localhost:5050"
```

Walk through:
1. Open `http://localhost:5050/` — UA hero renders, top-right shows **Увійти / Зареєструватися**.
2. Click **Зареєструватися** — fill form, submit. Expect redirect to `/`, top-right now shows greeting + **Профіль** + **Мої бронювання** + **Вийти**.
3. Click **Профіль** — see name + email.
4. Click **Мої бронювання** — see "Поки що бронювань немає."
5. Click **Вийти** — back to `/`, top-right shows Sign in / Register again.
6. Click **Увійти**, enter the same credentials — succeeds, layout reflects authenticated state.
7. Switch language to **English** — chrome flips to English; navigate `/Account/Login`, confirm form labels are English.

Stop the app (Ctrl+C).

- [ ] **Step 4: Verify `git status` is clean**

```bash
git status
```

Expected: no tracked changes; only the pre-existing untracked files (`.claude/`, `.github/`, `CLAUDE.md`, etc.).

- [ ] **Step 5: Phase 1 closing commit (only if drift)**

If any files changed during smoke-testing (none expected), commit them. Otherwise this step is a no-op.

---

## Phase 1 exit checklist

- [ ] `dotnet build CoreX.sln` returns 0 errors.
- [ ] `dotnet test CoreX.sln` shows all tests passing (Phase 0 + Phase 1).
- [ ] A new user can register at `/Account/Register` and is auto-logged in.
- [ ] An existing user can sign in at `/Account/Login`, see Profile + MyBookings, and sign out via the layout button.
- [ ] Invalid credentials show the localized "Невірна електронна адреса або пароль." (UA) / "Invalid email or password." (EN) message.
- [ ] Lockout after 5 failed attempts shows the localized "Акаунт тимчасово заблоковано..." message.
- [ ] Anonymous access to `/Account/Profile`, `/Account/MyBookings`, `/Account/Logout` redirects to `/Account/Login` with the original URL preserved as `ReturnUrl`.
- [ ] Language switcher continues to work (Phase 0 behaviour unaffected).

**Next phase:** `Phase 2 — Public discovery` (Home with city picker, `/Clubs`, club detail with HTMX tabs, `/Trainers/{id}`, Discounts, InformationMaterials). Builds the public browsing surface customers see before they ever consider buying.
