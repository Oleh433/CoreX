# Frontend Phase 3 — Memberships + Booking Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax.

**Goal:** Ship the subscription catalog (`/Memberships?clubId=...`) and the anonymous-friendly booking form (`/Memberships/{subId}/Book`) that creates a `Booking` record via `IBookingService`. Make booking work for both authenticated users and anonymous visitors, completing the demo-able critical path through the public site. Also adds the deferred Memberships HTMX tab on `/Clubs/{id}` and a populated-state test on `/Account/MyBookings`.

**Architecture:** Razor Pages under `CoreX/Pages/Memberships/`. PageModels call `ISubscriptionService` and `IBookingService` via DI. The single backend change in the whole frontend roadmap lands here: `IBookingService.CreateAsync(Guid userId, ...)` becomes `CreateAsync(Guid? userId, ...)` and `Booking.UserId` becomes `Guid?`. The existing `BookingsController` (now at `/api/bookings`) still passes a non-null `Guid` from the JWT claim — its behaviour is preserved. UA-hardcoded strings, same pattern as Phase 1 + 2.

**Tech Stack:** ASP.NET Core 8 Razor Pages · EF Core 8 (InMemory in tests, SQL Server in dev) · xUnit + `Microsoft.AspNetCore.Mvc.Testing`.

**Spec reference:** `docs/superpowers/specs/2026-05-20-frontend-design.md` — Phase 3 in §11, anonymous-booking design in §5, backend changes in §12.

---

## Scope cuts

- **No payment integration.** Booking remains a confirmation request reviewed by admin (per spec §13). Submission creates the `Booking` row with `Status = New`.
- **No discount-selection UI on the booking form** for Phase 3. The `CreateBookingDto.DiscountId` field stays in the DTO but the public form does not expose it — discount application happens admin-side. (Phase 6 polish: add a promo-code box if needed.)
- **MyBookings still has no row actions.** Cancel / rebook deferred to Phase 6.

## Prerequisites

- Phase 2 merged to `master`. HEAD: `c19a7fe Add featured clubs section to home`.
- `dotnet build CoreX.sln --nologo` → 0 errors.
- `dotnet test CoreX.sln --nologo --no-build` → 38/38 passing.

## Backend surface used by this phase (verified against master HEAD `c19a7fe`)

| Surface | File / Notes |
|---|---|
| `IBookingService.CreateAsync(Guid userId, CreateBookingDto)` | `CoreX.Application/ServiceInterfaces/IBookingService.cs:15` — **target of the signature change in Task 1.** |
| `CreateBookingDto` fields | `ClubId`, `SubscriptionId`, `DiscountId?`, `ContactFullName`, `ContactEmail`, `ContactPhone` — plain POCO, no DataAnnotations. We add a Razor Pages `BookingInput` with localized DataAnnotations for the form. |
| `BookingService.CreateAsync` impl | `CoreX.Application/Services/BookingService.cs:57-88` — guards on empty `userId`, sends a confirmation email to `ContactEmail` after `unitOfWork.SaveChangesAsync()`. Task 1 drops the empty-guid guard for the null case. |
| `Booking` entity | `CoreX.Domain/Entities/Booking.cs` — `UserId` is `Guid` `[Required]`; ctor takes `Guid userId`. **Both change in Task 1.** Contact fields (`ContactFullName` 3-100, `ContactEmail`, `ContactPhone`) are required; `Status = New` after creation. |
| `ISubscriptionService.GetByClubIdAsync(Guid)` | `CoreX.Application/ServiceInterfaces/ISubscriptionService.cs:11` — used by the catalog and the club-detail tab. |
| `SubscriptionResponseDto.Title` | Title (not Name); also Description, Price, DurationDays, VisitsLimit?, IsActive. |
| `IBookingRepository.AddAsync(Booking)` | `CoreX.Domain/RepositoryInterfaces/IBookingRepository.cs` — service path; no change needed. |
| `BookingsController` | `CoreX/Controllers/BookingsController.cs` — `[Authorize]` at class level. `Create` extracts `User.FindFirstValue(ClaimTypes.NameIdentifier)` and passes a non-null `Guid`. **No change needed.** The signature change is binary-compatible (was `Guid`, becomes `Guid?` — same call site works). |
| Existing tests touching booking | None. Zero existing test breakage. |

## File map

**New files:**

| File | Responsibility |
|---|---|
| `CoreX/Pages/Memberships/Index.cshtml` + `.cshtml.cs` | `/Memberships?clubId=...` catalog. Required `clubId` (we don't show a global subscription list — subscriptions are per-club in this domain). |
| `CoreX/Pages/Memberships/Book.cshtml` + `.cshtml.cs` | `/Memberships/{subId}/Book` form. GET shows the form (pre-filled for authed users). POST calls `IBookingService.CreateAsync` and redirects to `Confirmed`. |
| `CoreX/Pages/Memberships/Confirmed.cshtml` + `.cshtml.cs` | Thank-you confirmation page (`/Memberships/Confirmed?bookingId=...`). |
| `CoreX/Pages/Memberships/Models/BookingInput.cs` | Razor Pages input model with hardcoded UA `ErrorMessage` strings on DataAnnotations. |
| `CoreX/Pages/Clubs/_MembershipsList.cshtml` | HTMX swap partial for the Memberships tab on club detail. |
| `CoreX.UI.Tests/Pages/Memberships/IndexTests.cs` | Catalog tests. |
| `CoreX.UI.Tests/Pages/Memberships/BookTests.cs` | Anonymous + authenticated booking tests. |

**Modified files:**

| File | Change |
|---|---|
| `CoreX.Domain/Entities/Booking.cs` | `UserId` `Guid` → `Guid?`; drop `[Required]`; ctor's `userId` parameter `Guid` → `Guid?`. |
| `CoreX.Application/ServiceInterfaces/IBookingService.cs` | `CreateAsync` first parameter `Guid userId` → `Guid? userId`. |
| `CoreX.Application/Services/BookingService.cs` | Drop the `userId == Guid.Empty` guard when `userId` is null; pass-through to `Booking` ctor. |
| `CoreX/Pages/Clubs/Detail.cshtml.cs` | Add `ISubscriptionService` to ctor; add `OnGetMembershipsAsync` handler (mirrors the 3 existing tab handlers); add `Subscriptions` property. |
| `CoreX/Pages/Clubs/Detail.cshtml` | Add a fourth tab button "Абонементи" → `?handler=Memberships`. |
| `CoreX.UI.Tests/TestSupport/SeedData.cs` | Append two `Subscription` rows (one per club) to the seed fixture so the catalog and HTMX-tab tests find data. |
| `CoreX.UI.Tests/Pages/Clubs/DetailTests.cs` | Append two tests for the Memberships HTMX handler (HTMX hit returns partial; non-HTMX hit returns 404). |
| `CoreX.UI.Tests/Pages/Account/MyBookingsTests.cs` | Append one populated-state test: book a subscription, sign in, GET `/Account/MyBookings`, assert the booking row renders. |
| `CoreX.Infrastructure/Migrations/<timestamp>_MakeBookingUserIdNullable.cs` | EF migration for the column change. Generated via `dotnet ef migrations add`. |

**Out of scope (deferred):**

- Stripe / payment integration.
- Promo-code / discount input on the booking form.
- Bookings cancel / re-book actions on `/Account/MyBookings`.
- Booking edit (no entity supports it; the existing `Cancel` flow is admin-only).
- Email confirmation rendering (already exists — sends to `ContactEmail` via existing `ConsoleEmailSender`; logs to stdout in dev).

---

## Task 1 — Backend signature change: nullable Booking.UserId

**Files:**
- Modify: `CoreX.Domain/Entities/Booking.cs`
- Modify: `CoreX.Application/ServiceInterfaces/IBookingService.cs`
- Modify: `CoreX.Application/Services/BookingService.cs`
- Migration: `CoreX.Infrastructure/Migrations/<timestamp>_MakeBookingUserIdNullable.cs` (auto-generated)

This is the only backend code change in the whole frontend roadmap. Make `Booking.UserId` nullable so anonymous bookings can be created. The existing API controller is unaffected — it passes a non-null `Guid` from the JWT claim; the nullable signature is binary-compatible at the call site.

- [ ] **Step 1: Update `Booking.cs`**

In `CoreX.Domain/Entities/Booking.cs`:

```csharp
// Before:
[Required]
public Guid UserId { get; private set; }

// After:
public Guid? UserId { get; private set; }
```

Update the constructor's first parameter:

```csharp
// Before:
public Booking(Guid userId, Guid clubId, Guid subscriptionId, string contactFullName, ...)

// After:
public Booking(Guid? userId, Guid clubId, Guid subscriptionId, string contactFullName, ...)
```

The ctor body just assigns `UserId = userId;` — no validation needed for the null case.

- [ ] **Step 2: Update `IBookingService.cs`**

```csharp
// Before:
Task<Guid> CreateAsync(Guid userId, CreateBookingDto dto);

// After:
Task<Guid> CreateAsync(Guid? userId, CreateBookingDto dto);
```

- [ ] **Step 3: Update `BookingService.cs`**

In `CoreX.Application/Services/BookingService.cs`, the `CreateAsync` method:
1. Change the parameter `Guid userId` to `Guid? userId`.
2. Remove the `if (userId == Guid.Empty) throw new ArgumentException("UserId is required.");` line.
3. Pass `userId` directly to the `Booking` ctor (no change in syntax — the call site is `new Booking(userId, ...)`).

Leave the rest of the method unchanged (the `_bookingRepository.AddAsync` + `_unitOfWork.SaveChangesAsync` + `_emailSender.SendAsync` sequence is correct; email goes to `dto.ContactEmail`, which is required for both anonymous and authenticated paths).

- [ ] **Step 4: Generate EF migration**

```bash
dotnet ef migrations add MakeBookingUserIdNullable --project CoreX.Infrastructure --startup-project CoreX
```

(If `dotnet ef` is not installed: `dotnet tool install --global dotnet-ef`. Verify with `dotnet ef --version`.)

The generated migration should `AlterColumn` on `Bookings.UserId` to be nullable. If the migration tool produces additional unrelated changes, abort it (revert + re-run) — only the UserId nullability should change.

If `dotnet ef` is unavailable in the environment, **report BLOCKED**. The InMemory test DB doesn't need the migration to work, but the migration file is required for any future deployment.

- [ ] **Step 5: Build the full solution**

```bash
dotnet build CoreX.sln --nologo
```

Expected: 0 errors. The `BookingsController.Create` call site `_bookingService.CreateAsync(userId, dto)` continues to compile because `Guid` is implicitly convertible to `Guid?`.

- [ ] **Step 6: Run existing tests — confirm no regression**

```bash
dotnet test CoreX.UI.Tests/CoreX.UI.Tests.csproj --nologo --no-build
```

Expected: 38/38 still passing.

- [ ] **Step 7: Commit**

```bash
git add CoreX.Domain/Entities/Booking.cs CoreX.Application/ServiceInterfaces/IBookingService.cs CoreX.Application/Services/BookingService.cs CoreX.Infrastructure/Migrations/
git commit -m "Allow nullable UserId on Booking for anonymous booking"
```

---

## Task 2 — `/Memberships?clubId=...` catalog page (TDD)

**Files:**
- Create: `CoreX/Pages/Memberships/Index.cshtml`
- Create: `CoreX/Pages/Memberships/Index.cshtml.cs`
- Modify: `CoreX.UI.Tests/TestSupport/SeedData.cs` (append 2 subscriptions)
- Test: `CoreX.UI.Tests/Pages/Memberships/IndexTests.cs`

Subscriptions are per-club, so the catalog requires `?clubId=...`. Without it, render a 400-ish "вкажіть клуб" instructional page or redirect to `/Clubs` so the visitor picks a club first.

- [ ] **Step 1: Append two subscriptions to `SeedData.cs`**

After the trainer block in `SeedDiscoveryFixtureAsync`, add (inside the same method):

```csharp
// Subscriptions — one active per club
var subA = new Subscription(clubA.Id, "Місячний", durationDays: 30, price: 800m,
    description: "Безліміт у клубі Energy Kyiv", visitsLimit: null);
var subB = new Subscription(clubB.Id, "Квартальний", durationDays: 90, price: 2100m,
    description: "Безліміт у клубі Forge Lviv", visitsLimit: null);
db.Subscriptions.AddRange(subA, subB);
```

*(Adjust the constructor signature to whatever `Subscription` actually takes — read `CoreX.Domain/Entities/Subscription.cs` first if uncertain; the inventory may be incomplete here.)*

- [ ] **Step 2: Write the failing test**

`CoreX.UI.Tests/Pages/Memberships/IndexTests.cs`:

```csharp
using System.Net;
using CoreX.UI.Tests.TestSupport;
using Xunit;

namespace CoreX.UI.Tests.Pages.Memberships;

public class IndexTests : IClassFixture<CoreXFactory>
{
    private readonly CoreXFactory _factory;
    public IndexTests(CoreXFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_Memberships_WithClubId_ListsSubscriptions()
    {
        var clubs = await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/Memberships?clubId={clubs[0].Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Місячний", body);
        Assert.Contains("800", body); // price renders
    }

    [Fact]
    public async Task Get_Memberships_WithoutClubId_RendersInstructions()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/Memberships");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Оберіть клуб", body);
    }
}
```

- [ ] **Step 3: Run — confirm 2 failing**

- [ ] **Step 4: PageModel**

`CoreX/Pages/Memberships/Index.cshtml.cs`:

```csharp
using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Memberships;

public class IndexModel : PageModel
{
    private readonly ISubscriptionService _subscriptions;
    private readonly IClubService _clubs;

    public IndexModel(ISubscriptionService subscriptions, IClubService clubs)
    {
        _subscriptions = subscriptions;
        _clubs = clubs;
    }

    [BindProperty(SupportsGet = true)]
    public Guid? ClubId { get; set; }

    public ClubResponseDto? Club { get; private set; }
    public IReadOnlyList<SubscriptionResponseDto> Subscriptions { get; private set; } = Array.Empty<SubscriptionResponseDto>();

    public async Task OnGetAsync()
    {
        if (ClubId is null) return;

        Club = await _clubs.GetByIdAsync(ClubId.Value);
        if (Club is null) return;

        var all = await _subscriptions.GetByClubIdAsync(ClubId.Value);
        Subscriptions = all.Where(s => s.IsActive).ToList();
    }
}
```

- [ ] **Step 5: View**

`CoreX/Pages/Memberships/Index.cshtml`:

```cshtml
@page
@model CoreX.Pages.Memberships.IndexModel
@{
    ViewData["Title"] = "Абонементи";
}

<section class="max-w-5xl mx-auto px-4 py-12 md:py-16">
    @if (Model.ClubId is null)
    {
        <h1 class="text-3xl md:text-4xl font-black uppercase tracking-tight">Абонементи</h1>
        <p class="mt-4 text-ink-500">Оберіть клуб, щоб переглянути доступні абонементи.</p>
        <a href="/Clubs" class="btn-brand mt-6 inline-flex">До списку клубів →</a>
    }
    else if (Model.Club is null)
    {
        <h1 class="text-3xl md:text-4xl font-black uppercase tracking-tight">Клуб не знайдено</h1>
        <a href="/Clubs" class="btn-brand mt-6 inline-flex">До списку клубів</a>
    }
    else
    {
        <p class="text-xs font-semibold tracking-[0.2em] uppercase text-brand-500">@Model.Club.City · @Model.Club.Name</p>
        <h1 class="mt-2 text-3xl md:text-4xl font-black uppercase tracking-tight">Абонементи</h1>

        @if (Model.Subscriptions.Count == 0)
        {
            <p class="mt-8 text-ink-500">Поки немає активних абонементів.</p>
        }
        else
        {
            <ul class="mt-10 grid grid-cols-1 md:grid-cols-2 gap-6">
            @foreach (var s in Model.Subscriptions)
            {
                <li class="rounded-card border border-ink-200 bg-white p-6">
                    <h2 class="text-xl font-bold text-ink-900">@s.Title</h2>
                    @if (!string.IsNullOrWhiteSpace(s.Description))
                    {
                        <p class="mt-2 text-sm text-ink-500">@s.Description</p>
                    }
                    <p class="mt-4 text-3xl font-black text-brand-500">@s.Price.ToString("0") ₴</p>
                    <p class="mt-1 text-xs uppercase tracking-wider text-ink-500">@s.DurationDays днів</p>
                    <a asp-page="/Memberships/Book" asp-route-subId="@s.Id" class="btn-brand mt-6 inline-flex">Забронювати →</a>
                </li>
            }
            </ul>
        }
    }
</section>
```

- [ ] **Step 6: Run tests — confirm 2 passing**

- [ ] **Step 7: Commit**

```bash
git add CoreX.UI.Tests/TestSupport/SeedData.cs CoreX/Pages/Memberships/Index.cshtml CoreX/Pages/Memberships/Index.cshtml.cs CoreX.UI.Tests/Pages/Memberships/IndexTests.cs
git commit -m "Add /Memberships?clubId catalog + seed subscriptions + TDD"
```

---

## Task 3 — `/Memberships/{subId}/Book` form + POST handler (TDD)

**Files:**
- Create: `CoreX/Pages/Memberships/Models/BookingInput.cs`
- Create: `CoreX/Pages/Memberships/Book.cshtml`
- Create: `CoreX/Pages/Memberships/Book.cshtml.cs`
- Create: `CoreX/Pages/Memberships/Confirmed.cshtml`
- Create: `CoreX/Pages/Memberships/Confirmed.cshtml.cs`
- Test: `CoreX.UI.Tests/Pages/Memberships/BookTests.cs`

The form is the same for everyone. If `User.Identity.IsAuthenticated`, GET pre-fills `ContactFullName` from `ApplicationUser.FullName` and `ContactEmail` from `User.Identity.Name` (which is the email per the Identity setup). Phone is always empty (no profile field).

- [ ] **Step 1: Write the failing tests**

`CoreX.UI.Tests/Pages/Memberships/BookTests.cs`:

```csharp
using System.Net;
using CoreX.Application.ServiceInterfaces;
using CoreX.UI.Tests.TestSupport;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CoreX.UI.Tests.Pages.Memberships;

public class BookTests : IClassFixture<CoreXFactory>
{
    private readonly CoreXFactory _factory;
    public BookTests(CoreXFactory factory) => _factory = factory;

    private async Task<Guid> SubscriptionIdAsync(Guid clubId)
    {
        using var scope = _factory.Services.CreateScope();
        var subs = scope.ServiceProvider.GetRequiredService<ISubscriptionService>();
        var list = await subs.GetByClubIdAsync(clubId);
        return list[0].Id;
    }

    [Fact]
    public async Task Get_Book_AnonymousShowsEmptyForm()
    {
        var clubs = await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var subId = await SubscriptionIdAsync(clubs[0].Id);
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/Memberships/{subId}/Book");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("name=\"Input.ContactFullName\"", body);
        Assert.Contains("name=\"Input.ContactEmail\"", body);
        Assert.Contains("name=\"Input.ContactPhone\"", body);
        Assert.Contains("__RequestVerificationToken", body);
        Assert.Contains("Місячний", body); // subscription title surfaced on form
    }

    [Fact]
    public async Task Post_Book_Anonymous_CreatesBooking_AndRedirectsToConfirmed()
    {
        var clubs = await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var subId = await SubscriptionIdAsync(clubs[0].Id);

        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, $"/Memberships/{subId}/Book");
        var post = AntiforgeryClient.BuildPost(
            $"/Memberships/{subId}/Book",
            new Dictionary<string, string>
            {
                ["Input.ContactFullName"] = "Анонім Тест",
                ["Input.ContactEmail"] = "anon@test",
                ["Input.ContactPhone"] = "+380501234567",
            },
            token, afCookie);

        var response = await client.SendAsync(post);

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.StartsWith("/Memberships/Confirmed", response.Headers.Location?.AbsolutePath);
    }

    [Fact]
    public async Task Post_Book_Authenticated_CreatesBookingWithUserId()
    {
        var clubs = await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var subId = await SubscriptionIdAsync(clubs[0].Id);

        var email = $"booker-{Guid.NewGuid():N}@test";
        await TestUsers.CreateAsync(_factory, email, role: "User", fullName: "Тарас Шевченко");
        var client = await TestUsers.SignedInClientAsync(_factory, email);

        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, $"/Memberships/{subId}/Book");
        var post = AntiforgeryClient.BuildPost(
            $"/Memberships/{subId}/Book",
            new Dictionary<string, string>
            {
                ["Input.ContactFullName"] = "Тарас Шевченко",
                ["Input.ContactEmail"] = email,
                ["Input.ContactPhone"] = "+380501234567",
            },
            token, afCookie);

        var response = await client.SendAsync(post);

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.StartsWith("/Memberships/Confirmed", response.Headers.Location?.AbsolutePath);
    }

    [Fact]
    public async Task Post_Book_WithMissingPhone_ReturnsForm_WithError()
    {
        var clubs = await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var subId = await SubscriptionIdAsync(clubs[0].Id);

        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, $"/Memberships/{subId}/Book");
        var post = AntiforgeryClient.BuildPost(
            $"/Memberships/{subId}/Book",
            new Dictionary<string, string>
            {
                ["Input.ContactFullName"] = "Анонім",
                ["Input.ContactEmail"] = "a@b",
                ["Input.ContactPhone"] = "",
            },
            token, afCookie);

        var response = await client.SendAsync(post);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Введіть телефон", body);
    }
}
```

- [ ] **Step 2: Run — confirm 4 failing**

- [ ] **Step 3: Input model**

`CoreX/Pages/Memberships/Models/BookingInput.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace CoreX.Pages.Memberships.Models;

public class BookingInput
{
    [Required(ErrorMessage = "Введіть повне ім'я.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Ім'я має містити від 3 до 100 символів.")]
    public string ContactFullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть електронну пошту.")]
    [EmailAddress(ErrorMessage = "Введіть коректну електронну адресу.")]
    public string ContactEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть телефон.")]
    [Phone(ErrorMessage = "Введіть коректний номер телефону.")]
    public string ContactPhone { get; set; } = string.Empty;
}
```

- [ ] **Step 4: Book PageModel**

`CoreX/Pages/Memberships/Book.cshtml.cs`:

```csharp
using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using CoreX.Domain.IdentityEntities;
using CoreX.Pages.Memberships.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Memberships;

public class BookModel : PageModel
{
    private readonly ISubscriptionService _subscriptions;
    private readonly IBookingService _bookings;
    private readonly UserManager<ApplicationUser> _users;

    public BookModel(
        ISubscriptionService subscriptions,
        IBookingService bookings,
        UserManager<ApplicationUser> users)
    {
        _subscriptions = subscriptions;
        _bookings = bookings;
        _users = users;
    }

    public SubscriptionResponseDto Subscription { get; private set; } = default!;

    [BindProperty]
    public BookingInput Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid subId)
    {
        var sub = await _subscriptions.GetByIdAsync(subId);
        if (sub is null) return NotFound();
        Subscription = sub;

        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _users.GetUserAsync(User);
            if (user is not null)
            {
                Input.ContactFullName = user.FullName;
                Input.ContactEmail = user.Email ?? string.Empty;
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid subId)
    {
        var sub = await _subscriptions.GetByIdAsync(subId);
        if (sub is null) return NotFound();
        Subscription = sub;

        if (!ModelState.IsValid)
            return Page();

        Guid? userId = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _users.GetUserAsync(User);
            userId = user?.Id;
        }

        Guid bookingId;
        try
        {
            bookingId = await _bookings.CreateAsync(userId, new CreateBookingDto
            {
                ClubId = sub.ClubId,
                SubscriptionId = sub.Id,
                ContactFullName = Input.ContactFullName,
                ContactEmail = Input.ContactEmail,
                ContactPhone = Input.ContactPhone,
            });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }

        return RedirectToPage("/Memberships/Confirmed", new { bookingId });
    }
}
```

- [ ] **Step 5: Book view**

`CoreX/Pages/Memberships/Book.cshtml`:

```cshtml
@page "/Memberships/{subId:guid}/Book"
@model CoreX.Pages.Memberships.BookModel
@{
    ViewData["Title"] = "Бронювання";
}

<section class="max-w-md mx-auto px-4 py-12 md:py-16">
    <p class="text-xs font-semibold tracking-[0.2em] uppercase text-brand-500">Бронювання</p>
    <h1 class="mt-2 text-3xl md:text-4xl font-black uppercase tracking-tight">@Model.Subscription.Title</h1>
    <p class="mt-2 text-ink-500">@Model.Subscription.Price.ToString("0") ₴ · @Model.Subscription.DurationDays днів</p>

    <form method="post" class="mt-8 space-y-5" novalidate>
        <div asp-validation-summary="ModelOnly" class="rounded-card border border-danger bg-danger/5 text-danger px-4 py-3 text-sm"></div>

        <div>
            <label asp-for="Input.ContactFullName" class="block text-xs font-semibold uppercase tracking-wide text-ink-800">
                Повне ім'я
            </label>
            <input asp-for="Input.ContactFullName" autocomplete="name" required
                   class="mt-1 block w-full rounded-card border-ink-200 focus:border-brand-500 focus:ring-brand-500" />
            <span asp-validation-for="Input.ContactFullName" class="mt-1 block text-sm text-danger"></span>
        </div>

        <div>
            <label asp-for="Input.ContactEmail" class="block text-xs font-semibold uppercase tracking-wide text-ink-800">
                Електронна пошта
            </label>
            <input asp-for="Input.ContactEmail" autocomplete="email" required
                   class="mt-1 block w-full rounded-card border-ink-200 focus:border-brand-500 focus:ring-brand-500" />
            <span asp-validation-for="Input.ContactEmail" class="mt-1 block text-sm text-danger"></span>
        </div>

        <div>
            <label asp-for="Input.ContactPhone" class="block text-xs font-semibold uppercase tracking-wide text-ink-800">
                Телефон
            </label>
            <input asp-for="Input.ContactPhone" type="tel" autocomplete="tel" required
                   placeholder="+380501234567"
                   class="mt-1 block w-full rounded-card border-ink-200 focus:border-brand-500 focus:ring-brand-500" />
            <span asp-validation-for="Input.ContactPhone" class="mt-1 block text-sm text-danger"></span>
        </div>

        <button type="submit" class="btn-brand w-full">Підтвердити бронювання</button>
    </form>
</section>
```

- [ ] **Step 6: Confirmed PageModel**

`CoreX/Pages/Memberships/Confirmed.cshtml.cs`:

```csharp
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Memberships;

public class ConfirmedModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid? BookingId { get; set; }

    public void OnGet() { }
}
```

- [ ] **Step 7: Confirmed view**

`CoreX/Pages/Memberships/Confirmed.cshtml`:

```cshtml
@page
@model CoreX.Pages.Memberships.ConfirmedModel
@{
    ViewData["Title"] = "Дякуємо";
}

<section class="max-w-2xl mx-auto px-4 py-16 md:py-24 text-center">
    <p class="text-xs font-semibold tracking-[0.2em] uppercase text-brand-500">Бронювання прийнято</p>
    <h1 class="mt-3 text-4xl md:text-5xl font-black uppercase tracking-tight">Дякуємо!</h1>
    <p class="mt-4 text-ink-500 max-w-lg mx-auto">
        Ми надіслали підтвердження на вашу електронну пошту. Адміністратор клубу зв'яжеться з вами найближчим часом.
    </p>
    @if (Model.BookingId is not null)
    {
        <p class="mt-6 text-xs font-mono text-ink-500">Номер бронювання: @Model.BookingId</p>
    }
    <div class="mt-10 flex gap-3 justify-center">
        <a href="/" class="btn-ghost">На головну</a>
        <a href="/Account/MyBookings" class="btn-brand">Мої бронювання</a>
    </div>
</section>
```

- [ ] **Step 8: Run tests — confirm 4 passing**

- [ ] **Step 9: Commit**

```bash
git add CoreX/Pages/Memberships/Book.cshtml CoreX/Pages/Memberships/Book.cshtml.cs CoreX/Pages/Memberships/Confirmed.cshtml CoreX/Pages/Memberships/Confirmed.cshtml.cs CoreX/Pages/Memberships/Models/BookingInput.cs CoreX.UI.Tests/Pages/Memberships/BookTests.cs
git commit -m "Add /Memberships/{subId}/Book form + Confirmed page + TDD"
```

---

## Task 4 — Memberships HTMX tab on `/Clubs/{id}` (TDD)

**Files:**
- Modify: `CoreX/Pages/Clubs/Detail.cshtml.cs` (add `ISubscriptionService` + `OnGetMembershipsAsync` + `Subscriptions` property)
- Modify: `CoreX/Pages/Clubs/Detail.cshtml` (add fourth tab button)
- Create: `CoreX/Pages/Clubs/_MembershipsList.cshtml`
- Test: append to `CoreX.UI.Tests/Pages/Clubs/DetailTests.cs`

- [ ] **Step 1: Append failing tests**

```csharp
[Fact]
public async Task GetHx_MembershipsHandler_ReturnsPartialWithSubscriptionTitle()
{
    var clubs = await SeedData.SeedDiscoveryFixtureAsync(_factory);
    var client = _factory.CreateClient();

    var response = await client.GetHxAsync($"/Clubs/{clubs[0].Id}?handler=Memberships");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var body = await response.Content.ReadAsStringAsync();
    Assert.DoesNotContain("<html", body);
    Assert.Contains("Місячний", body);
}

[Fact]
public async Task Get_MembershipsHandler_WithoutHxHeader_Returns404()
{
    var clubs = await SeedData.SeedDiscoveryFixtureAsync(_factory);
    var client = _factory.CreateClient();

    var response = await client.GetAsync($"/Clubs/{clubs[0].Id}?handler=Memberships");

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
}
```

- [ ] **Step 2: Run — confirm 2 failing**

- [ ] **Step 3: Extend `DetailModel`**

In `CoreX/Pages/Clubs/Detail.cshtml.cs`:
- Add `ISubscriptionService _subscriptions` field + ctor parameter (alongside the existing 3 services).
- Add `public IReadOnlyList<SubscriptionResponseDto> Subscriptions { get; private set; } = Array.Empty<SubscriptionResponseDto>();`.
- Add the handler at the end of the class:

```csharp
public async Task<IActionResult> OnGetMembershipsAsync(Guid id)
{
    if (!Request.IsHtmx()) return NotFound();
    var all = await _subscriptions.GetByClubIdAsync(id);
    Subscriptions = all.Where(s => s.IsActive).ToList();
    return Partial("_MembershipsList", this);
}
```

- [ ] **Step 4: Add a fourth tab button to `Detail.cshtml`**

In the `<nav>` block, after the Vacancies button, insert:

```cshtml
<button type="button" hx-get="/Clubs/@Model.Club.Id?handler=Memberships"
        hx-target="#tab-content" hx-swap="innerHTML"
        class="px-4 py-3 hover:text-brand-500">Абонементи</button>
```

- [ ] **Step 5: Create `_MembershipsList.cshtml`**

```cshtml
@model CoreX.Pages.Clubs.DetailModel

@if (Model.Subscriptions.Count == 0)
{
    <p class="text-ink-500">Активних абонементів немає.</p>
}
else
{
    <ul class="grid grid-cols-1 md:grid-cols-2 gap-4">
    @foreach (var s in Model.Subscriptions)
    {
        <li class="rounded-card border border-ink-200 p-4">
            <p class="font-bold text-ink-900">@s.Title</p>
            <p class="text-sm text-ink-500">@s.Price.ToString("0") ₴ · @s.DurationDays днів</p>
            <a asp-page="/Memberships/Book" asp-route-subId="@s.Id" class="btn-brand mt-3 inline-flex text-xs">Забронювати →</a>
        </li>
    }
    </ul>
}
```

- [ ] **Step 6: Run tests — confirm 2 passing**

- [ ] **Step 7: Commit**

```bash
git add CoreX/Pages/Clubs/Detail.cshtml CoreX/Pages/Clubs/Detail.cshtml.cs CoreX/Pages/Clubs/_MembershipsList.cshtml CoreX.UI.Tests/Pages/Clubs/DetailTests.cs
git commit -m "Add Memberships HTMX tab to /Clubs/{id}"
```

---

## Task 5 — Populated `/Account/MyBookings` state (TDD)

**Files:**
- Modify: `CoreX.UI.Tests/Pages/Account/MyBookingsTests.cs` — add one populated test

Now that bookings can be created, exercise the path: book a subscription as an authenticated user, then load `/Account/MyBookings` and confirm the row shows up.

- [ ] **Step 1: Append the test**

```csharp
[Fact]
public async Task Get_MyBookings_AuthenticatedWithBookings_ShowsBookingRow()
{
    var clubs = await SeedData.SeedDiscoveryFixtureAsync(_factory);

    // Resolve a subscription
    Guid subId;
    using (var scope = _factory.Services.CreateScope())
    {
        var subs = scope.ServiceProvider.GetRequiredService<CoreX.Application.ServiceInterfaces.ISubscriptionService>();
        var list = await subs.GetByClubIdAsync(clubs[0].Id);
        subId = list[0].Id;
    }

    var email = $"mb-pop-{Guid.NewGuid():N}@test";
    await TestUsers.CreateAsync(_factory, email, role: "User", fullName: "Бронер Тест");
    var client = await TestUsers.SignedInClientAsync(_factory, email);

    // POST a booking through the public form
    var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, $"/Memberships/{subId}/Book");
    var post = AntiforgeryClient.BuildPost(
        $"/Memberships/{subId}/Book",
        new Dictionary<string, string>
        {
            ["Input.ContactFullName"] = "Бронер Тест",
            ["Input.ContactEmail"] = email,
            ["Input.ContactPhone"] = "+380501234567",
        },
        token, afCookie);
    var bookResponse = await client.SendAsync(post);
    Assert.Equal(System.Net.HttpStatusCode.Found, bookResponse.StatusCode);

    // Now /Account/MyBookings should show the booking
    var response = await client.GetAsync("/Account/MyBookings");
    Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    var body = await response.Content.ReadAsStringAsync();
    Assert.Contains("Місячний", body); // subscription Title
    Assert.Contains("Energy Kyiv", body); // club Name
    Assert.DoesNotContain("Поки що бронювань немає.", body);
}
```

Add `using Microsoft.Extensions.DependencyInjection;` at the top if it isn't already.

- [ ] **Step 2: Run — confirm test passes immediately**

(The MyBookings page from Phase 1 already does the lookup; this test just verifies the end-to-end flow now that bookings can be created.)

- [ ] **Step 3: Commit**

```bash
git add CoreX.UI.Tests/Pages/Account/MyBookingsTests.cs
git commit -m "Exercise /Account/MyBookings populated state via Phase 3 booking flow"
```

---

## Task 6 — End-to-end smoke + cleanup

- [ ] **Step 1: Full build**

```bash
dotnet build CoreX.sln --nologo
```

Expected: 0 errors.

- [ ] **Step 2: Full test suite**

```bash
dotnet test CoreX.sln --nologo --no-build
```

Expected: 38 (prior) + 2 (Memberships index) + 4 (Book) + 2 (Memberships tab) + 1 (populated MyBookings) = **47 total, all passing**.

- [ ] **Step 3: Browser smoke**

Start the app:

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet run --project CoreX/CoreX.UI.csproj --no-build --no-launch-profile --urls "http://localhost:5053"
```

Walk through (dev DB is likely empty — the relevant assertions are status codes + chrome, not seeded data):

1. `/Memberships` (no clubId) → 200 with "Оберіть клуб" instruction.
2. `/Memberships?clubId={anyGuid}` → 200, "Клуб не знайдено" if dev DB doesn't have the club (or the list if it does).
3. `/Clubs/{anyClubId}` → click "Абонементи" tab in browser; HTMX swaps the tab content via the `?handler=Memberships` endpoint.
4. Register a user, navigate to `/Memberships?clubId=<id>` (with a real club row in dev DB), click "Забронювати →" on a subscription, fill the form, submit. Expect redirect to `/Memberships/Confirmed?bookingId=...`. Click "Мої бронювання" — the new booking should appear.

Stop the app.

- [ ] **Step 4: `git status` clean**

Expected: no tracked changes.

---

## Phase 3 exit checklist

- [ ] `dotnet build CoreX.sln` → 0 errors.
- [ ] `dotnet test CoreX.sln` → 47 passing.
- [ ] `Booking.UserId` is `Guid?` in the entity and via `BookingService.CreateAsync(Guid?, …)`.
- [ ] EF migration `MakeBookingUserIdNullable` exists in `CoreX.Infrastructure/Migrations/`.
- [ ] Anonymous visitor can fill out the booking form and receives the confirmation page.
- [ ] Authenticated visitor sees their booking on `/Account/MyBookings` after submission.
- [ ] `/Clubs/{id}` has 4 HTMX tabs (Trainers, Заняття, Вакансії, Абонементи), all returning partials.
- [ ] `BookingsController` at `/api/bookings` continues to require auth (no regression in the JSON API).

**Next phase:** Phase 4 — Vacancies + applications (`/Vacancies/{id}`, `/Vacancies/{id}/Apply`, email hook on submission, admin review queue is later in Phase 5 admin panel).
