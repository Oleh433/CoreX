# Frontend Phase 1 — Auth + Account Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Ship the user-facing auth flow (Login, Register, Logout) and authenticated Account pages (Profile, MyBookings) on top of the existing Identity cookie setup, plus a role-boundary integration test matrix.

**Architecture:** Add four Razor Pages under `CoreX/Pages/Account/` calling `IUserService` directly via DI (no HTTP self-call to the existing `UsersController`). **All UI strings are hardcoded UA in Razor markup** — the multi-layer localization scaffold from Phase 0 was simplified away in commit `<simplify>` after the `IStringLocalizer<SharedResource>` lookup was found to be broken (manifest baseName mismatch made it render raw keys like `"NavClubs"`). The `_Layout` already shows the authenticated user menu (Profile / MyBookings / Logout) when `User.Identity.IsAuthenticated`, so no additional Task is needed for layout chrome. Phase 1 ships UA-only; bilingual support is deferred to a polish phase.

**Tech Stack:** ASP.NET Core 8 Razor Pages · Identity cookie auth (existing) · xUnit + `Microsoft.AspNetCore.Mvc.Testing` · EF Core InMemory (test override from Phase 0).

**Spec reference:** `docs/superpowers/specs/2026-05-20-frontend-design.md` — Phase 1 in §11, auth specifics in §5.

---

## Prerequisites

- HEAD on the worktree branch carries the Phase 1 simplifications: `CoreXFactory` DB hoist, `SharedResource` layer removed, `_Layout` chrome hardcoded UA, user menu inlined.
- `dotnet build CoreX.sln --nologo` returns 0 errors.
- `dotnet test CoreX.UI.Tests/CoreX.UI.Tests.csproj --nologo --no-build` shows 2 passing tests from `CoreX.UI.Tests`.

## Backend surface used by this phase

| Surface | Where | Notes |
|---|---|---|
| `IUserService.UserRegisterAsync(UserRegisterRequest)` | `CoreX.Application/ServiceInterfaces/IUserService.cs` | Throws `InvalidOperationException("Passwords do not match.")`, `InvalidOperationException("You must accept the terms of use.")`, `InvalidOperationException("A user with this email already exists.")`, and `InvalidOperationException` for Identity errors. Sends a confirmation email on success. **Does not auto-sign-in.** |
| `IUserService.SignInAsync(UserSignInRequest)` | same | `SignInManager.PasswordSignInAsync(..., lockoutOnFailure: true)`. Throws `UnauthorizedAccessException("Account is temporarily locked due to multiple failed sign-in attempts.")`, `UnauthorizedAccessException("Account is not allowed to sign in.")`, `UnauthorizedAccessException("Invalid email or password.")`. |
| `IUserService.SignOutAsync()` | same | Calls `SignInManager.SignOutAsync()`. |
| `UserRegisterRequest` | `CoreX.Application/DTO/UserRegisterRequest.cs` | `string FullName`, `string Email`, `string Password`, `string ConfirmPassword`, `bool TermsAccepted`. |
| `UserSignInRequest` | `CoreX.Application/DTO/UserSignInRequest.cs` | `string Email`, `string Password`. |
| `ApplicationUser` | `CoreX.Domain/IdentityEntities/ApplicationUser.cs` | `IdentityUser<Guid>` + `string FullName`. |
| `UserManager<ApplicationUser>` | DI | Pages look up the current user via `GetUserAsync(User)`. |
| `IBookingService.GetByUserIdAsync(Guid)` | `CoreX.Application/ServiceInterfaces/IBookingService.cs` | Returns `List<BookingResponseDto>` (empty if none). DTO has IDs only — no club / subscription names. |
| `IClubService.GetByIdAsync(Guid) → ClubResponseDto?` | `CoreX.Application/ServiceInterfaces/IClubService.cs` | `ClubResponseDto.Name`. |
| `ISubscriptionService.GetByIdAsync(Guid) → SubscriptionResponseDto?` | `CoreX.Application/ServiceInterfaces/ISubscriptionService.cs` | `SubscriptionResponseDto.Title` (note: `Title`, not `Name`). |
| Razor Pages auth conventions | `CoreX/Program.cs` | `AuthorizeFolder("/Account", "AuthenticatedOnly")`, `AllowAnonymousToPage("/Account/Login")`, `AllowAnonymousToPage("/Account/Register")`. Already in place from Phase 0. **No `Program.cs` changes in Phase 1.** |

## File map (post-simplification)

**New files:**

| File | Responsibility |
|---|---|
| `CoreX/Pages/Account/Models/LoginInput.cs` | Razor Pages input model for `/Account/Login`. |
| `CoreX/Pages/Account/Models/RegisterInput.cs` | Razor Pages input model for `/Account/Register`. |
| `CoreX/Pages/Account/Login.cshtml` | Login page markup (UA strings inline). |
| `CoreX/Pages/Account/Login.cshtml.cs` | `LoginModel`: GET renders form, POST calls `IUserService.SignInAsync`, maps `UnauthorizedAccessException` to UA form error, redirects on success. |
| `CoreX/Pages/Account/Register.cshtml` | Register page markup (UA strings inline). |
| `CoreX/Pages/Account/Register.cshtml.cs` | `RegisterModel`: GET renders form, POST calls `IUserService.UserRegisterAsync` then `SignInAsync` (auto-login), maps `InvalidOperationException` to UA form error. |
| `CoreX/Pages/Account/Logout.cshtml` | View-less page. |
| `CoreX/Pages/Account/Logout.cshtml.cs` | `LogoutModel`: POST calls `IUserService.SignOutAsync` and redirects to `/`. GET redirects to `/Account/Login` (folder auth would redirect anyway). |
| `CoreX/Pages/Account/Profile.cshtml` | Read-only profile display. |
| `CoreX/Pages/Account/Profile.cshtml.cs` | `ProfileModel`: GET loads `ApplicationUser` via `UserManager.GetUserAsync(User)`. |
| `CoreX/Pages/Account/MyBookings.cshtml` | List of the current user's bookings. |
| `CoreX/Pages/Account/MyBookings.cshtml.cs` | `MyBookingsModel`: GET fetches `IBookingService.GetByUserIdAsync(userId)` and joins club / subscription names through `IClubService` / `ISubscriptionService`. |
| `CoreX.UI.Tests/TestSupport/AntiforgeryClient.cs` | (Already landed in Task 3 commit `2123fb8`.) |
| `CoreX.UI.Tests/TestSupport/TestUsers.cs` | (Already landed in Task 3 commit `2123fb8`.) |
| `CoreX.UI.Tests/Pages/Account/LoginTests.cs` | TDD tests for Login. |
| `CoreX.UI.Tests/Pages/Account/RegisterTests.cs` | TDD tests for Register. |
| `CoreX.UI.Tests/Pages/Account/LogoutTests.cs` | TDD tests for Logout. |
| `CoreX.UI.Tests/Pages/Account/ProfileTests.cs` | TDD tests for Profile. |
| `CoreX.UI.Tests/Pages/Account/MyBookingsTests.cs` | TDD tests for MyBookings (empty state). |
| `CoreX.UI.Tests/Pages/Account/AuthPolicyMatrixTests.cs` | Role × page matrix (anon redirected to login; any role can load own Account pages). |

**Not created (simplification):**

- No `Resources/Pages/Account/*.{uk,en}.resx` — auth strings live in the Razor markup.
- No `ValidationMessages.*` class or resource.
- No additional `Program.cs` registrations.

**Out of scope for Phase 1:**

- Forgot-password / email-confirmation public pages.
- 2FA, external logins, account deletion.
- Editable profile.
- `IBookingService.CreateAsync(Guid?, …)` signature change (Phase 3).
- `/Account/MyBookings` row actions (cancel / rebook).
- TrainingPlan-related pages (out of frontend scope entirely).
- EN localization of auth pages (deferred to polish).

---

## Task 1 — Drop TrainingPlan nav link  ✅ landed

Already implemented in commit `505df07`. Removed the `/TrainingPlan` link from `_Layout.cshtml` and the `NavTrainingPlan` key from `SharedResource.{uk,en}.resx` (those resx files have since been deleted by the simplification commit).

## Task 2 — Shared chrome + auth strings  ✅ superseded by simplification

Originally added a 9-key auth-chrome block plus 6 validation keys to `SharedResource.{uk,en}.resx`, plus `AddDataAnnotationsLocalization` wiring in Program.cs. **All of that was removed in the simplification commit.** Chrome strings are now hardcoded UA in `_Layout.cshtml`; validation messages on input models will be hardcoded UA `ErrorMessage` strings.

## Task 3 — Test infrastructure (AntiforgeryClient + TestUsers)  ✅ landed

Already implemented in commit `2123fb8`. The two helpers are reused by every Phase 1 test task below. The InMemoryDb sharing fix from commit `a1dd016` makes `TestUsers.CreateAsync` work across scopes.

---

## Task 4 — Login page (TDD)

**Files:**
- Create: `CoreX/Pages/Account/Models/LoginInput.cs`
- Create: `CoreX/Pages/Account/Login.cshtml`
- Create: `CoreX/Pages/Account/Login.cshtml.cs`
- Test: `CoreX.UI.Tests/Pages/Account/LoginTests.cs`

- [ ] **Step 1: Write the failing tests**

`CoreX.UI.Tests/Pages/Account/LoginTests.cs`:

```csharp
using System.Net;
using CoreX.UI.Tests.TestSupport;
using Xunit;

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
        Assert.Contains("Увійти", body);
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
    public async Task Post_Login_WithInvalidPassword_ReturnsForm_WithError()
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
        Assert.False(response.Headers.TryGetValues("Set-Cookie", out var setCookies)
            && setCookies.Any(c => c.StartsWith(".AspNetCore.Identity.Application", StringComparison.Ordinal)),
            "No auth cookie should be set on failed sign-in.");
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
}
```

- [ ] **Step 2: Run the tests — confirm they fail**

```bash
dotnet test CoreX.UI.Tests/CoreX.UI.Tests.csproj --nologo --filter "FullyQualifiedName~LoginTests"
```

Expected: all 4 fail (404 on GETs, "No antiforgery token found" on POSTs).

- [ ] **Step 3: Create the input model**

`CoreX/Pages/Account/Models/LoginInput.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace CoreX.Pages.Account.Models;

public class LoginInput
{
    [Required(ErrorMessage = "Введіть електронну пошту.")]
    [EmailAddress(ErrorMessage = "Введіть коректну електронну адресу.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть пароль.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
```

- [ ] **Step 4: Create the PageModel**

`CoreX/Pages/Account/Login.cshtml.cs`:

```csharp
using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using CoreX.Pages.Account.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Account;

public class LoginModel : PageModel
{
    private readonly IUserService _users;

    public LoginModel(IUserService users) => _users = users;

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

    private static string MapSignInError(string serviceMessage) => serviceMessage switch
    {
        "Account is temporarily locked due to multiple failed sign-in attempts."
            => "Акаунт тимчасово заблоковано. Спробуйте за 15 хвилин.",
        "Account is not allowed to sign in."
            => "Невірна електронна адреса або пароль.",
        _ => "Невірна електронна адреса або пароль.",
    };
}
```

- [ ] **Step 5: Create the view**

`CoreX/Pages/Account/Login.cshtml`:

```cshtml
@page
@model CoreX.Pages.Account.LoginModel
@{
    ViewData["Title"] = "Увійти";
}

<section class="max-w-md mx-auto px-4 py-12 md:py-16">
    <h1 class="text-3xl font-black uppercase tracking-tight">Увійти</h1>
    <p class="mt-3 text-ink-500">Раді знову бачити.</p>

    <form method="post" class="mt-8 space-y-5"
          asp-route-returnUrl="@Model.ReturnUrl"
          novalidate>
        <div asp-validation-summary="ModelOnly" class="rounded-card border border-danger bg-danger/5 text-danger px-4 py-3 text-sm"></div>

        <div>
            <label asp-for="Input.Email" class="block text-xs font-semibold uppercase tracking-wide text-ink-800">
                Електронна пошта
            </label>
            <input asp-for="Input.Email" autocomplete="email" required
                   class="mt-1 block w-full rounded-card border-ink-200 focus:border-brand-500 focus:ring-brand-500" />
            <span asp-validation-for="Input.Email" class="mt-1 block text-sm text-danger"></span>
        </div>

        <div>
            <label asp-for="Input.Password" class="block text-xs font-semibold uppercase tracking-wide text-ink-800">
                Пароль
            </label>
            <input asp-for="Input.Password" type="password" autocomplete="current-password" required
                   class="mt-1 block w-full rounded-card border-ink-200 focus:border-brand-500 focus:ring-brand-500" />
            <span asp-validation-for="Input.Password" class="mt-1 block text-sm text-danger"></span>
        </div>

        <button type="submit" class="btn-brand w-full">Увійти</button>
    </form>

    <p class="mt-6 text-sm text-ink-500 text-center">
        Ще немає акаунту?
        <a asp-page="/Account/Register" class="font-semibold text-brand-500 hover:underline">Зареєструватися</a>
    </p>
</section>
```

- [ ] **Step 6: Run the tests — confirm they pass**

```bash
dotnet test CoreX.UI.Tests/CoreX.UI.Tests.csproj --nologo --filter "FullyQualifiedName~LoginTests"
```

Expected: 4 passing, 0 failing.

- [ ] **Step 7: Commit**

```bash
git add CoreX/Pages/Account/Models/LoginInput.cs CoreX/Pages/Account/Login.cshtml CoreX/Pages/Account/Login.cshtml.cs CoreX.UI.Tests/Pages/Account/LoginTests.cs
git commit -m "Add /Account/Login (UA, hardcoded) with TDD"
```

---

## Task 5 — Register page (TDD)

**Files:**
- Create: `CoreX/Pages/Account/Models/RegisterInput.cs`
- Create: `CoreX/Pages/Account/Register.cshtml`
- Create: `CoreX/Pages/Account/Register.cshtml.cs`
- Test: `CoreX.UI.Tests/Pages/Account/RegisterTests.cs`

- [ ] **Step 1: Write the failing tests**

`CoreX.UI.Tests/Pages/Account/RegisterTests.cs`:

```csharp
using System.Net;
using CoreX.UI.Tests.TestSupport;
using Xunit;

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
    public async Task Post_Register_WithMismatchedPasswords_ReturnsForm_WithError()
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
    public async Task Post_Register_WithTermsNotAccepted_ReturnsForm_WithError()
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
    public async Task Post_Register_WithDuplicateEmail_ReturnsForm_WithError()
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

Expected: 5 failing.

- [ ] **Step 3: Create the input model**

`CoreX/Pages/Account/Models/RegisterInput.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace CoreX.Pages.Account.Models;

public class RegisterInput
{
    [Required(ErrorMessage = "Введіть повне ім'я.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Ім'я має містити від 3 до 100 символів.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть електронну пошту.")]
    [EmailAddress(ErrorMessage = "Введіть коректну електронну адресу.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть пароль.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Пароль має містити щонайменше 8 символів.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Підтвердіть пароль.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Паролі не співпадають.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Range(typeof(bool), "true", "true", ErrorMessage = "Потрібно прийняти умови використання.")]
    public bool TermsAccepted { get; set; }
}
```

- [ ] **Step 4: Create the PageModel**

`CoreX/Pages/Account/Register.cshtml.cs`:

```csharp
using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using CoreX.Pages.Account.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly IUserService _users;

    public RegisterModel(IUserService users) => _users = users;

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
            return RedirectToPage("/Account/Login");
        }

        return LocalRedirect("/");
    }

    private static string MapRegisterError(string serviceMessage) => serviceMessage switch
    {
        "A user with this email already exists." => "Користувач з такою електронною адресою вже існує.",
        "Passwords do not match." => "Паролі не співпадають.",
        "You must accept the terms of use." => "Потрібно прийняти умови використання.",
        _ => "Не вдалося створити акаунт. Спробуйте ще раз.",
    };
}
```

- [ ] **Step 5: Create the view**

`CoreX/Pages/Account/Register.cshtml`:

```cshtml
@page
@model CoreX.Pages.Account.RegisterModel
@{
    ViewData["Title"] = "Реєстрація";
}

<section class="max-w-md mx-auto px-4 py-12 md:py-16">
    <h1 class="text-3xl font-black uppercase tracking-tight">Реєстрація</h1>
    <p class="mt-3 text-ink-500">Перетни свою межу разом з CoreX.</p>

    <form method="post" class="mt-8 space-y-5" novalidate>
        <div asp-validation-summary="ModelOnly" class="rounded-card border border-danger bg-danger/5 text-danger px-4 py-3 text-sm"></div>

        <div>
            <label asp-for="Input.FullName" class="block text-xs font-semibold uppercase tracking-wide text-ink-800">
                Повне ім'я
            </label>
            <input asp-for="Input.FullName" autocomplete="name" required
                   class="mt-1 block w-full rounded-card border-ink-200 focus:border-brand-500 focus:ring-brand-500" />
            <span asp-validation-for="Input.FullName" class="mt-1 block text-sm text-danger"></span>
        </div>

        <div>
            <label asp-for="Input.Email" class="block text-xs font-semibold uppercase tracking-wide text-ink-800">
                Електронна пошта
            </label>
            <input asp-for="Input.Email" autocomplete="email" required
                   class="mt-1 block w-full rounded-card border-ink-200 focus:border-brand-500 focus:ring-brand-500" />
            <span asp-validation-for="Input.Email" class="mt-1 block text-sm text-danger"></span>
        </div>

        <div>
            <label asp-for="Input.Password" class="block text-xs font-semibold uppercase tracking-wide text-ink-800">
                Пароль
            </label>
            <input asp-for="Input.Password" type="password" autocomplete="new-password" required
                   class="mt-1 block w-full rounded-card border-ink-200 focus:border-brand-500 focus:ring-brand-500" />
            <span asp-validation-for="Input.Password" class="mt-1 block text-sm text-danger"></span>
        </div>

        <div>
            <label asp-for="Input.ConfirmPassword" class="block text-xs font-semibold uppercase tracking-wide text-ink-800">
                Підтвердження пароля
            </label>
            <input asp-for="Input.ConfirmPassword" type="password" autocomplete="new-password" required
                   class="mt-1 block w-full rounded-card border-ink-200 focus:border-brand-500 focus:ring-brand-500" />
            <span asp-validation-for="Input.ConfirmPassword" class="mt-1 block text-sm text-danger"></span>
        </div>

        <label class="flex items-start gap-3 text-sm text-ink-800">
            <input asp-for="Input.TermsAccepted" type="checkbox" class="mt-1 rounded border-ink-200 text-brand-500 focus:ring-brand-500" />
            <span>Я приймаю умови використання та політику конфіденційності.</span>
        </label>
        <span asp-validation-for="Input.TermsAccepted" class="block text-sm text-danger"></span>

        <button type="submit" class="btn-brand w-full">Зареєструватися</button>
    </form>

    <p class="mt-6 text-sm text-ink-500 text-center">
        Вже маєте акаунт?
        <a asp-page="/Account/Login" class="font-semibold text-brand-500 hover:underline">Увійти</a>
    </p>
</section>
```

- [ ] **Step 6: Run the tests — confirm they pass**

Expected: 5 passing.

- [ ] **Step 7: Commit**

```bash
git add CoreX/Pages/Account/Models/RegisterInput.cs CoreX/Pages/Account/Register.cshtml CoreX/Pages/Account/Register.cshtml.cs CoreX.UI.Tests/Pages/Account/RegisterTests.cs
git commit -m "Add /Account/Register with auto-login on success + TDD"
```

---

## Task 6 — Logout page (TDD)

**Files:**
- Create: `CoreX/Pages/Account/Logout.cshtml`
- Create: `CoreX/Pages/Account/Logout.cshtml.cs`
- Test: `CoreX.UI.Tests/Pages/Account/LogoutTests.cs`

Triggered by the layout's user-menu form (already inlined into `_Layout.cshtml`). The `.cshtml` exists only so Razor Pages routes the URL; no body is rendered.

- [ ] **Step 1: Write the failing tests**

`CoreX.UI.Tests/Pages/Account/LogoutTests.cs`:

```csharp
using System.Net;
using CoreX.UI.Tests.TestSupport;
using Xunit;

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

        // Reuse the antiforgery token / cookie from any authenticated page; Profile is convenient.
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, "/Account/Profile");
        var post = AntiforgeryClient.BuildPost(
            "/Account/Logout",
            Array.Empty<KeyValuePair<string, string>>(),
            token,
            afCookie);
        var response = await client.SendAsync(post);

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Equal("/", response.Headers.Location?.OriginalString);
        Assert.Contains(response.Headers.GetValues("Set-Cookie"),
            c => c.StartsWith(".AspNetCore.Identity.Application=;", StringComparison.Ordinal));
    }
}
```

- [ ] **Step 2: Run — confirm failure**

Both fail (no page yet).

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

- [ ] **Step 5: Run — confirm pass**

Both pass.

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
- Test: `CoreX.UI.Tests/Pages/Account/ProfileTests.cs`

- [ ] **Step 1: Write the failing tests**

`CoreX.UI.Tests/Pages/Account/ProfileTests.cs`:

```csharp
using System.Net;
using CoreX.UI.Tests.TestSupport;
using Xunit;

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
    ViewData["Title"] = "Профіль";
}

<section class="max-w-2xl mx-auto px-4 py-12 md:py-16">
    <p class="text-xs font-semibold tracking-[0.2em] uppercase text-brand-500">Ваш акаунт</p>
    <h1 class="mt-2 text-3xl md:text-4xl font-black uppercase tracking-tight">@Model.FullName</h1>

    <dl class="mt-8 grid grid-cols-1 md:grid-cols-2 gap-6 text-sm">
        <div>
            <dt class="text-ink-500 uppercase tracking-wide text-xs font-semibold">Електронна пошта</dt>
            <dd class="mt-1 text-ink-900 break-all">@Model.Email</dd>
        </div>
    </dl>

    <div class="mt-10 flex gap-3">
        <a asp-page="/Account/MyBookings" class="btn-brand">Мої бронювання</a>
    </div>
</section>
```

- [ ] **Step 5: Run — confirm pass**

- [ ] **Step 6: Commit**

```bash
git add CoreX/Pages/Account/Profile.cshtml CoreX/Pages/Account/Profile.cshtml.cs CoreX.UI.Tests/Pages/Account/ProfileTests.cs
git commit -m "Add /Account/Profile (read-only) + TDD"
```

---

## Task 8 — MyBookings page (TDD)

**Files:**
- Create: `CoreX/Pages/Account/MyBookings.cshtml`
- Create: `CoreX/Pages/Account/MyBookings.cshtml.cs`
- Test: `CoreX.UI.Tests/Pages/Account/MyBookingsTests.cs`

The page joins each `BookingResponseDto` with `IClubService.GetByIdAsync` (returns `ClubResponseDto.Name`) and `ISubscriptionService.GetByIdAsync` (returns `SubscriptionResponseDto.Title`) so the user sees readable club + subscription names.

- [ ] **Step 1: Write the failing tests**

`CoreX.UI.Tests/Pages/Account/MyBookingsTests.cs`:

```csharp
using System.Net;
using CoreX.UI.Tests.TestSupport;
using Xunit;

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

(A "with bookings" test requires standing up `Club` + `Subscription` + `Booking` entities in the in-memory DB. Add it in Phase 3 when the booking creation flow is built.)

- [ ] **Step 2: Run — confirm failure**

- [ ] **Step 3: Create the PageModel**

`CoreX/Pages/Account/MyBookings.cshtml.cs`:

```csharp
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

- [ ] **Step 4: Create the view**

`CoreX/Pages/Account/MyBookings.cshtml`:

```cshtml
@page
@model CoreX.Pages.Account.MyBookingsModel
@{
    ViewData["Title"] = "Мої бронювання";
}

<section class="max-w-4xl mx-auto px-4 py-12 md:py-16">
    <h1 class="text-3xl md:text-4xl font-black uppercase tracking-tight">Мої бронювання</h1>

    @if (Model.Rows.Count == 0)
    {
        <p class="mt-8 text-ink-500">Поки що бронювань немає.</p>
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

- [ ] **Step 5: Run — confirm pass**

- [ ] **Step 6: Commit**

```bash
git add CoreX/Pages/Account/MyBookings.cshtml CoreX/Pages/Account/MyBookings.cshtml.cs CoreX.UI.Tests/Pages/Account/MyBookingsTests.cs
git commit -m "Add /Account/MyBookings (list + empty state) + TDD"
```

---

## Task 9 — Role-boundary integration matrix

**Files:**
- Create: `CoreX.UI.Tests/Pages/Account/AuthPolicyMatrixTests.cs`

(Renumbered: was Task 10 originally; the original Task 9 "Layout user menu" is already folded into the simplification commit.)

- [ ] **Step 1: Create the matrix test**

`CoreX.UI.Tests/Pages/Account/AuthPolicyMatrixTests.cs`:

```csharp
using System.Net;
using CoreX.UI.Tests.TestSupport;
using Xunit;

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

Expected: 5 anonymous cases + 3 role cases = 8 test runs, all passing.

- [ ] **Step 3: Commit**

```bash
git add CoreX.UI.Tests/Pages/Account/AuthPolicyMatrixTests.cs
git commit -m "Add role-boundary integration matrix for /Account pages"
```

---

## Task 10 — End-to-end smoke + final cleanup

**Files:**
- (no new files — manual verification + final commit if drift)

- [ ] **Step 1: Run the full solution build**

```bash
dotnet build CoreX.sln --nologo
```

Expected: 0 errors.

- [ ] **Step 2: Run all tests**

```bash
dotnet test CoreX.sln --nologo --no-build
```

Expected: Phase 0 (2) + Phase 1 (~20 across Login/Register/Logout/Profile/MyBookings/Matrix) all pass.

- [ ] **Step 3: Smoke-test in browser**

Start the app:

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet run --project CoreX/CoreX.UI.csproj --no-build --no-launch-profile --urls "http://localhost:5050"
```

Walk through:
1. `http://localhost:5050/` — UA hero renders, top-right shows **Увійти / Реєстрація**.
2. Click **Реєстрація** — fill form, submit. Expect redirect to `/`, top-right now shows **Профіль / Мої бронювання / Вийти**.
3. Click **Профіль** — see full name + email.
4. Click **Мої бронювання** — see "Поки що бронювань немає."
5. Click **Вийти** — back to `/`, top-right shows Увійти / Реєстрація.
6. Click **Увійти**, enter the same credentials — succeeds.
7. Try a wrong password — see "Невірна електронна адреса або пароль." inline.

Stop the app (Ctrl+C).

- [ ] **Step 4: Verify `git status` is clean**

Expected: no tracked changes; only the pre-existing untracked files.

- [ ] **Step 5: Phase 1 closing commit (only if drift)**

No-op unless smoke-testing touched any tracked file.

---

## Phase 1 exit checklist

- [ ] `dotnet build CoreX.sln` returns 0 errors.
- [ ] `dotnet test CoreX.sln` shows all tests passing (Phase 0 + Phase 1).
- [ ] A new user can register at `/Account/Register` and is auto-logged in.
- [ ] An existing user can sign in at `/Account/Login`, see Profile + MyBookings, and sign out via the layout button.
- [ ] Invalid credentials show the UA "Невірна електронна адреса або пароль." message.
- [ ] Lockout after 5 failed attempts shows "Акаунт тимчасово заблоковано. Спробуйте за 15 хвилин." (existing Identity lockout policy, no Phase 1 wiring needed).
- [ ] Anonymous access to `/Account/Profile`, `/Account/MyBookings`, `/Account/Logout` redirects to `/Account/Login` with the original URL preserved as `ReturnUrl`.

**Next phase:** `Phase 2 — Public discovery` (Home with city picker, `/Clubs`, club detail with HTMX tabs, `/Trainers/{id}`, Discounts, InformationMaterials).
