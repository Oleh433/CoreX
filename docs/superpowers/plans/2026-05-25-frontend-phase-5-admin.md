# Frontend Phase 5 — Admin Panel Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax.

**Goal:** Ship the admin panel under `/Admin/*` so an Admin (and an Owner) can run the whole CoreX business through a browser. Covers CRUD pages for Clubs, Trainers, GroupClasses, Vacancies, InformationMaterials; admin review queues for Bookings and VacancyApplications with HTMX row actions; Owner-only CRUD for Subscriptions and Discounts; and an Owner-only Admin-user registration form.

**Architecture:** Razor Pages under `CoreX/Pages/Admin/`. Folder-level authorization (`[Authorize(Policy="AdminOrOwner")]`) is already configured in `Program.cs` from Phase 0; three Owner-only pages have explicit `OwnerOnly` page-level conventions. A new `_AdminLayout.cshtml` provides the sidebar shell. All PageModels call backend services directly via DI. **No backend changes** — every service method needed is already in place.

**Tech Stack:** ASP.NET Core 8 Razor Pages · HTMX 2 for row actions · xUnit + `Microsoft.AspNetCore.Mvc.Testing`.

**Spec reference:** `docs/superpowers/specs/2026-05-20-frontend-design.md` — Phase 5 in §11, admin routes in §4.

---

## Scope simplifications

- **Global admin pages, not per-club tabs.** The spec hinted `/Admin/Clubs/{id}/Trainers`; the simpler shape is `/Admin/Trainers` with an optional `?clubId=...` filter dropdown. Same for GroupClasses and Vacancies. URL surface is flatter and tests are tighter.
- **Edit is optional for low-volume entities.** Trainers, GroupClasses, InformationMaterials — only List + Create + Delete (no Edit for v1). Edit can come back in Polish.
- **Dashboard is welcome-only.** No counts/recent-activity widget — just section cards linking into each admin area.
- **Discount activate/deactivate via Edit form.** The service has no `ActivateAsync` for discounts (only Update with `IsActive`); UI surfaces it as a checkbox on Edit.
- **No bulk actions / pagination / search.** Lists are flat. Polish phase if volume grows.

## Prerequisites

- Phase 4 merged. HEAD on master: `9b31927 Add /Vacancies/{id}/Apply form + Applied confirmation + TDD`.
- `dotnet build CoreX.sln --nologo` → 0 errors.
- `dotnet test CoreX.sln --nologo --no-build` → 54/54 passing.
- Razor Pages auth conventions are already set in `CoreX/Program.cs:85-95`:
  - `AuthorizeFolder("/Admin", "AdminOrOwner")` — every page under `/Admin/` requires an Admin or Owner role.
  - `AuthorizePage("/Admin/Subscriptions/Index", "OwnerOnly")` — Owner only.
  - `AuthorizePage("/Admin/Discounts/Index", "OwnerOnly")` — Owner only.
  - `AuthorizePage("/Admin/Users/RegisterAdmin", "OwnerOnly")` — Owner only.

## Backend surface (verified on master)

| Service | Methods used in admin |
|---|---|
| `IClubService` | `GetAllAsync`, `GetByIdAsync`, `CreateAsync(CreateClubDto)`, `UpdateAsync(id, UpdateClubDto)`, `DeleteAsync(id)` |
| `ITrainerService` | `GetAllAsync`, `GetByIdAsync`, `CreateAsync(CreateTrainerDto)`, `DeleteAsync(id)` |
| `IGroupClassService` | `GetByClubIdAsync(clubId)`, `CreateAsync(CreateGroupClassDto)`, `DeleteAsync(id)` |
| `IVacancyService` | `GetAllAsync`, `GetByIdAsync`, `CreateAsync(CreateVacancyDto)`, `UpdateAsync(id, UpdateVacancyDto)`, `ActivateAsync(id)`, `DeactivateAsync(id)`, `DeleteAsync(id)` |
| `ISubscriptionService` | `GetAllAsync`, `GetByIdAsync`, `CreateAsync(CreateSubscriptionDto)`, `UpdateAsync(id, UpdateSubscriptionDto)`, `ActivateAsync(id)`, `DeactivateAsync(id)`, `DeleteAsync(id)` |
| `IDiscountService` | `GetAllAsync`, `GetByIdAsync`, `CreateAsync(CreateDiscountDto)`, `UpdateAsync(id, UpdateDiscountDto)` (Update flips `IsActive`), `DeleteAsync(id)` |
| `IInformationMaterialService` | `GetAllAsync`, `CreateAsync(CreateInformationMaterialDto)`, `DeleteAsync(id)` |
| `IBookingService` | `GetAllAsync`, `ConfirmAsync(id)`, `CancelAsync(id, reason?)` |
| `IVacancyApplicationService` | `GetAllAsync`, `GetByVacancyIdAsync(vacancyId)`, `ChangeStatusAsync(id, ChangeVacancyApplicationStatusDto)` |
| `IUserService` | `AdminRegisterAsync(UserRegisterRequest)` |

**DTOs have no `[DataAnnotation]` attributes** (except `UserRegisterRequest` from Phase 1). Validation lives on the domain entity constructors as `ArgumentException` throws. Admin input models add Razor Pages `[Required]`/`[StringLength]`/`[Range]` etc. with hardcoded UA `ErrorMessage` strings for client-side hints; PageModels catch `ArgumentException` from the service to surface domain validation as ModelState errors.

## File map (high level)

**New files:**

- `CoreX/Pages/Admin/_AdminLayout.cshtml` — sidebar shell.
- `CoreX/Pages/Admin/Index.cshtml(.cs)` — dashboard / landing.
- `CoreX/Pages/Admin/Clubs/Index.cshtml(.cs)`, `Create.cshtml(.cs)`, `Edit.cshtml(.cs)` — full CRUD.
- `CoreX/Pages/Admin/Trainers/Index.cshtml(.cs)`, `Create.cshtml(.cs)` — list + create + HTMX delete.
- `CoreX/Pages/Admin/GroupClasses/Index.cshtml(.cs)`, `Create.cshtml(.cs)` — list + create + HTMX delete (requires `?clubId=` to pick the club).
- `CoreX/Pages/Admin/Vacancies/Index.cshtml(.cs)`, `Create.cshtml(.cs)`, `Edit.cshtml(.cs)` — full CRUD + HTMX activate/deactivate.
- `CoreX/Pages/Admin/InformationMaterials/Index.cshtml(.cs)`, `Create.cshtml(.cs)` — list + create + HTMX delete.
- `CoreX/Pages/Admin/Subscriptions/Index.cshtml(.cs)`, `Create.cshtml(.cs)`, `Edit.cshtml(.cs)` — Owner-only.
- `CoreX/Pages/Admin/Discounts/Index.cshtml(.cs)`, `Create.cshtml(.cs)`, `Edit.cshtml(.cs)` — Owner-only.
- `CoreX/Pages/Admin/Bookings/Index.cshtml(.cs)` — list + HTMX confirm/cancel.
- `CoreX/Pages/Admin/VacancyApplications/Index.cshtml(.cs)` — list + HTMX status change.
- `CoreX/Pages/Admin/Users/RegisterAdmin.cshtml(.cs)` — Owner-only form.
- `CoreX.UI.Tests/Pages/Admin/*` — tests per section + a single AccessMatrix.cs covering role boundaries.

**Modified files:**

- `CoreX.UI.Tests/TestSupport/TestUsers.cs` — add `SignedInAsAdminAsync` / `SignedInAsOwnerAsync` thin wrappers.
- `CoreX/Pages/Shared/_Layout.cshtml` — show "Адмін-панель" link in the authenticated user menu when the user is in `Admin` or `Owner` role.

---

## Task 1 — Admin test infrastructure

**Files:**
- Modify: `CoreX.UI.Tests/TestSupport/TestUsers.cs` — add role-typed convenience helpers.

The existing `SignedInClientAsync(factory, email, password)` takes credentials; admin tests want a one-liner that creates a fresh admin/owner and signs in.

- [ ] **Step 1: Append to `TestUsers.cs`**

```csharp
public static async Task<HttpClient> SignedInAsAdminAsync(CoreXFactory factory, string fullName = "Test Admin")
{
    var email = $"admin-{Guid.NewGuid():N}@test";
    await CreateAsync(factory, email, role: "Admin", fullName: fullName);
    return await SignedInClientAsync(factory, email);
}

public static async Task<HttpClient> SignedInAsOwnerAsync(CoreXFactory factory, string fullName = "Test Owner")
{
    var email = $"owner-{Guid.NewGuid():N}@test";
    await CreateAsync(factory, email, role: "Owner", fullName: fullName);
    return await SignedInClientAsync(factory, email);
}
```

- [ ] **Step 2: Build + run existing tests — no regression**

```bash
dotnet build CoreX.UI.Tests/CoreX.UI.Tests.csproj --nologo
dotnet test CoreX.UI.Tests/CoreX.UI.Tests.csproj --nologo --no-build
```

Expected: 54/54 still passing.

- [ ] **Step 3: Commit**

```bash
git add CoreX.UI.Tests/TestSupport/TestUsers.cs
git commit -m "Add SignedInAsAdminAsync / SignedInAsOwnerAsync test helpers"
```

---

## Task 2 — `_AdminLayout.cshtml` + `/Admin` dashboard (TDD)

**Files:**
- Create: `CoreX/Pages/Admin/_AdminLayout.cshtml`
- Create: `CoreX/Pages/Admin/_ViewStart.cshtml` (sets Layout to `_AdminLayout`)
- Create: `CoreX/Pages/Admin/Index.cshtml(.cs)`
- Modify: `CoreX/Pages/Shared/_Layout.cshtml` — add "Адмін-панель" link when authenticated user is Admin or Owner.
- Test: `CoreX.UI.Tests/Pages/Admin/AccessTests.cs`

The admin layout has a left sidebar with section links (Clubs, Trainers, GroupClasses, Vacancies, Bookings, Applications, Subscriptions, Discounts, InformationMaterials, RegisterAdmin) and a top bar with "← На сайт" back-to-public-site link + user-menu.

- [ ] **Step 1: Failing tests**

`CoreX.UI.Tests/Pages/Admin/AccessTests.cs`:

```csharp
using System.Net;
using CoreX.UI.Tests.TestSupport;
using Xunit;

namespace CoreX.UI.Tests.Pages.Admin;

public class AccessTests : IClassFixture<CoreXFactory>
{
    private readonly CoreXFactory _factory;
    public AccessTests(CoreXFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_AdminIndex_Anonymous_RedirectsToLogin()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
        var response = await client.GetAsync("/Admin");
        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.StartsWith("/Account/Login", response.Headers.Location?.AbsolutePath);
    }

    [Fact]
    public async Task Get_AdminIndex_UserRole_IsForbiddenOrRedirected()
    {
        var email = $"user-noadmin-{Guid.NewGuid():N}@test";
        await TestUsers.CreateAsync(_factory, email, role: "User");
        var client = await TestUsers.SignedInClientAsync(_factory, email);

        var response = await client.GetAsync("/Admin");

        // Identity default: 302 to /Account/AccessDenied (or 403 if configured).
        // Either way, NOT 200.
        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_AdminIndex_AdminRole_ReturnsOk_WithSidebar()
    {
        var client = await TestUsers.SignedInAsAdminAsync(_factory);
        var response = await client.GetAsync("/Admin");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Адмін-панель", body);
        Assert.Contains("Клуби", body);   // sidebar link
        Assert.Contains("Бронювання", body); // sidebar link
    }

    [Fact]
    public async Task Get_AdminIndex_OwnerRole_ReturnsOk()
    {
        var client = await TestUsers.SignedInAsOwnerAsync(_factory);
        var response = await client.GetAsync("/Admin");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
```

- [ ] **Step 2: Run — confirm 4 fail / fail-or-redirect-wrong**

- [ ] **Step 3: Create `_ViewStart.cshtml` in `CoreX/Pages/Admin/`**

```cshtml
@{
    Layout = "/Pages/Admin/_AdminLayout.cshtml";
}
```

- [ ] **Step 4: Create `_AdminLayout.cshtml`**

`CoreX/Pages/Admin/_AdminLayout.cshtml`:

```cshtml
@using Microsoft.AspNetCore.Antiforgery
@inject IAntiforgery Antiforgery
@{
    var tokens = Antiforgery.GetAndStoreTokens(Context);
    var isOwner = User.IsInRole("Owner");
}
<!DOCTYPE html>
<html lang="uk">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>@(ViewData["Title"]) · CoreX Адмін</title>
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@@400;500;600;700;800;900&display=swap" rel="stylesheet">
    <link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
</head>
<body class="bg-ink-50 min-h-screen" hx-headers='{"@tokens.HeaderName":"@tokens.RequestToken"}'>

    <header class="bg-ink-900 text-white">
        <div class="px-6 py-3 flex items-center justify-between">
            <div class="flex items-center gap-4">
                <a href="/" class="text-sm text-ink-200 hover:text-white">← На сайт</a>
                <span class="text-sm font-black tracking-wider uppercase">Адмін-панель</span>
            </div>
            <form method="post" action="/Account/Logout" class="inline">
                <input type="hidden" name="__RequestVerificationToken" value="@tokens.RequestToken" />
                <button type="submit" class="text-sm text-ink-200 hover:text-white">Вийти</button>
            </form>
        </div>
    </header>

    <div class="flex">
        <aside class="w-56 shrink-0 bg-white border-r border-ink-200 min-h-[calc(100vh-49px)]">
            <nav class="p-4 text-sm space-y-1">
                <a href="/Admin" class="block px-3 py-2 rounded-card hover:bg-ink-50 font-semibold">Огляд</a>
                <a href="/Admin/Clubs" class="block px-3 py-2 rounded-card hover:bg-ink-50">Клуби</a>
                <a href="/Admin/Trainers" class="block px-3 py-2 rounded-card hover:bg-ink-50">Тренери</a>
                <a href="/Admin/GroupClasses" class="block px-3 py-2 rounded-card hover:bg-ink-50">Заняття</a>
                <a href="/Admin/Vacancies" class="block px-3 py-2 rounded-card hover:bg-ink-50">Вакансії</a>
                <a href="/Admin/Bookings" class="block px-3 py-2 rounded-card hover:bg-ink-50">Бронювання</a>
                <a href="/Admin/VacancyApplications" class="block px-3 py-2 rounded-card hover:bg-ink-50">Заявки</a>
                <a href="/Admin/InformationMaterials" class="block px-3 py-2 rounded-card hover:bg-ink-50">Інформація</a>
                @if (isOwner)
                {
                    <hr class="my-3 border-ink-200" />
                    <p class="px-3 text-xs font-semibold uppercase tracking-wider text-ink-500">Власник</p>
                    <a href="/Admin/Subscriptions" class="block px-3 py-2 rounded-card hover:bg-ink-50">Абонементи</a>
                    <a href="/Admin/Discounts" class="block px-3 py-2 rounded-card hover:bg-ink-50">Акції</a>
                    <a href="/Admin/Users/RegisterAdmin" class="block px-3 py-2 rounded-card hover:bg-ink-50">Додати адміна</a>
                }
            </nav>
        </aside>
        <main class="flex-1 p-6">
            @RenderBody()
        </main>
    </div>

    <script src="~/js/htmx.min.js" asp-append-version="true"></script>
    <script src="~/js/site.js" asp-append-version="true"></script>
</body>
</html>
```

- [ ] **Step 5: Create `Index.cshtml.cs`**

```csharp
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Admin;

public class IndexModel : PageModel
{
    public void OnGet() { }
}
```

- [ ] **Step 6: Create `Index.cshtml`**

```cshtml
@page
@model CoreX.Pages.Admin.IndexModel
@{
    ViewData["Title"] = "Огляд";
}

<h1 class="text-2xl font-black uppercase tracking-tight text-ink-900">Огляд</h1>
<p class="mt-2 text-ink-500">Керуйте клубами, тренерами, заняттями, бронюваннями та заявками з лівої панелі.</p>
```

- [ ] **Step 7: Update public `_Layout.cshtml` to show "Адмін-панель" link for admins**

In the authenticated user-menu block, add (right before the Profile/MyBookings/Logout cluster):

```cshtml
@if (User.IsInRole("Admin") || User.IsInRole("Owner"))
{
    <a href="/Admin" class="btn-ghost text-sm">Адмін-панель</a>
}
```

- [ ] **Step 8: Run all tests — Phase 0-4 unaffected, 4 new AccessTests passing**

Expected: 58/58 passing (54 prior + 4 new).

- [ ] **Step 9: Commit**

```bash
git add CoreX/Pages/Admin/_AdminLayout.cshtml CoreX/Pages/Admin/_ViewStart.cshtml CoreX/Pages/Admin/Index.cshtml CoreX/Pages/Admin/Index.cshtml.cs CoreX/Pages/Shared/_Layout.cshtml CoreX.UI.Tests/Pages/Admin/AccessTests.cs
git commit -m "Add admin shell layout + /Admin dashboard + access tests"
```

---

## Task 3 — `/Admin/Clubs` CRUD (canonical pattern, TDD)

**Files:**
- Create: `CoreX/Pages/Admin/Clubs/Index.cshtml(.cs)`
- Create: `CoreX/Pages/Admin/Clubs/Create.cshtml(.cs)`
- Create: `CoreX/Pages/Admin/Clubs/Edit.cshtml(.cs)`
- Create: `CoreX/Pages/Admin/Clubs/Models/ClubInput.cs`
- Test: `CoreX.UI.Tests/Pages/Admin/Clubs/CrudTests.cs`

This task defines the canonical admin-CRUD pattern that the remaining 7 admin sections follow. Read it carefully and adapt the file/field lists for each follow-up task.

### Canonical pattern (used by Clubs, Vacancies, InformationMaterials, Subscriptions, Discounts CRUDs)

1. **Index page** — lists all entities in a table. Each row has Edit + Delete buttons.
   - Delete uses HTMX: `<form hx-post="?handler=Delete&id={id}" hx-confirm="Видалити?" hx-target="closest tr" hx-swap="outerHTML swap:0.3s">` — server removes the row from DB, returns empty content; the row vanishes.
2. **Create page** — full form. Top-level `@page`. POST handler calls `_service.CreateAsync(dto)`, catches `ArgumentException`, redirects to Index on success.
3. **Edit page** — `@page "/Admin/{Entity}/{id:guid}/Edit"`. GET loads entity → 404 if missing → pre-fills form. POST calls `UpdateAsync(id, dto)` with same error handling.
4. **Input model** — Razor Pages `[Required]` etc. with hardcoded UA `ErrorMessage`. Maps to the service DTO in the POST handler.

### Steps for Clubs CRUD

- [ ] **Step 1: Failing tests**

`CoreX.UI.Tests/Pages/Admin/Clubs/CrudTests.cs`:

```csharp
using System.Net;
using CoreX.Application.ServiceInterfaces;
using CoreX.UI.Tests.TestSupport;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CoreX.UI.Tests.Pages.Admin.Clubs;

public class CrudTests : IClassFixture<CoreXFactory>
{
    private readonly CoreXFactory _factory;
    public CrudTests(CoreXFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_AdminClubs_AsAdmin_ListsClubs()
    {
        await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var client = await TestUsers.SignedInAsAdminAsync(_factory);

        var response = await client.GetAsync("/Admin/Clubs");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Energy Kyiv", body);
        Assert.Contains("Forge Lviv", body);
    }

    [Fact]
    public async Task Post_AdminClubs_Create_CreatesClubAndRedirectsToIndex()
    {
        var client = await TestUsers.SignedInAsAdminAsync(_factory);
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, "/Admin/Clubs/Create");
        var post = AntiforgeryClient.BuildPost(
            "/Admin/Clubs/Create",
            new Dictionary<string, string>
            {
                ["Input.Name"] = "Spark Odesa",
                ["Input.City"] = "Одеса",
                ["Input.Address"] = "вул. Дерибасівська, 10",
            },
            token, afCookie);

        var response = await client.SendAsync(post);

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.StartsWith("/Admin/Clubs", response.Headers.Location?.AbsolutePath);

        // Confirm it persisted
        using var scope = _factory.Services.CreateScope();
        var clubs = scope.ServiceProvider.GetRequiredService<IClubService>();
        var all = await clubs.GetAllAsync();
        Assert.Contains(all, c => c.Name == "Spark Odesa");
    }

    [Fact]
    public async Task Post_AdminClubs_Create_WithBlankName_ReturnsForm_WithError()
    {
        var client = await TestUsers.SignedInAsAdminAsync(_factory);
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, "/Admin/Clubs/Create");
        var post = AntiforgeryClient.BuildPost(
            "/Admin/Clubs/Create",
            new Dictionary<string, string>
            {
                ["Input.Name"] = "",
                ["Input.City"] = "Одеса",
                ["Input.Address"] = "вул. Дерибасівська, 10",
            },
            token, afCookie);

        var response = await client.SendAsync(post);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Введіть назву клубу", body);
    }

    [Fact]
    public async Task Post_AdminClubs_Edit_UpdatesClub()
    {
        var clubs = await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var clubId = clubs[0].Id;

        var client = await TestUsers.SignedInAsAdminAsync(_factory);
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, $"/Admin/Clubs/{clubId}/Edit");
        var post = AntiforgeryClient.BuildPost(
            $"/Admin/Clubs/{clubId}/Edit",
            new Dictionary<string, string>
            {
                ["Input.Name"] = "Energy Kyiv (оновлений)",
                ["Input.City"] = "Київ",
                ["Input.Address"] = "вул. Хрещатик, 1",
            },
            token, afCookie);

        var response = await client.SendAsync(post);

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IClubService>();
        var updated = await service.GetByIdAsync(clubId);
        Assert.Equal("Energy Kyiv (оновлений)", updated!.Name);
    }

    [Fact]
    public async Task PostHx_AdminClubs_Delete_RemovesClub()
    {
        var seeded = await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var clubId = seeded[1].Id; // delete Forge Lviv

        var client = await TestUsers.SignedInAsAdminAsync(_factory);
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, "/Admin/Clubs");

        var req = new HttpRequestMessage(HttpMethod.Post, $"/Admin/Clubs?handler=Delete&id={clubId}");
        req.Headers.Add("HX-Request", "true");
        req.Headers.Add("Cookie", afCookie);
        req.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
        });

        var response = await client.SendAsync(req);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IClubService>();
        var stillThere = await service.GetByIdAsync(clubId);
        Assert.Null(stillThere);
    }
}
```

- [ ] **Step 2: Run — confirm 5 failing**

- [ ] **Step 3: `ClubInput.cs`**

`CoreX/Pages/Admin/Clubs/Models/ClubInput.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace CoreX.Pages.Admin.Clubs.Models;

public class ClubInput
{
    [Required(ErrorMessage = "Введіть назву клубу.")]
    [StringLength(150, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть місто.")]
    [StringLength(50)]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть адресу.")]
    [StringLength(250)]
    public string Address { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Phone(ErrorMessage = "Введіть коректний номер.")]
    [StringLength(30)]
    public string? Phone { get; set; }

    [EmailAddress(ErrorMessage = "Введіть коректний email.")]
    [StringLength(150)]
    public string? Email { get; set; }

    [StringLength(150)]
    public string? WorkingHours { get; set; }

    [Url(ErrorMessage = "Введіть коректне посилання.")]
    [StringLength(500)]
    public string? PhotoUrl { get; set; }

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
```

- [ ] **Step 4: `Index.cshtml.cs` + `Index.cshtml`**

`CoreX/Pages/Admin/Clubs/Index.cshtml.cs`:

```csharp
using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Admin.Clubs;

public class IndexModel : PageModel
{
    private readonly IClubService _clubs;
    public IndexModel(IClubService clubs) => _clubs = clubs;

    public IReadOnlyList<ClubResponseDto> Clubs { get; private set; } = Array.Empty<ClubResponseDto>();

    public async Task OnGetAsync() => Clubs = await _clubs.GetAllAsync();

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        await _clubs.DeleteAsync(id);
        return Content(string.Empty, "text/html"); // HTMX swaps the row to nothing
    }
}
```

`CoreX/Pages/Admin/Clubs/Index.cshtml`:

```cshtml
@page
@model CoreX.Pages.Admin.Clubs.IndexModel
@{
    ViewData["Title"] = "Клуби";
}

<div class="flex items-center justify-between">
    <h1 class="text-2xl font-black uppercase tracking-tight">Клуби</h1>
    <a asp-page="Create" class="btn-brand">+ Новий клуб</a>
</div>

@if (Model.Clubs.Count == 0)
{
    <p class="mt-6 text-ink-500">Поки немає клубів.</p>
}
else
{
    <table class="mt-6 w-full bg-white rounded-card border border-ink-200 text-sm">
        <thead class="bg-ink-50 text-left text-xs uppercase tracking-wider text-ink-500">
            <tr>
                <th class="px-4 py-2">Назва</th>
                <th class="px-4 py-2">Місто</th>
                <th class="px-4 py-2">Адреса</th>
                <th class="px-4 py-2 text-right">Дії</th>
            </tr>
        </thead>
        <tbody class="divide-y divide-ink-200">
        @foreach (var c in Model.Clubs)
        {
            <tr id="row-@c.Id">
                <td class="px-4 py-3 font-semibold">@c.Name</td>
                <td class="px-4 py-3">@c.City</td>
                <td class="px-4 py-3 text-ink-500">@c.Address</td>
                <td class="px-4 py-3 text-right space-x-2">
                    <a asp-page="Edit" asp-route-id="@c.Id" class="text-brand-500 hover:underline">Редагувати</a>
                    <button type="button"
                            hx-post="/Admin/Clubs?handler=Delete&id=@c.Id"
                            hx-confirm="Видалити клуб @c.Name?"
                            hx-target="#row-@c.Id"
                            hx-swap="outerHTML swap:0.3s"
                            class="text-danger hover:underline">Видалити</button>
                </td>
            </tr>
        }
        </tbody>
    </table>
}
```

- [ ] **Step 5: `Create.cshtml.cs` + `Create.cshtml`**

`CoreX/Pages/Admin/Clubs/Create.cshtml.cs`:

```csharp
using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using CoreX.Pages.Admin.Clubs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Admin.Clubs;

public class CreateModel : PageModel
{
    private readonly IClubService _clubs;
    public CreateModel(IClubService clubs) => _clubs = clubs;

    [BindProperty]
    public ClubInput Input { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        try
        {
            await _clubs.CreateAsync(new CreateClubDto
            {
                Name = Input.Name,
                City = Input.City,
                Address = Input.Address,
                Description = Input.Description,
                Phone = Input.Phone,
                Email = Input.Email,
                WorkingHours = Input.WorkingHours,
                PhotoUrl = Input.PhotoUrl,
                Latitude = Input.Latitude,
                Longitude = Input.Longitude,
            });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }

        return RedirectToPage("Index");
    }
}
```

`CoreX/Pages/Admin/Clubs/Create.cshtml`:

```cshtml
@page
@model CoreX.Pages.Admin.Clubs.CreateModel
@{
    ViewData["Title"] = "Новий клуб";
}

<h1 class="text-2xl font-black uppercase tracking-tight">Новий клуб</h1>

<form method="post" class="mt-6 max-w-2xl space-y-5" novalidate>
    <div asp-validation-summary="ModelOnly" class="rounded-card border border-danger bg-danger/5 text-danger px-4 py-3 text-sm"></div>

    @foreach (var (label, name, type, required) in new[]
    {
        ("Назва", "Name", "text", true),
        ("Місто", "City", "text", true),
        ("Адреса", "Address", "text", true),
        ("Телефон", "Phone", "tel", false),
        ("Email", "Email", "email", false),
        ("Години роботи", "WorkingHours", "text", false),
        ("Фото (URL)", "PhotoUrl", "url", false),
    })
    {
        <div>
            <label class="block text-xs font-semibold uppercase tracking-wide text-ink-800">@label</label>
            <input name="Input.@name" type="@type" @(required ? "required" : "")
                   value="@(typeof(CoreX.Pages.Admin.Clubs.Models.ClubInput).GetProperty(name)!.GetValue(Model.Input))"
                   class="mt-1 block w-full rounded-card border-ink-200 focus:border-brand-500 focus:ring-brand-500" />
            <span asp-validation-for="Input.@(name)" class="mt-1 block text-sm text-danger"></span>
        </div>
    }

    <div>
        <label class="block text-xs font-semibold uppercase tracking-wide text-ink-800">Опис</label>
        <textarea asp-for="Input.Description" rows="4" class="mt-1 block w-full rounded-card border-ink-200 focus:border-brand-500 focus:ring-brand-500"></textarea>
        <span asp-validation-for="Input.Description" class="mt-1 block text-sm text-danger"></span>
    </div>

    <div class="flex gap-3">
        <button type="submit" class="btn-brand">Створити</button>
        <a asp-page="Index" class="btn-ghost">Скасувати</a>
    </div>
</form>
```

*(The reflection-based `foreach` keeps the form short. If the implementer prefers, expand it into explicit inputs per field — both work; reflection is just less code.)*

- [ ] **Step 6: `Edit.cshtml.cs` + `Edit.cshtml`**

`CoreX/Pages/Admin/Clubs/Edit.cshtml.cs`:

```csharp
using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using CoreX.Pages.Admin.Clubs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Admin.Clubs;

public class EditModel : PageModel
{
    private readonly IClubService _clubs;
    public EditModel(IClubService clubs) => _clubs = clubs;

    [BindProperty]
    public ClubInput Input { get; set; } = new();

    public Guid Id { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        Id = id;
        var club = await _clubs.GetByIdAsync(id);
        if (club is null) return NotFound();

        Input = new ClubInput
        {
            Name = club.Name,
            City = club.City,
            Address = club.Address,
            Description = club.Description,
            Phone = club.Phone,
            Email = club.Email,
            WorkingHours = club.WorkingHours,
            PhotoUrl = club.PhotoUrl,
            Latitude = club.Latitude,
            Longitude = club.Longitude,
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        Id = id;
        if (!ModelState.IsValid) return Page();

        try
        {
            await _clubs.UpdateAsync(id, new UpdateClubDto
            {
                Name = Input.Name,
                City = Input.City,
                Address = Input.Address,
                Description = Input.Description,
                Phone = Input.Phone,
                Email = Input.Email,
                WorkingHours = Input.WorkingHours,
                PhotoUrl = Input.PhotoUrl,
                Latitude = Input.Latitude,
                Longitude = Input.Longitude,
            });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }

        return RedirectToPage("Index");
    }
}
```

`CoreX/Pages/Admin/Clubs/Edit.cshtml`:

```cshtml
@page "/Admin/Clubs/{id:guid}/Edit"
@model CoreX.Pages.Admin.Clubs.EditModel
@{
    ViewData["Title"] = "Редагування клубу";
}

<h1 class="text-2xl font-black uppercase tracking-tight">Редагування клубу</h1>

<form method="post" class="mt-6 max-w-2xl space-y-5" novalidate>
    <div asp-validation-summary="ModelOnly" class="rounded-card border border-danger bg-danger/5 text-danger px-4 py-3 text-sm"></div>

    <div><label class="block text-xs font-semibold uppercase tracking-wide text-ink-800">Назва</label>
        <input asp-for="Input.Name" required class="mt-1 block w-full rounded-card border-ink-200 focus:border-brand-500 focus:ring-brand-500" />
        <span asp-validation-for="Input.Name" class="mt-1 block text-sm text-danger"></span></div>
    <div><label class="block text-xs font-semibold uppercase tracking-wide text-ink-800">Місто</label>
        <input asp-for="Input.City" required class="mt-1 block w-full rounded-card border-ink-200" />
        <span asp-validation-for="Input.City" class="mt-1 block text-sm text-danger"></span></div>
    <div><label class="block text-xs font-semibold uppercase tracking-wide text-ink-800">Адреса</label>
        <input asp-for="Input.Address" required class="mt-1 block w-full rounded-card border-ink-200" />
        <span asp-validation-for="Input.Address" class="mt-1 block text-sm text-danger"></span></div>
    <div><label class="block text-xs font-semibold uppercase tracking-wide text-ink-800">Телефон</label>
        <input asp-for="Input.Phone" type="tel" class="mt-1 block w-full rounded-card border-ink-200" />
        <span asp-validation-for="Input.Phone" class="mt-1 block text-sm text-danger"></span></div>
    <div><label class="block text-xs font-semibold uppercase tracking-wide text-ink-800">Email</label>
        <input asp-for="Input.Email" type="email" class="mt-1 block w-full rounded-card border-ink-200" />
        <span asp-validation-for="Input.Email" class="mt-1 block text-sm text-danger"></span></div>
    <div><label class="block text-xs font-semibold uppercase tracking-wide text-ink-800">Години роботи</label>
        <input asp-for="Input.WorkingHours" class="mt-1 block w-full rounded-card border-ink-200" />
        <span asp-validation-for="Input.WorkingHours" class="mt-1 block text-sm text-danger"></span></div>
    <div><label class="block text-xs font-semibold uppercase tracking-wide text-ink-800">Фото (URL)</label>
        <input asp-for="Input.PhotoUrl" type="url" class="mt-1 block w-full rounded-card border-ink-200" />
        <span asp-validation-for="Input.PhotoUrl" class="mt-1 block text-sm text-danger"></span></div>
    <div><label class="block text-xs font-semibold uppercase tracking-wide text-ink-800">Опис</label>
        <textarea asp-for="Input.Description" rows="4" class="mt-1 block w-full rounded-card border-ink-200"></textarea>
        <span asp-validation-for="Input.Description" class="mt-1 block text-sm text-danger"></span></div>

    <div class="flex gap-3">
        <button type="submit" class="btn-brand">Зберегти</button>
        <a asp-page="Index" class="btn-ghost">Скасувати</a>
    </div>
</form>
```

- [ ] **Step 7: Run all Clubs tests — 5/5 passing**

- [ ] **Step 8: Commit**

```bash
git add CoreX/Pages/Admin/Clubs/ CoreX.UI.Tests/Pages/Admin/Clubs/
git commit -m "Add /Admin/Clubs CRUD with HTMX delete + canonical pattern"
```

Expected full suite: 63/63 (58 prior + 5 new).

---

## Tasks 4-12 — Apply the canonical pattern to remaining sections

Each of these tasks follows Task 3's structure. The implementer reads Task 3, then this task's specifics, and writes equivalent files. Each task has its own commit.

### Task 4 — `/Admin/Trainers` (List + Create + HTMX Delete) — no Edit

**Files:** `CoreX/Pages/Admin/Trainers/Index.cshtml(.cs)`, `Create.cshtml(.cs)`, `Models/TrainerInput.cs`, `CoreX.UI.Tests/Pages/Admin/Trainers/CrudTests.cs`.

**Specifics:**
- `ITrainerService.GetAllAsync` for the list.
- Index table columns: FullName, Specialization, ClubName, ExperienceYears, Actions (Delete only).
- Create form needs a `<select asp-for="Input.ClubId">` populated with clubs from `IClubService.GetAllAsync()` injected via DI alongside `ITrainerService`.
- `CreateTrainerDto` field list: `ClubId`, `FullName`, `Specialization`, `ExperienceYears`, `Bio?`, `Email?`, `Phone?`.
- `TrainerInput`: `Guid ClubId` (required), `string FullName` (required, 3-150), `string Specialization` (required, 2-100), `int ExperienceYears` (required, Range 0-60), `string? Bio` (max 2000), `string? Email` (EmailAddress), `string? Phone` (Phone).
- Tests: Index lists seeded trainers; Create with valid data persists; Create with blank FullName returns form with error; HTMX delete removes row.

### Task 5 — `/Admin/GroupClasses` (List + Create + HTMX Delete) — no Edit

**Files:** `CoreX/Pages/Admin/GroupClasses/Index.cshtml(.cs)`, `Create.cshtml(.cs)`, `Models/GroupClassInput.cs`, `CoreX.UI.Tests/Pages/Admin/GroupClasses/CrudTests.cs`.

**Specifics:**
- `IGroupClassService` has no `GetAllAsync` — only `GetByClubIdAsync`. Index must require `?clubId=...` (similar to public `/Memberships`). Render an "Оберіть клуб" instruction page if `ClubId` is null.
- Create form: club dropdown, Audience radio (Adults / Kids), Type text, StartTime datetime-local, DurationMinutes int, Capacity int, Trainer dropdown (optional, scoped to selected club — keep it as a global dropdown for simplicity in v1).
- `GroupClassInput`: `Guid ClubId` (Required), `Guid? TrainerId`, `string Type` (Required, 2-100), `string? Description`, `GroupClassAudience Audience` (Required), `DateTime StartTime` (Required), `int DurationMinutes` (Range 5-300), `int Capacity` (Range 1-200), `decimal? Price`.
- Tests: with `?clubId=...` lists club's classes; Create persists; missing `?clubId` shows instruction; HTMX delete removes row.

### Task 6 — `/Admin/Vacancies` (Full CRUD + HTMX Activate/Deactivate)

**Files:** `CoreX/Pages/Admin/Vacancies/Index.cshtml(.cs)`, `Create.cshtml(.cs)`, `Edit.cshtml(.cs)`, `Models/VacancyInput.cs`, `CoreX.UI.Tests/Pages/Admin/Vacancies/CrudTests.cs`.

**Specifics:**
- `IVacancyService.GetAllAsync` for the list (admin sees inactive too).
- Index columns: Title, ClubName, IsActive (status badge), Actions (Edit, Activate/Deactivate via HTMX, Delete).
- Activate/Deactivate handlers: `OnPostActivateAsync(Guid id)` / `OnPostDeactivateAsync(Guid id)`. Each returns the updated row partial (`Partial("_VacancyRow", updatedDto)`) — HTMX swaps the row.
- Create `VacancyInput`: `Guid ClubId` (Required), `string Title` (Required, 3-150), `string Description` (Required, min 10), `string Requirements` (Required, min 5), `decimal? Salary`, `DateTime? ApplicationDeadline`.
- Edit drops `ClubId` (use `UpdateVacancyDto`).
- Tests: list shows seeded vacancy; Create persists; Edit updates; Deactivate flips `IsActive`; HTMX delete removes row.

### Task 7 — `/Admin/InformationMaterials` (List + Create + HTMX Delete) — no Edit

**Files:** `CoreX/Pages/Admin/InformationMaterials/Index.cshtml(.cs)`, `Create.cshtml(.cs)`, `Models/MaterialInput.cs`, `CoreX.UI.Tests/Pages/Admin/InformationMaterials/CrudTests.cs`.

**Specifics:**
- `MaterialInput`: `string Title` (Required, 3-200), `string Body` (Required, min 10), `string? Category` (max 50).
- Index: Title, Category, CreatedAt, Delete.
- Tests: list shows seeded materials; Create persists; HTMX delete removes row.

### Task 8 — `/Admin/Subscriptions` (Owner-only; full CRUD + HTMX Activate/Deactivate)

**Files:** `CoreX/Pages/Admin/Subscriptions/Index.cshtml(.cs)`, `Create.cshtml(.cs)`, `Edit.cshtml(.cs)`, `Models/SubscriptionInput.cs`, `CoreX.UI.Tests/Pages/Admin/Subscriptions/CrudTests.cs`.

**Specifics:**
- Authorization: `/Admin/Subscriptions/Index` is already `OwnerOnly` per Phase 0 `Program.cs`. Create + Edit pages inherit the folder policy (`AdminOrOwner`); explicitly upgrade them to `OwnerOnly` by adding `options.Conventions.AuthorizePage("/Admin/Subscriptions/Create", "OwnerOnly");` and same for Edit — **modify `CoreX/Program.cs` to add those two AuthorizePage lines**.
- Tests must cover the role boundary: Admin → 403/302 to AccessDenied; Owner → 200.
- `SubscriptionInput`: `Guid ClubId`, `string Title` (3-150), `decimal Price` (>= 0), `int DurationDays` (Range 1-3650), `int? VisitsLimit` (Range 1-1000), `string? Description` (max 2000).
- Tests: anon → redirected; admin (non-owner) → not-200; owner → 200 + list/create/edit/activate/deactivate work.

### Task 9 — `/Admin/Discounts` (Owner-only; List + Create + Edit; IsActive toggle via Edit)

**Files:** `CoreX/Pages/Admin/Discounts/Index.cshtml(.cs)`, `Create.cshtml(.cs)`, `Edit.cshtml(.cs)`, `Models/DiscountInput.cs`, `CoreX.UI.Tests/Pages/Admin/Discounts/CrudTests.cs`.

**Specifics:**
- Authorization: `/Admin/Discounts/Index` already `OwnerOnly`. Add `AuthorizePage` for Create + Edit (same `Program.cs` change as Task 8).
- `IDiscountService` has no Activate/Deactivate — Edit form has an `IsActive` checkbox that maps to `UpdateDiscountDto.IsActive`.
- `DiscountInput`: `string Title` (3-150), `string? Description` (max 2000), `decimal? DiscountPercent` (Range 0-100), `string? Conditions` (max 1000), `string? PromoCode` (max 50), `DateTime StartDate`, `DateTime EndDate` (must be >= StartDate — catch `ArgumentException` from the entity), `bool IsActive` (only on Edit; Create defaults to true via entity ctor).
- Tests: list, create, edit; Admin role → 403/redirected on Index/Create/Edit; Owner → 200.

### Task 10 — `/Admin/Bookings` (List + HTMX Confirm/Cancel)

**Files:** `CoreX/Pages/Admin/Bookings/Index.cshtml(.cs)`, `_BookingRow.cshtml` (partial returned by HTMX handlers), `CoreX.UI.Tests/Pages/Admin/Bookings/IndexTests.cs`.

**Specifics:**
- `IBookingService.GetAllAsync` for the list.
- Columns: ContactFullName, ContactEmail, ContactPhone, Status (badge), CreatedAt, Actions.
- Status badge classes: `New` → orange tint, `Confirmed` → green, `Cancelled` → grey, `Completed` → blue.
- HTMX actions: `OnPostConfirmAsync(Guid id)` → calls `ConfirmAsync(id)`, returns the updated `_BookingRow` partial. `OnPostCancelAsync(Guid id)` → calls `CancelAsync(id, reason: null)`, returns the partial. Both handlers refetch the booking via `GetByIdAsync(id)` so the partial has current Status/CancelledAt.
- Tests: list shows seeded booking (create one through `IBookingService.CreateAsync` via DI scope — adapt the populated-MyBookings test pattern from Phase 3); Confirm HTMX returns 200 + partial with new status; Cancel HTMX returns 200 + partial with cancelled status.

### Task 11 — `/Admin/VacancyApplications` (List + HTMX status change)

**Files:** `CoreX/Pages/Admin/VacancyApplications/Index.cshtml(.cs)`, `_ApplicationRow.cshtml`, `CoreX.UI.Tests/Pages/Admin/VacancyApplications/IndexTests.cs`.

**Specifics:**
- `IVacancyApplicationService.GetAllAsync` for the list.
- Columns: FullName, Email, Phone, VacancyTitle, Status (badge), Actions (Reviewed / Accepted / Rejected buttons).
- HTMX handler: `OnPostStatusAsync(Guid id, string status)` → maps the string to `VacancyApplicationStatus` enum → calls `ChangeStatusAsync(id, new ChangeVacancyApplicationStatusDto { Status = ... })` → returns updated `_ApplicationRow` partial.
- Tests: list shows a created application (POST through the Phase 4 form first); status change reflects in the partial.

### Task 12 — `/Admin/Users/RegisterAdmin` (Owner-only form)

**Files:** `CoreX/Pages/Admin/Users/RegisterAdmin.cshtml(.cs)`, `CoreX.UI.Tests/Pages/Admin/Users/RegisterAdminTests.cs`.

**Specifics:**
- Already `OwnerOnly` via the existing `AuthorizePage` convention.
- Form mirrors the public Register page (Phase 1) — FullName, Email, Password, ConfirmPassword, TermsAccepted (checkbox, must be true).
- POST calls `IUserService.AdminRegisterAsync(new UserRegisterRequest { ... })`. On success, redirect to `/Admin` with a flash message or just redirect with TempData. On `InvalidOperationException` (duplicate email, etc.), `AddModelError` and re-render.
- Tests: Admin role → 403/redirected; Owner POST valid → 302 (verify the new user exists via `UserManager.FindByEmailAsync`); duplicate email → form with error.

---

## Task 13 — End-to-end smoke + final cleanup

- [ ] **Step 1: Build**

```bash
dotnet build CoreX.sln --nologo
```

Expected: 0 errors.

- [ ] **Step 2: Full test suite**

```bash
dotnet test CoreX.sln --nologo --no-build
```

Expected target: ~85-95 tests total (54 prior + ~30-40 new across Phase 5 tasks).

- [ ] **Step 3: Browser smoke**

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet run --project CoreX/CoreX.UI.csproj --no-build --no-launch-profile --urls "http://localhost:5057"
```

Walk through (sign in as the dev `Owner` from `appsettings.Development.json` — Identity bootstraps it):
1. Sign in → user menu shows "Адмін-панель" link → click it.
2. `/Admin` dashboard renders with sidebar (Owner sees the "Власник" section with Subscriptions/Discounts/RegisterAdmin links).
3. Visit `/Admin/Clubs` → list (empty or seeded) → Create a club → form submits → row appears.
4. Edit that club → save → name updated in list.
5. Delete a row via HTMX confirmation → row disappears.
6. `/Admin/Subscriptions` → Owner can access; create a subscription against the club.
7. `/Admin/Vacancies` → create a vacancy; toggle Activate/Deactivate via HTMX.
8. `/Admin/Bookings` → if any bookings exist, Confirm one via HTMX → status badge updates.
9. Sign out, sign in as a non-Admin user → confirm `/Admin` redirects away.

Stop with Ctrl+C.

- [ ] **Step 4: `git status` clean**

---

## Phase 5 exit checklist

- [ ] `dotnet build CoreX.sln` → 0 errors.
- [ ] All tests passing.
- [ ] Admin user can: list / create / edit / delete Clubs · list / create / delete Trainers · list / create / delete GroupClasses · list / create / edit / activate / deactivate / delete Vacancies · list / create / delete InformationMaterials · list bookings + confirm/cancel them via HTMX · list applications + change status via HTMX.
- [ ] Owner user can: everything an Admin can, plus list / create / edit / activate / deactivate / delete Subscriptions · list / create / edit / delete Discounts · register a new Admin user.
- [ ] Non-Admin users (anonymous, User role) cannot reach any `/Admin/*` page.
- [ ] Admin (non-Owner) users cannot reach `/Admin/Subscriptions`, `/Admin/Discounts`, `/Admin/Users/RegisterAdmin`.
- [ ] Layout's "Адмін-панель" link only appears for Admin/Owner.
- [ ] HTMX row actions (delete, activate, deactivate, confirm, cancel, status change) work without full page reloads.

**Next phase:** Phase 6 — Polish (404/500/403 pages, empty states, loading indicators, toasts, a11y / Lighthouse pass).
