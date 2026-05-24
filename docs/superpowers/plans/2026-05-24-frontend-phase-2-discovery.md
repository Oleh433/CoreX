# Frontend Phase 2 — Public Discovery Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax.

**Goal:** Ship the anonymous public-browsing surface: list clubs (with city filter), open a club detail page with HTMX-swapped tabs for Trainers / GroupClasses / Vacancies, dedicated `/Trainers/{id}` / `/Discounts` / `/InformationMaterials` pages, and a home page that shows a few featured clubs.

**Architecture:** Razor Pages under `CoreX/Pages/Clubs/`, `CoreX/Pages/Trainers/`, `CoreX/Pages/Discounts/`, `CoreX/Pages/InformationMaterials/`. PageModels call backend services directly via DI (no HTTP self-call). Club detail uses HTMX 2 to swap tab content into a `#tab-content` target — handlers detect `HX-Request: true` and return partials. All UI strings hardcoded UA, matching the Phase 1 pattern; no per-page resx, no `IStringLocalizer`.

**Tech Stack:** ASP.NET Core 8 Razor Pages · HTMX 2 (already vendored from Phase 0) · xUnit + `Microsoft.AspNetCore.Mvc.Testing` · EF Core InMemory (test override).

**Spec reference:** `docs/superpowers/specs/2026-05-20-frontend-design.md` — Phase 2 in §11, routing in §4, HTMX patterns in §6.

---

## Scope cuts vs. the original spec

- **Memberships tab on `/Clubs/{id}` is deferred to Phase 3.** Booking only makes sense once the subscription catalog + booking form exist; bundling the tab with the booking flow keeps the change set focused.
- **Home city picker UI is deferred to Phase 6 polish.** The home page shows a static "featured clubs" grid (first 6 clubs by name). `/Clubs?city=...` already accepts a free-form query parameter for direct linking.
- **`InformationMaterial.Locale` column is dropped from the plan.** Phase 2 is UA-only; rebuilding the schema for bilingual content can wait.
- **`/InformationMaterials/{id}` detail page is not built.** The list page shows full bodies inline (titles + body). Materials are short marketing/help blurbs in this codebase; pagination is fine if there are many.

## Prerequisites

- Phase 1 merged to `master`. HEAD: `99b0a2d Add role-boundary integration matrix for /Account pages`.
- `dotnet build CoreX.sln --nologo` → 0 errors.
- `dotnet test CoreX.sln --nologo --no-build` → 25/25 passing.

## Backend surface used by this phase (verified on master)

| Surface | Returns / Notes |
|---|---|
| `IClubService.GetAllAsync()` | `Task<List<ClubResponseDto>>` |
| `IClubService.GetByCityAsync(string city)` | `Task<List<ClubResponseDto>>` (note: `Async` suffix; case-sensitive exact match per `ClubService`) |
| `IClubService.GetByIdAsync(Guid id)` | `Task<ClubResponseDto?>` |
| `ClubResponseDto` fields | `Guid Id`, `string Name`, `string City`, `string Address`, `string? Description`, `string? Phone`, `string? Email`, `string? WorkingHours`, `string? PhotoUrl`, `double? Latitude`, `double? Longitude` |
| `ITrainerService.GetByClubIdAsync(Guid clubId)` | `Task<List<TrainerResponseDto>>` |
| `ITrainerService.GetByIdAsync(Guid id)` | `Task<TrainerResponseDto?>` |
| `TrainerResponseDto` fields | `Guid Id`, `Guid ClubId`, `string? ClubName`, `string FullName`, `string Specialization`, `int ExperienceYears`, `string? Bio`, `string? Email`, `string? Phone` |
| `IGroupClassService.GetByClubIdAsync(Guid clubId, GroupClassAudience? audience = null)` | `Task<List<GroupClassResponseDto>>` — no `GetAllAsync` |
| `GroupClassResponseDto` fields | `Guid Id`, `Guid ClubId`, `Guid? TrainerId`, `string? TrainerFullName`, `string Type`, `string? Description`, `string Audience` (serialized enum), `DateTime StartTime`, `int DurationMinutes`, `int Capacity`, `decimal? Price` |
| `IVacancyService.GetByClubIdAsync(Guid clubId)` | `Task<List<VacancyResponseDto>>` |
| `IVacancyService.GetActiveAsync()` | `Task<List<VacancyResponseDto>>` (used if we ever want a global vacancy list — not in Phase 2 scope) |
| `VacancyResponseDto` fields | `Guid Id`, `Guid ClubId`, `string? ClubName`, `string Title`, `string Description`, `string Requirements`, `decimal? Salary`, `bool IsActive`, `DateTime CreatedAt`, `DateTime? ApplicationDeadline`, `int ApplicationsCount` |
| `IDiscountService.GetActiveAsync()` | `Task<List<DiscountResponseDto>>` — discounts are global, no `ClubId` |
| `DiscountResponseDto` fields | `Guid Id`, `string Title`, `string? Description`, `decimal? DiscountPercent`, `string? Conditions`, `string? PromoCode`, `DateTime? StartDate`, `DateTime? EndDate`, `bool IsActive` |
| `IInformationMaterialService.GetAllAsync()` | `Task<List<InformationMaterialResponseDto>>` |
| `InformationMaterialResponseDto` fields | `Guid Id`, `string Title`, `string Body`, `string? Category`, `DateTime CreatedAt`, `DateTime? UpdatedAt` |
| HTMX detection | Server reads `HX-Request: true`. The `Request.Headers["HX-Request"] == "true"` check is canonical — Phase 2 introduces an extension method `Request.IsHtmx()` to centralize it. |

## File map

**New files:**

| File | Responsibility |
|---|---|
| `CoreX/Pages/Clubs/Index.cshtml` + `.cshtml.cs` | List of clubs, optional `?city=` filter via `IClubService.GetByCityAsync`, otherwise `GetAllAsync`. |
| `CoreX/Pages/Clubs/Detail.cshtml` + `.cshtml.cs` | `/Clubs/{id}` overview + tab nav. HTMX handlers `OnGetTrainers`, `OnGetGroupClasses`, `OnGetVacancies` return partials. |
| `CoreX/Pages/Clubs/_TrainersList.cshtml` | HTMX swap partial for the Trainers tab. |
| `CoreX/Pages/Clubs/_GroupClassesList.cshtml` | HTMX swap partial for the GroupClasses tab. |
| `CoreX/Pages/Clubs/_VacanciesList.cshtml` | HTMX swap partial for the Vacancies tab. |
| `CoreX/Pages/Trainers/Detail.cshtml` + `.cshtml.cs` | `/Trainers/{id}` single trainer view. |
| `CoreX/Pages/Discounts/Index.cshtml` + `.cshtml.cs` | `/Discounts` list of active discounts. |
| `CoreX/Pages/InformationMaterials/Index.cshtml` + `.cshtml.cs` | `/InformationMaterials` list of materials with full body inline. |
| `CoreX/Http/HttpRequestExtensions.cs` | `public static bool IsHtmx(this HttpRequest request)` — single source for the header check. |
| `CoreX.UI.Tests/TestSupport/HtmxClient.cs` | Helper that builds `HttpRequestMessage` with `HX-Request: true`. |
| `CoreX.UI.Tests/Pages/Clubs/IndexTests.cs` | List + city-filter tests. |
| `CoreX.UI.Tests/Pages/Clubs/DetailTests.cs` | Overview + 3 HTMX tab handlers. |
| `CoreX.UI.Tests/Pages/Trainers/DetailTests.cs` | Trainer detail. |
| `CoreX.UI.Tests/Pages/Discounts/IndexTests.cs` | Discount list. |
| `CoreX.UI.Tests/Pages/InformationMaterials/IndexTests.cs` | Materials list. |
| `CoreX.UI.Tests/TestSupport/SeedData.cs` | Helpers that seed a few `Club`, `Trainer`, `GroupClass`, `Vacancy`, `Discount`, `InformationMaterial` rows into the test InMemoryDb. Phase 2 is the first phase that needs real test data, so a single seed helper centralizes it. |

**Modified files:**

| File | Change |
|---|---|
| `CoreX/Pages/Index.cshtml` + `.cshtml.cs` | Add "Featured clubs" section beneath the hero (calls `IClubService.GetAllAsync()` and takes the first 6). |
| `CoreX.UI.Tests/Pages/IndexTests.cs` | Add a third test asserting the featured-clubs section renders. |

**Out of scope (deferred to later phases):**

- `/Clubs/{id}` Memberships tab and `/Memberships` catalog (Phase 3).
- Home city picker dropdown UI (Phase 6 polish).
- `/InformationMaterials/{id}` detail page.
- Pagination, search, sorting controls.
- Map rendering of `Latitude`/`Longitude` on club detail.
- EN translation of any of these pages.

---

## Task 1 — HTMX detection extension + test client helper

**Files:**
- Create: `CoreX/Http/HttpRequestExtensions.cs`
- Create: `CoreX.UI.Tests/TestSupport/HtmxClient.cs`

Centralize the `HX-Request: true` check so every HTMX-aware PageModel reads from one place, and give the test project a one-liner for sending HTMX-flavored GETs.

- [ ] **Step 1: Create the extension**

`CoreX/Http/HttpRequestExtensions.cs`:

```csharp
namespace CoreX.Http;

public static class HttpRequestExtensions
{
    // HTMX sets HX-Request: true on every request it issues (including hx-boost
    // navigations). Use this to branch between "full page" and "partial swap" responses.
    public static bool IsHtmx(this HttpRequest request) =>
        request.Headers.TryGetValue("HX-Request", out var v) && v == "true";
}
```

- [ ] **Step 2: Create the test client helper**

`CoreX.UI.Tests/TestSupport/HtmxClient.cs`:

```csharp
namespace CoreX.UI.Tests.TestSupport;

public static class HtmxClient
{
    public static Task<HttpResponseMessage> GetHxAsync(this HttpClient client, string url)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Add("HX-Request", "true");
        return client.SendAsync(req);
    }
}
```

- [ ] **Step 3: Build**

```bash
dotnet build CoreX.sln --nologo
```

Expected: 0 errors.

- [ ] **Step 4: Commit**

```bash
git add CoreX/Http/HttpRequestExtensions.cs CoreX.UI.Tests/TestSupport/HtmxClient.cs
git commit -m "Add HX-Request detection extension + test client helper"
```

---

## Task 2 — Test seed helpers

**Files:**
- Create: `CoreX.UI.Tests/TestSupport/SeedData.cs`

Phase 2 is the first phase whose tests need real `Club`/`Trainer`/`GroupClass`/`Vacancy`/`Discount`/`InformationMaterial` rows in the test InMemoryDb. Centralize the seeding so every test stays focused on assertions.

- [ ] **Step 1: Inspect the domain entity constructors**

Before writing the seed helper, read the entity constructors so the helper passes the right parameters in the right order:

```bash
ls CoreX.Domain/Entities/*.cs
```

Read each of: `Club.cs`, `Trainer.cs`, `GroupClass.cs`, `Vacancy.cs`, `Discount.cs`, `InformationMaterial.cs`. Note constructor parameters, required fields, and which factory methods (if any) exist.

If a constructor signature doesn't match what this plan assumes, **stop and report**.

- [ ] **Step 2: Create the seed helper**

`CoreX.UI.Tests/TestSupport/SeedData.cs`:

```csharp
using CoreX.Domain.Entities;
using CoreX.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace CoreX.UI.Tests.TestSupport;

public static class SeedData
{
    public sealed record SeededClub(Guid Id, string Name, string City);

    // Seeds two clubs in different cities, each with one trainer, one group class,
    // one vacancy. Plus two global discounts and two information materials.
    // Returns the seeded clubs so tests can reference their IDs.
    public static async Task<List<SeededClub>> SeedDiscoveryFixtureAsync(CoreXFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Bail out if already seeded by a previous test in the same fixture lifecycle.
        if (db.Clubs.Any())
        {
            return db.Clubs
                .Select(c => new SeededClub(c.Id, c.Name, c.City))
                .ToList();
        }

        // Construct entities. NOTE: replace these `new Club(...)` calls with whatever
        // the actual constructor surface looks like (Step 1's inspection). If the
        // entity uses a factory method, call that instead.
        var clubA = /* construct Club: name "Energy Kyiv", city "Київ", address "вул. Хрещатик, 1" */;
        var clubB = /* construct Club: name "Forge Lviv",  city "Львів", address "пр. Свободи, 5" */;

        db.Clubs.AddRange(clubA, clubB);

        // Trainer per club
        var trainerA = /* construct Trainer for clubA: FullName "Ірина Швець", Specialization "Силові", ExperienceYears 7 */;
        var trainerB = /* construct Trainer for clubB: FullName "Петро Шеремет", Specialization "Кросфіт", ExperienceYears 5 */;
        db.Trainers.AddRange(trainerA, trainerB);

        // Group class per club
        var classA = /* construct GroupClass for clubA, Type "Yoga", StartTime today+1h, DurationMinutes 60, Capacity 12, Audience Adults */;
        var classB = /* construct GroupClass for clubB, Type "Crossfit Lite", Audience Adults */;
        db.GroupClasses.AddRange(classA, classB);

        // Vacancy per club
        var vacancyA = /* construct active Vacancy for clubA: Title "Тренер з йоги", Requirements "сертифікат" */;
        var vacancyB = /* construct active Vacancy for clubB: Title "Адміністратор" */;
        db.Vacancies.AddRange(vacancyA, vacancyB);

        // Global discounts
        var discountA = /* construct Discount: Title "Студентам -15%", DiscountPercent 15, IsActive true */;
        var discountB = /* construct Discount: Title "Літня акція", DiscountPercent 25, IsActive true */;
        db.Discounts.AddRange(discountA, discountB);

        // Information materials
        var materialA = /* construct InformationMaterial: Title "Правила відвідування", Body "..." */;
        var materialB = /* construct InformationMaterial: Title "Як забронювати тренера", Body "..." */;
        db.InformationMaterials.AddRange(materialA, materialB);

        await db.SaveChangesAsync();

        return new()
        {
            new SeededClub(clubA.Id, clubA.Name, clubA.City),
            new SeededClub(clubB.Id, clubB.Name, clubB.City),
        };
    }
}
```

After Step 1's inspection, replace the `/* construct ... */` placeholders with the actual constructor calls. If the entities have public parameterless constructors + public setters (typical for an EF-mapped class), the seed becomes object initializers. If they have a constructor-only API, pass the parameters in the right order.

- [ ] **Step 3: Build (file may not compile until entities are wired in)**

If the build fails because of the placeholders, that's a sign Step 1 wasn't completed; revisit it.

- [ ] **Step 4: Commit**

```bash
git add CoreX.UI.Tests/TestSupport/SeedData.cs
git commit -m "Add seed helper for Phase 2 discovery tests"
```

---

## Task 3 — `/Clubs` index page (TDD)

**Files:**
- Create: `CoreX/Pages/Clubs/Index.cshtml`
- Create: `CoreX/Pages/Clubs/Index.cshtml.cs`
- Test: `CoreX.UI.Tests/Pages/Clubs/IndexTests.cs`

- [ ] **Step 1: Write failing tests**

`CoreX.UI.Tests/Pages/Clubs/IndexTests.cs`:

```csharp
using System.Net;
using CoreX.UI.Tests.TestSupport;
using Xunit;

namespace CoreX.UI.Tests.Pages.Clubs;

public class IndexTests : IClassFixture<CoreXFactory>
{
    private readonly CoreXFactory _factory;

    public IndexTests(CoreXFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_Clubs_ListsAllClubs()
    {
        await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/Clubs");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Energy Kyiv", body);
        Assert.Contains("Forge Lviv", body);
    }

    [Fact]
    public async Task Get_Clubs_WithCityFilter_ListsOnlyMatching()
    {
        await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/Clubs?city=%D0%9B%D1%8C%D0%B2%D1%96%D0%B2"); // "Львів"

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Forge Lviv", body);
        Assert.DoesNotContain("Energy Kyiv", body);
    }
}
```

- [ ] **Step 2: Run — confirm 2 fail (404)**

- [ ] **Step 3: Create the PageModel**

`CoreX/Pages/Clubs/Index.cshtml.cs`:

```csharp
using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Clubs;

public class IndexModel : PageModel
{
    private readonly IClubService _clubs;

    public IndexModel(IClubService clubs) => _clubs = clubs;

    [BindProperty(SupportsGet = true)]
    public string? City { get; set; }

    public IReadOnlyList<ClubResponseDto> Clubs { get; private set; } = Array.Empty<ClubResponseDto>();

    public async Task OnGetAsync()
    {
        Clubs = string.IsNullOrWhiteSpace(City)
            ? await _clubs.GetAllAsync()
            : await _clubs.GetByCityAsync(City);
    }
}
```

- [ ] **Step 4: Create the view**

`CoreX/Pages/Clubs/Index.cshtml`:

```cshtml
@page
@model CoreX.Pages.Clubs.IndexModel
@{
    ViewData["Title"] = "Клуби";
}

<section class="max-w-6xl mx-auto px-4 py-12 md:py-16">
    <h1 class="text-3xl md:text-4xl font-black uppercase tracking-tight">Клуби</h1>
    @if (!string.IsNullOrWhiteSpace(Model.City))
    {
        <p class="mt-2 text-ink-500">Місто: <span class="font-semibold">@Model.City</span> · <a href="/Clubs" class="text-brand-500 hover:underline">показати всі</a></p>
    }

    @if (Model.Clubs.Count == 0)
    {
        <p class="mt-10 text-ink-500">Поки немає клубів за обраним містом.</p>
    }
    else
    {
        <ul class="mt-10 grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            @foreach (var c in Model.Clubs)
            {
                <li class="rounded-card border border-ink-200 bg-white p-5">
                    <p class="text-xs font-semibold tracking-[0.2em] uppercase text-brand-500">@c.City</p>
                    <h2 class="mt-2 text-xl font-bold text-ink-900">@c.Name</h2>
                    <p class="mt-1 text-sm text-ink-500">@c.Address</p>
                    <a asp-page="/Clubs/Detail" asp-route-id="@c.Id" class="btn-ghost mt-4 inline-flex">Деталі →</a>
                </li>
            }
        </ul>
    }
</section>
```

- [ ] **Step 5: Run tests — confirm pass**

- [ ] **Step 6: Commit**

```bash
git add CoreX/Pages/Clubs/Index.cshtml CoreX/Pages/Clubs/Index.cshtml.cs CoreX.UI.Tests/Pages/Clubs/IndexTests.cs
git commit -m "Add /Clubs list page with optional city filter + TDD"
```

---

## Task 4 — `/Clubs/{id}` detail page (overview only, no tabs yet) — TDD

**Files:**
- Create: `CoreX/Pages/Clubs/Detail.cshtml`
- Create: `CoreX/Pages/Clubs/Detail.cshtml.cs`
- Test: `CoreX.UI.Tests/Pages/Clubs/DetailTests.cs`

- [ ] **Step 1: Write failing tests**

`CoreX.UI.Tests/Pages/Clubs/DetailTests.cs`:

```csharp
using System.Net;
using CoreX.UI.Tests.TestSupport;
using Xunit;

namespace CoreX.UI.Tests.Pages.Clubs;

public class DetailTests : IClassFixture<CoreXFactory>
{
    private readonly CoreXFactory _factory;

    public DetailTests(CoreXFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_ClubDetail_ShowsClubInfo()
    {
        var clubs = await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/Clubs/{clubs[0].Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains(clubs[0].Name, body);
        Assert.Contains(clubs[0].City, body);
    }

    [Fact]
    public async Task Get_ClubDetail_UnknownId_Returns404()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/Clubs/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
```

- [ ] **Step 2: Run — confirm 2 fail**

- [ ] **Step 3: Create the PageModel**

`CoreX/Pages/Clubs/Detail.cshtml.cs`:

```csharp
using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Clubs;

public class DetailModel : PageModel
{
    private readonly IClubService _clubs;

    public DetailModel(IClubService clubs) => _clubs = clubs;

    public ClubResponseDto Club { get; private set; } = default!;

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var club = await _clubs.GetByIdAsync(id);
        if (club is null) return NotFound();

        Club = club;
        return Page();
    }
}
```

- [ ] **Step 4: Create the view**

`CoreX/Pages/Clubs/Detail.cshtml`:

```cshtml
@page "/Clubs/{id:guid}"
@model CoreX.Pages.Clubs.DetailModel
@{
    ViewData["Title"] = Model.Club.Name;
}

<section class="max-w-5xl mx-auto px-4 py-12 md:py-16">
    <p class="text-xs font-semibold tracking-[0.2em] uppercase text-brand-500">@Model.Club.City</p>
    <h1 class="mt-2 text-3xl md:text-4xl font-black uppercase tracking-tight">@Model.Club.Name</h1>
    <p class="mt-2 text-ink-500">@Model.Club.Address</p>

    @if (!string.IsNullOrWhiteSpace(Model.Club.Description))
    {
        <p class="mt-6 text-ink-800 leading-relaxed max-w-2xl">@Model.Club.Description</p>
    }

    <dl class="mt-8 grid grid-cols-1 md:grid-cols-3 gap-6 text-sm">
        @if (!string.IsNullOrWhiteSpace(Model.Club.Phone))
        {
            <div><dt class="text-xs uppercase tracking-wide text-ink-500 font-semibold">Телефон</dt><dd class="mt-1">@Model.Club.Phone</dd></div>
        }
        @if (!string.IsNullOrWhiteSpace(Model.Club.Email))
        {
            <div><dt class="text-xs uppercase tracking-wide text-ink-500 font-semibold">Email</dt><dd class="mt-1 break-all">@Model.Club.Email</dd></div>
        }
        @if (!string.IsNullOrWhiteSpace(Model.Club.WorkingHours))
        {
            <div><dt class="text-xs uppercase tracking-wide text-ink-500 font-semibold">Години роботи</dt><dd class="mt-1">@Model.Club.WorkingHours</dd></div>
        }
    </dl>

    <nav class="mt-12 border-b border-ink-200 flex gap-2 text-sm font-semibold" role="tablist">
        <button type="button" hx-get="/Clubs/@Model.Club.Id?handler=Trainers"
                hx-target="#tab-content" hx-swap="innerHTML"
                class="px-4 py-3 hover:text-brand-500">Тренери</button>
        <button type="button" hx-get="/Clubs/@Model.Club.Id?handler=GroupClasses"
                hx-target="#tab-content" hx-swap="innerHTML"
                class="px-4 py-3 hover:text-brand-500">Заняття</button>
        <button type="button" hx-get="/Clubs/@Model.Club.Id?handler=Vacancies"
                hx-target="#tab-content" hx-swap="innerHTML"
                class="px-4 py-3 hover:text-brand-500">Вакансії</button>
    </nav>

    <div id="tab-content" class="mt-6"></div>
</section>
```

- [ ] **Step 5: Run tests — confirm pass**

- [ ] **Step 6: Commit**

```bash
git add CoreX/Pages/Clubs/Detail.cshtml CoreX/Pages/Clubs/Detail.cshtml.cs CoreX.UI.Tests/Pages/Clubs/DetailTests.cs
git commit -m "Add /Clubs/{id} detail page (overview, no tab handlers yet) + TDD"
```

---

## Task 5 — Club detail HTMX tab handlers (Trainers / GroupClasses / Vacancies)

**Files:**
- Modify: `CoreX/Pages/Clubs/Detail.cshtml.cs`
- Create: `CoreX/Pages/Clubs/_TrainersList.cshtml`
- Create: `CoreX/Pages/Clubs/_GroupClassesList.cshtml`
- Create: `CoreX/Pages/Clubs/_VacanciesList.cshtml`
- Test: append to `CoreX.UI.Tests/Pages/Clubs/DetailTests.cs`

Add 3 named handlers to `DetailModel` that return partials when called with `HX-Request: true`. Same pattern for each tab — only the service call differs.

- [ ] **Step 1: Add failing tests**

Append to `CoreX.UI.Tests/Pages/Clubs/DetailTests.cs`:

```csharp
[Fact]
public async Task GetHx_TrainersHandler_ReturnsPartialWithTrainerName()
{
    var clubs = await SeedData.SeedDiscoveryFixtureAsync(_factory);
    var client = _factory.CreateClient();

    var response = await client.GetHxAsync($"/Clubs/{clubs[0].Id}?handler=Trainers");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var body = await response.Content.ReadAsStringAsync();
    Assert.DoesNotContain("<html", body); // partial, no full layout
    Assert.Contains("Ірина Швець", body);
}

[Fact]
public async Task GetHx_GroupClassesHandler_ReturnsPartialWithClassType()
{
    var clubs = await SeedData.SeedDiscoveryFixtureAsync(_factory);
    var client = _factory.CreateClient();

    var response = await client.GetHxAsync($"/Clubs/{clubs[0].Id}?handler=GroupClasses");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var body = await response.Content.ReadAsStringAsync();
    Assert.DoesNotContain("<html", body);
    Assert.Contains("Yoga", body);
}

[Fact]
public async Task GetHx_VacanciesHandler_ReturnsPartialWithVacancyTitle()
{
    var clubs = await SeedData.SeedDiscoveryFixtureAsync(_factory);
    var client = _factory.CreateClient();

    var response = await client.GetHxAsync($"/Clubs/{clubs[0].Id}?handler=Vacancies");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var body = await response.Content.ReadAsStringAsync();
    Assert.DoesNotContain("<html", body);
    Assert.Contains("Тренер з йоги", body);
}

[Fact]
public async Task Get_TrainersHandler_WithoutHxHeader_Returns404()
{
    var clubs = await SeedData.SeedDiscoveryFixtureAsync(_factory);
    var client = _factory.CreateClient();

    // Non-HTMX direct hit should not render a partial as a full page.
    var response = await client.GetAsync($"/Clubs/{clubs[0].Id}?handler=Trainers");

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
}
```

- [ ] **Step 2: Run — confirm 4 fail**

- [ ] **Step 3: Extend the PageModel**

Update `CoreX/Pages/Clubs/Detail.cshtml.cs`:

```csharp
using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using CoreX.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Clubs;

public class DetailModel : PageModel
{
    private readonly IClubService _clubs;
    private readonly ITrainerService _trainers;
    private readonly IGroupClassService _groupClasses;
    private readonly IVacancyService _vacancies;

    public DetailModel(
        IClubService clubs,
        ITrainerService trainers,
        IGroupClassService groupClasses,
        IVacancyService vacancies)
    {
        _clubs = clubs;
        _trainers = trainers;
        _groupClasses = groupClasses;
        _vacancies = vacancies;
    }

    public ClubResponseDto Club { get; private set; } = default!;
    public IReadOnlyList<TrainerResponseDto> Trainers { get; private set; } = Array.Empty<TrainerResponseDto>();
    public IReadOnlyList<GroupClassResponseDto> GroupClasses { get; private set; } = Array.Empty<GroupClassResponseDto>();
    public IReadOnlyList<VacancyResponseDto> Vacancies { get; private set; } = Array.Empty<VacancyResponseDto>();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var club = await _clubs.GetByIdAsync(id);
        if (club is null) return NotFound();
        Club = club;
        return Page();
    }

    public async Task<IActionResult> OnGetTrainersAsync(Guid id)
    {
        if (!Request.IsHtmx()) return NotFound();
        Trainers = await _trainers.GetByClubIdAsync(id);
        return Partial("_TrainersList", this);
    }

    public async Task<IActionResult> OnGetGroupClassesAsync(Guid id)
    {
        if (!Request.IsHtmx()) return NotFound();
        GroupClasses = await _groupClasses.GetByClubIdAsync(id);
        return Partial("_GroupClassesList", this);
    }

    public async Task<IActionResult> OnGetVacanciesAsync(Guid id)
    {
        if (!Request.IsHtmx()) return NotFound();
        Vacancies = await _vacancies.GetByClubIdAsync(id);
        return Partial("_VacanciesList", this);
    }
}
```

- [ ] **Step 4: Create `_TrainersList.cshtml`**

```cshtml
@model CoreX.Pages.Clubs.DetailModel

@if (Model.Trainers.Count == 0)
{
    <p class="text-ink-500">Тренерів поки немає.</p>
}
else
{
    <ul class="grid grid-cols-1 md:grid-cols-2 gap-4">
    @foreach (var t in Model.Trainers)
    {
        <li class="rounded-card border border-ink-200 p-4">
            <p class="font-bold text-ink-900">@t.FullName</p>
            <p class="text-sm text-ink-500">@t.Specialization · @t.ExperienceYears р. досвіду</p>
        </li>
    }
    </ul>
}
```

- [ ] **Step 5: Create `_GroupClassesList.cshtml`**

```cshtml
@model CoreX.Pages.Clubs.DetailModel

@if (Model.GroupClasses.Count == 0)
{
    <p class="text-ink-500">Групових занять поки немає.</p>
}
else
{
    <ul class="divide-y divide-ink-200">
    @foreach (var g in Model.GroupClasses)
    {
        <li class="py-4 flex justify-between gap-4">
            <div>
                <p class="font-bold">@g.Type</p>
                <p class="text-sm text-ink-500">@g.StartTime.ToString("d MMMM, HH:mm") · @g.DurationMinutes хв · @g.Audience</p>
            </div>
            <span class="text-xs text-ink-500">@g.TrainerFullName</span>
        </li>
    }
    </ul>
}
```

- [ ] **Step 6: Create `_VacanciesList.cshtml`**

```cshtml
@model CoreX.Pages.Clubs.DetailModel

@if (Model.Vacancies.Count == 0)
{
    <p class="text-ink-500">Відкритих вакансій немає.</p>
}
else
{
    <ul class="space-y-4">
    @foreach (var v in Model.Vacancies)
    {
        <li class="rounded-card border border-ink-200 p-4">
            <p class="font-bold text-ink-900">@v.Title</p>
            <p class="mt-1 text-sm text-ink-500">@v.Description</p>
        </li>
    }
    </ul>
}
```

- [ ] **Step 7: Run tests — confirm pass**

- [ ] **Step 8: Commit**

```bash
git add CoreX/Pages/Clubs/Detail.cshtml.cs CoreX/Pages/Clubs/_TrainersList.cshtml CoreX/Pages/Clubs/_GroupClassesList.cshtml CoreX/Pages/Clubs/_VacanciesList.cshtml CoreX.UI.Tests/Pages/Clubs/DetailTests.cs
git commit -m "Add HTMX tabs (Trainers/GroupClasses/Vacancies) on /Clubs/{id}"
```

---

## Task 6 — `/Trainers/{id}` page (TDD)

**Files:**
- Create: `CoreX/Pages/Trainers/Detail.cshtml`
- Create: `CoreX/Pages/Trainers/Detail.cshtml.cs`
- Test: `CoreX.UI.Tests/Pages/Trainers/DetailTests.cs`

- [ ] **Step 1: Write failing tests**

```csharp
using System.Net;
using CoreX.UI.Tests.TestSupport;
using Xunit;

namespace CoreX.UI.Tests.Pages.Trainers;

public class DetailTests : IClassFixture<CoreXFactory>
{
    private readonly CoreXFactory _factory;
    public DetailTests(CoreXFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_TrainerDetail_ShowsTrainerInfo()
    {
        var clubs = await SeedData.SeedDiscoveryFixtureAsync(_factory);
        // Find the trainer for clubA via the service (Phase 2 tests can use DI scope).
        using var scope = _factory.Services.CreateScope();
        var trainerService = scope.ServiceProvider.GetRequiredService<CoreX.Application.ServiceInterfaces.ITrainerService>();
        var trainers = await trainerService.GetByClubIdAsync(clubs[0].Id);
        var trainerId = trainers[0].Id;

        var client = _factory.CreateClient();
        var response = await client.GetAsync($"/Trainers/{trainerId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Ірина Швець", body);
        Assert.Contains("Силові", body);
    }

    [Fact]
    public async Task Get_TrainerDetail_UnknownId_Returns404()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync($"/Trainers/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
```

Add `using Microsoft.Extensions.DependencyInjection;` at the top.

- [ ] **Step 2: Run — confirm 2 fail**

- [ ] **Step 3: PageModel**

```csharp
using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Trainers;

public class DetailModel : PageModel
{
    private readonly ITrainerService _trainers;
    public DetailModel(ITrainerService trainers) => _trainers = trainers;

    public TrainerResponseDto Trainer { get; private set; } = default!;

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var trainer = await _trainers.GetByIdAsync(id);
        if (trainer is null) return NotFound();
        Trainer = trainer;
        return Page();
    }
}
```

- [ ] **Step 4: View**

```cshtml
@page "/Trainers/{id:guid}"
@model CoreX.Pages.Trainers.DetailModel
@{
    ViewData["Title"] = Model.Trainer.FullName;
}

<section class="max-w-3xl mx-auto px-4 py-12 md:py-16">
    <p class="text-xs font-semibold tracking-[0.2em] uppercase text-brand-500">@Model.Trainer.Specialization</p>
    <h1 class="mt-2 text-3xl md:text-4xl font-black uppercase tracking-tight">@Model.Trainer.FullName</h1>
    <p class="mt-2 text-ink-500">@Model.Trainer.ExperienceYears років досвіду @if (Model.Trainer.ClubName is not null) { <text> · @Model.Trainer.ClubName</text> }</p>

    @if (!string.IsNullOrWhiteSpace(Model.Trainer.Bio))
    {
        <p class="mt-6 text-ink-800 leading-relaxed">@Model.Trainer.Bio</p>
    }

    @if (!string.IsNullOrWhiteSpace(Model.Trainer.Email) || !string.IsNullOrWhiteSpace(Model.Trainer.Phone))
    {
        <dl class="mt-8 grid grid-cols-1 md:grid-cols-2 gap-6 text-sm">
            @if (!string.IsNullOrWhiteSpace(Model.Trainer.Email))
            {
                <div><dt class="text-xs uppercase tracking-wide text-ink-500 font-semibold">Email</dt><dd class="mt-1 break-all">@Model.Trainer.Email</dd></div>
            }
            @if (!string.IsNullOrWhiteSpace(Model.Trainer.Phone))
            {
                <div><dt class="text-xs uppercase tracking-wide text-ink-500 font-semibold">Телефон</dt><dd class="mt-1">@Model.Trainer.Phone</dd></div>
            }
        </dl>
    }
</section>
```

- [ ] **Step 5: Run + commit**

```bash
git add CoreX/Pages/Trainers/Detail.cshtml CoreX/Pages/Trainers/Detail.cshtml.cs CoreX.UI.Tests/Pages/Trainers/DetailTests.cs
git commit -m "Add /Trainers/{id} detail page + TDD"
```

---

## Task 7 — `/Discounts` page (TDD)

**Files:**
- Create: `CoreX/Pages/Discounts/Index.cshtml`
- Create: `CoreX/Pages/Discounts/Index.cshtml.cs`
- Test: `CoreX.UI.Tests/Pages/Discounts/IndexTests.cs`

- [ ] **Step 1: Failing test**

```csharp
using System.Net;
using CoreX.UI.Tests.TestSupport;
using Xunit;

namespace CoreX.UI.Tests.Pages.Discounts;

public class IndexTests : IClassFixture<CoreXFactory>
{
    private readonly CoreXFactory _factory;
    public IndexTests(CoreXFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_Discounts_ListsActiveOnes()
    {
        await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/Discounts");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Студентам -15%", body);
        Assert.Contains("Літня акція", body);
    }
}
```

- [ ] **Step 2: PageModel**

```csharp
using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Discounts;

public class IndexModel : PageModel
{
    private readonly IDiscountService _discounts;
    public IndexModel(IDiscountService discounts) => _discounts = discounts;

    public IReadOnlyList<DiscountResponseDto> Discounts { get; private set; } = Array.Empty<DiscountResponseDto>();

    public async Task OnGetAsync() => Discounts = await _discounts.GetActiveAsync();
}
```

- [ ] **Step 3: View**

```cshtml
@page
@model CoreX.Pages.Discounts.IndexModel
@{
    ViewData["Title"] = "Акції";
}

<section class="max-w-5xl mx-auto px-4 py-12 md:py-16">
    <h1 class="text-3xl md:text-4xl font-black uppercase tracking-tight">Акції</h1>

    @if (Model.Discounts.Count == 0)
    {
        <p class="mt-8 text-ink-500">Активних акцій немає.</p>
    }
    else
    {
        <ul class="mt-10 grid grid-cols-1 md:grid-cols-2 gap-6">
        @foreach (var d in Model.Discounts)
        {
            <li class="rounded-card border border-ink-200 bg-white p-5">
                @if (d.DiscountPercent is not null)
                {
                    <p class="text-3xl font-black text-brand-500">-@(d.DiscountPercent.Value.ToString("0"))%</p>
                }
                <h2 class="mt-2 text-lg font-bold text-ink-900">@d.Title</h2>
                @if (!string.IsNullOrWhiteSpace(d.Description))
                {
                    <p class="mt-2 text-sm text-ink-500">@d.Description</p>
                }
                @if (!string.IsNullOrWhiteSpace(d.PromoCode))
                {
                    <p class="mt-3 text-xs uppercase tracking-wider text-ink-800">Промокод: <span class="font-mono font-bold">@d.PromoCode</span></p>
                }
            </li>
        }
        </ul>
    }
</section>
```

- [ ] **Step 4: Run + commit**

```bash
git add CoreX/Pages/Discounts/Index.cshtml CoreX/Pages/Discounts/Index.cshtml.cs CoreX.UI.Tests/Pages/Discounts/IndexTests.cs
git commit -m "Add /Discounts list page + TDD"
```

---

## Task 8 — `/InformationMaterials` page (TDD)

**Files:**
- Create: `CoreX/Pages/InformationMaterials/Index.cshtml`
- Create: `CoreX/Pages/InformationMaterials/Index.cshtml.cs`
- Test: `CoreX.UI.Tests/Pages/InformationMaterials/IndexTests.cs`

- [ ] **Step 1: Failing test**

```csharp
using System.Net;
using CoreX.UI.Tests.TestSupport;
using Xunit;

namespace CoreX.UI.Tests.Pages.InformationMaterials;

public class IndexTests : IClassFixture<CoreXFactory>
{
    private readonly CoreXFactory _factory;
    public IndexTests(CoreXFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_Materials_ListsAll()
    {
        await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/InformationMaterials");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Правила відвідування", body);
        Assert.Contains("Як забронювати тренера", body);
    }
}
```

- [ ] **Step 2: PageModel**

```csharp
using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.InformationMaterials;

public class IndexModel : PageModel
{
    private readonly IInformationMaterialService _materials;
    public IndexModel(IInformationMaterialService materials) => _materials = materials;

    public IReadOnlyList<InformationMaterialResponseDto> Materials { get; private set; } = Array.Empty<InformationMaterialResponseDto>();

    public async Task OnGetAsync() => Materials = await _materials.GetAllAsync();
}
```

- [ ] **Step 3: View**

```cshtml
@page
@model CoreX.Pages.InformationMaterials.IndexModel
@{
    ViewData["Title"] = "Інформація";
}

<section class="max-w-4xl mx-auto px-4 py-12 md:py-16">
    <h1 class="text-3xl md:text-4xl font-black uppercase tracking-tight">Інформація</h1>

    @if (Model.Materials.Count == 0)
    {
        <p class="mt-8 text-ink-500">Матеріалів поки немає.</p>
    }
    else
    {
        <div class="mt-10 space-y-10">
        @foreach (var m in Model.Materials)
        {
            <article>
                @if (!string.IsNullOrWhiteSpace(m.Category))
                {
                    <p class="text-xs font-semibold tracking-[0.2em] uppercase text-brand-500">@m.Category</p>
                }
                <h2 class="mt-2 text-2xl font-bold text-ink-900">@m.Title</h2>
                <p class="mt-3 text-ink-800 leading-relaxed whitespace-pre-line">@m.Body</p>
            </article>
        }
        </div>
    }
</section>
```

- [ ] **Step 4: Run + commit**

```bash
git add CoreX/Pages/InformationMaterials/Index.cshtml CoreX/Pages/InformationMaterials/Index.cshtml.cs CoreX.UI.Tests/Pages/InformationMaterials/IndexTests.cs
git commit -m "Add /InformationMaterials list page + TDD"
```

---

## Task 9 — Home page: featured clubs section

**Files:**
- Modify: `CoreX/Pages/Index.cshtml`
- Modify: `CoreX/Pages/Index.cshtml.cs`
- Test: append to `CoreX.UI.Tests/Pages/IndexTests.cs`

- [ ] **Step 1: Add failing test**

Append to `CoreX.UI.Tests/Pages/IndexTests.cs`:

```csharp
[Fact]
public async Task Get_Index_ShowsFeaturedClubsSection()
{
    await CoreX.UI.Tests.TestSupport.SeedData.SeedDiscoveryFixtureAsync(_factory);
    var client = _factory.CreateClient();

    var response = await client.GetAsync("/");
    Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    var body = await response.Content.ReadAsStringAsync();
    Assert.Contains("Energy Kyiv", body);
    Assert.Contains("Forge Lviv", body);
}
```

(Existing 2 Index tests remain untouched.)

- [ ] **Step 2: Run — confirm new test fails**

- [ ] **Step 3: Extend PageModel**

`CoreX/Pages/Index.cshtml.cs`:

```csharp
using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages;

public class IndexModel : PageModel
{
    private readonly IClubService _clubs;
    public IndexModel(IClubService clubs) => _clubs = clubs;

    public IReadOnlyList<ClubResponseDto> Featured { get; private set; } = Array.Empty<ClubResponseDto>();

    public async Task OnGetAsync()
    {
        var all = await _clubs.GetAllAsync();
        Featured = all.Take(6).ToList();
    }
}
```

- [ ] **Step 4: Add the featured section to the view**

Edit `CoreX/Pages/Index.cshtml` — keep the existing hero `<section>` (Phase 0 content) and append a second `<section>` after it:

```cshtml
@if (Model.Featured.Count > 0)
{
    <section class="max-w-6xl mx-auto px-4 py-12 md:py-16">
        <div class="flex items-baseline justify-between gap-4">
            <h2 class="text-2xl md:text-3xl font-black uppercase tracking-tight">Наші клуби</h2>
            <a href="/Clubs" class="text-sm font-semibold text-brand-500 hover:underline">Усі клуби →</a>
        </div>
        <ul class="mt-8 grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        @foreach (var c in Model.Featured)
        {
            <li class="rounded-card border border-ink-200 bg-white p-5">
                <p class="text-xs font-semibold tracking-[0.2em] uppercase text-brand-500">@c.City</p>
                <h3 class="mt-2 text-lg font-bold text-ink-900">@c.Name</h3>
                <p class="mt-1 text-sm text-ink-500">@c.Address</p>
                <a asp-page="/Clubs/Detail" asp-route-id="@c.Id" class="btn-ghost mt-4 inline-flex">Деталі →</a>
            </li>
        }
        </ul>
    </section>
}
```

- [ ] **Step 5: Run all Index tests — confirm 3/3 pass**

- [ ] **Step 6: Commit**

```bash
git add CoreX/Pages/Index.cshtml CoreX/Pages/Index.cshtml.cs CoreX.UI.Tests/Pages/IndexTests.cs
git commit -m "Add featured clubs section to home"
```

---

## Task 10 — End-to-end smoke + final cleanup

- [ ] **Step 1: Build**

```bash
dotnet build CoreX.sln --nologo
```

Expected: 0 errors.

- [ ] **Step 2: Full test run**

```bash
dotnet test CoreX.sln --nologo --no-build
```

Expected: Phase 0 (3 — one new + 2 old) + Phase 1 (23) + Phase 2 (~15 across Clubs/Trainers/Discounts/InformationMaterials) ≈ 41 total, all passing.

- [ ] **Step 3: Browser smoke**

Start the app:

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet run --project CoreX/CoreX.UI.csproj --no-build --no-launch-profile --urls "http://localhost:5050"
```

Walk through (UA-only; the test fixture's seed isn't in the dev DB, so the lists may render empty if the dev DB isn't seeded — that's expected for v1):

1. `/` — Hero + featured clubs grid (may be empty in dev DB; that's fine).
2. `/Clubs` — list page; click into a club detail.
3. `/Clubs/{id}` — overview renders; click Тренери / Заняття / Вакансії tabs and confirm content swaps in via HTMX (visible network calls to `?handler=Trainers` etc., HTMX response body inserted into `#tab-content`).
4. `/Trainers/{id}` — single trainer page.
5. `/Discounts` — discount cards.
6. `/InformationMaterials` — list with inline bodies.

Stop with Ctrl+C.

- [ ] **Step 4: `git status` clean**

Expected: no tracked changes; only pre-existing untracked.

---

## Phase 2 exit checklist

- [ ] `dotnet build CoreX.sln` → 0 errors.
- [ ] `dotnet test CoreX.sln` → all tests passing (Phase 0 + Phase 1 + Phase 2 ≈ 41 total).
- [ ] `/Clubs` lists every seeded club.
- [ ] `/Clubs?city=Львів` filters to clubs in that city only.
- [ ] `/Clubs/{id}` renders the club's name + address; the three HTMX tab buttons swap their tab partial into `#tab-content` when clicked (manual browser check).
- [ ] `/Clubs/{id}?handler=Trainers` returns 404 without `HX-Request: true` (defense against direct partial scraping).
- [ ] `/Trainers/{id}` renders the trainer's full name + specialization.
- [ ] `/Discounts` renders all active discounts.
- [ ] `/InformationMaterials` renders all materials with their full body.
- [ ] Home page shows the "Наші клуби" section after the hero.

**Next phase:** Phase 3 — Memberships + booking (subscription catalog, anonymous booking form, `IBookingService.CreateAsync(Guid?, …)` signature change, Memberships tab on `/Clubs/{id}`, `/Account/MyBookings` becomes populated).
