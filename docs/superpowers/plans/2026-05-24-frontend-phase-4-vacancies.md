# Frontend Phase 4 — Vacancies + Applications Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax.

**Goal:** Ship the public job-listings flow: a `/Vacancies` board that lists active vacancies, a `/Vacancies/{id}` detail page, and a `/Vacancies/{id}/Apply` form that creates a `VacancyApplication` via `IVacancyApplicationService.ApplyAsync(...)`. Anonymous and authenticated visitors both supported. Existing `IEmailSender` fires automatically on submission. Admin queue / accept-reject UX lands in Phase 5.

**Architecture:** Razor Pages under `CoreX/Pages/Vacancies/`. PageModels call `IVacancyService` + `IVacancyApplicationService` via DI. The Phase 4 nav link "Вакансії" (already in `_Layout`) finally has a real target. No backend changes — the service signature `ApplyAsync(CreateVacancyApplicationDto, Guid? applicantId = null)` already supports both anonymous and authenticated paths.

**Tech Stack:** ASP.NET Core 8 Razor Pages · existing `ConsoleEmailSender` for the post-apply email · xUnit + `Microsoft.AspNetCore.Mvc.Testing`.

**Spec reference:** `docs/superpowers/specs/2026-05-20-frontend-design.md` — Phase 4 in §11.

---

## Scope cuts

- **No admin queue UI** — that's Phase 5 (Admin panel).
- **No `/Account/MyApplications`** for the authenticated applicant — the service has `GetByApplicantIdAsync` but there's no user-facing page in the spec. If we want one later, it's a tiny addition.
- **No file uploads** for CVs — applicants paste a URL into the `CVLink` field. Matches the existing entity design.

## Prerequisites

- Phase 3 merged. HEAD on master: `2557cd2 Exercise /Account/MyBookings populated state via Phase 3 booking flow`.
- `dotnet build CoreX.sln --nologo` → 0 errors.
- `dotnet test CoreX.sln --nologo --no-build` → 47/47 passing.

## Backend surface used by this phase (verified on master)

| Surface | Notes |
|---|---|
| `IVacancyService.GetActiveAsync()` | `Task<List<VacancyResponseDto>>` — global list of active vacancies (used by `/Vacancies` index). |
| `IVacancyService.GetByIdAsync(Guid)` | `Task<VacancyResponseDto?>` — used by `/Vacancies/{id}`. |
| `VacancyResponseDto` fields | `Id`, `ClubId`, `ClubName?`, `Title`, `Description`, `Requirements`, `Salary?`, `IsActive`, `CreatedAt`, `ApplicationDeadline?`, `ApplicationsCount`. |
| `IVacancyApplicationService.ApplyAsync(CreateVacancyApplicationDto dto, Guid? applicantId = null)` | `Task<Guid>` — creates the application + sends an email to the applicant. **Already accepts nullable `applicantId`; no backend change.** |
| `CreateVacancyApplicationDto` fields | `Guid VacancyId`, `string FullName`, `string Email`, `string Phone`, `string Experience` (all required); `string? Message`, `string? CVLink` (optional). |
| `VacancyApplication` entity | Status enum `New / Reviewed / Accepted / Rejected`. Sets `Status = New`, `CreatedAt = UtcNow` on construction. |
| `VacancyApplicationsController` | `POST /api/vacancy-applications` is `[AllowAnonymous]` — but Razor Pages calls the service directly, so no controller change. The controller reads `NameIdentifier` claim if present. |
| Email hook | `VacancyApplicationService.ApplyAsync` calls `_emailSender.SendAsync` after `SaveChangesAsync` — emails the applicant (`application.Email`) subject "Application received". Dev uses `ConsoleEmailSender` (logs to stdout). No setup required. |

## File map

**New files:**

| File | Responsibility |
|---|---|
| `CoreX/Pages/Vacancies/Index.cshtml` + `.cshtml.cs` | `/Vacancies` global list of active vacancies. |
| `CoreX/Pages/Vacancies/Detail.cshtml` + `.cshtml.cs` | `/Vacancies/{id}` single vacancy detail + "Apply" CTA. |
| `CoreX/Pages/Vacancies/Apply.cshtml` + `.cshtml.cs` | `/Vacancies/{id}/Apply` form. GET pre-fills name+email for authenticated users. POST calls `IVacancyApplicationService.ApplyAsync`. |
| `CoreX/Pages/Vacancies/Applied.cshtml` + `.cshtml.cs` | Confirmation page after successful application. |
| `CoreX/Pages/Vacancies/Models/ApplicationInput.cs` | Razor Pages input model with hardcoded UA `ErrorMessage` strings. |
| `CoreX.UI.Tests/Pages/Vacancies/IndexTests.cs` | List page tests. |
| `CoreX.UI.Tests/Pages/Vacancies/DetailTests.cs` | Vacancy detail tests (200 OK / unknown id 404). |
| `CoreX.UI.Tests/Pages/Vacancies/ApplyTests.cs` | Anonymous + authenticated apply tests + validation. |

**Out of scope for Phase 4:**

- Admin review/accept/reject UI — Phase 5.
- Applicant-facing "my applications" page.
- CV file upload (URL field only).
- Pagination / search on the vacancy board.

---

## Task 1 — `/Vacancies` index page (TDD)

**Files:**
- Create: `CoreX/Pages/Vacancies/Index.cshtml`
- Create: `CoreX/Pages/Vacancies/Index.cshtml.cs`
- Test: `CoreX.UI.Tests/Pages/Vacancies/IndexTests.cs`

The seed fixture from Phase 2 already creates 2 active vacancies (one per club). Reuse it.

- [ ] **Step 1: Write the failing test**

`CoreX.UI.Tests/Pages/Vacancies/IndexTests.cs`:

```csharp
using System.Net;
using CoreX.UI.Tests.TestSupport;
using Xunit;

namespace CoreX.UI.Tests.Pages.Vacancies;

public class IndexTests : IClassFixture<CoreXFactory>
{
    private readonly CoreXFactory _factory;
    public IndexTests(CoreXFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_Vacancies_ListsActiveOnes()
    {
        await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/Vacancies");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Тренер з йоги", body);
        Assert.Contains("Адміністратор", body);
    }
}
```

- [ ] **Step 2: Run — confirm failure**

- [ ] **Step 3: PageModel**

`CoreX/Pages/Vacancies/Index.cshtml.cs`:

```csharp
using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Vacancies;

public class IndexModel : PageModel
{
    private readonly IVacancyService _vacancies;
    public IndexModel(IVacancyService vacancies) => _vacancies = vacancies;

    public IReadOnlyList<VacancyResponseDto> Vacancies { get; private set; } = Array.Empty<VacancyResponseDto>();

    public async Task OnGetAsync() => Vacancies = await _vacancies.GetActiveAsync();
}
```

- [ ] **Step 4: View**

`CoreX/Pages/Vacancies/Index.cshtml`:

```cshtml
@page
@model CoreX.Pages.Vacancies.IndexModel
@{
    ViewData["Title"] = "Вакансії";
}

<section class="max-w-5xl mx-auto px-4 py-12 md:py-16">
    <h1 class="text-3xl md:text-4xl font-black uppercase tracking-tight">Вакансії</h1>

    @if (Model.Vacancies.Count == 0)
    {
        <p class="mt-8 text-ink-500">Поки немає відкритих вакансій.</p>
    }
    else
    {
        <ul class="mt-10 grid grid-cols-1 md:grid-cols-2 gap-6">
        @foreach (var v in Model.Vacancies)
        {
            <li class="rounded-card border border-ink-200 bg-white p-5">
                @if (!string.IsNullOrWhiteSpace(v.ClubName))
                {
                    <p class="text-xs font-semibold tracking-[0.2em] uppercase text-brand-500">@v.ClubName</p>
                }
                <h2 class="mt-2 text-lg font-bold text-ink-900">@v.Title</h2>
                @if (v.Salary is not null)
                {
                    <p class="mt-1 text-sm text-ink-500">від @v.Salary.Value.ToString("0") ₴</p>
                }
                <a asp-page="/Vacancies/Detail" asp-route-id="@v.Id" class="btn-ghost mt-4 inline-flex">Деталі →</a>
            </li>
        }
        </ul>
    }
</section>
```

- [ ] **Step 5: Run — confirm pass + commit**

```bash
git add CoreX/Pages/Vacancies/Index.cshtml CoreX/Pages/Vacancies/Index.cshtml.cs CoreX.UI.Tests/Pages/Vacancies/IndexTests.cs
git commit -m "Add /Vacancies index page + TDD"
```

Expected: 48/48 passing (47 prior + 1 new).

---

## Task 2 — `/Vacancies/{id}` detail page (TDD)

**Files:**
- Create: `CoreX/Pages/Vacancies/Detail.cshtml`
- Create: `CoreX/Pages/Vacancies/Detail.cshtml.cs`
- Test: `CoreX.UI.Tests/Pages/Vacancies/DetailTests.cs`

- [ ] **Step 1: Failing tests**

```csharp
using System.Net;
using CoreX.Application.ServiceInterfaces;
using CoreX.UI.Tests.TestSupport;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CoreX.UI.Tests.Pages.Vacancies;

public class DetailTests : IClassFixture<CoreXFactory>
{
    private readonly CoreXFactory _factory;
    public DetailTests(CoreXFactory factory) => _factory = factory;

    private async Task<Guid> FirstVacancyIdAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var vacancies = scope.ServiceProvider.GetRequiredService<IVacancyService>();
        var list = await vacancies.GetActiveAsync();
        return list[0].Id;
    }

    [Fact]
    public async Task Get_VacancyDetail_ShowsTitleAndDescription()
    {
        await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var id = await FirstVacancyIdAsync();
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/Vacancies/{id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Тренер з йоги", body);
        Assert.Contains("Подати заявку", body);
    }

    [Fact]
    public async Task Get_VacancyDetail_UnknownId_Returns404()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync($"/Vacancies/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
```

- [ ] **Step 2: Run — confirm 2 fail (or 1 fail + 1 incidentally pass from missing route)**

- [ ] **Step 3: PageModel**

```csharp
using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Vacancies;

public class DetailModel : PageModel
{
    private readonly IVacancyService _vacancies;
    public DetailModel(IVacancyService vacancies) => _vacancies = vacancies;

    public VacancyResponseDto Vacancy { get; private set; } = default!;

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var v = await _vacancies.GetByIdAsync(id);
        if (v is null || !v.IsActive) return NotFound();
        Vacancy = v;
        return Page();
    }
}
```

- [ ] **Step 4: View**

```cshtml
@page "/Vacancies/{id:guid}"
@model CoreX.Pages.Vacancies.DetailModel
@{
    ViewData["Title"] = Model.Vacancy.Title;
}

<section class="max-w-3xl mx-auto px-4 py-12 md:py-16">
    @if (!string.IsNullOrWhiteSpace(Model.Vacancy.ClubName))
    {
        <p class="text-xs font-semibold tracking-[0.2em] uppercase text-brand-500">@Model.Vacancy.ClubName</p>
    }
    <h1 class="mt-2 text-3xl md:text-4xl font-black uppercase tracking-tight">@Model.Vacancy.Title</h1>
    @if (Model.Vacancy.Salary is not null)
    {
        <p class="mt-2 text-ink-500">від @Model.Vacancy.Salary.Value.ToString("0") ₴</p>
    }

    <div class="mt-8 space-y-6 text-ink-800 leading-relaxed">
        <div>
            <h2 class="text-xs uppercase tracking-wide text-ink-500 font-semibold">Опис</h2>
            <p class="mt-2 whitespace-pre-line">@Model.Vacancy.Description</p>
        </div>
        <div>
            <h2 class="text-xs uppercase tracking-wide text-ink-500 font-semibold">Вимоги</h2>
            <p class="mt-2 whitespace-pre-line">@Model.Vacancy.Requirements</p>
        </div>
        @if (Model.Vacancy.ApplicationDeadline is not null)
        {
            <p class="text-sm text-ink-500">Дедлайн: @Model.Vacancy.ApplicationDeadline.Value.ToString("d MMMM yyyy")</p>
        }
    </div>

    <a asp-page="/Vacancies/Apply" asp-route-id="@Model.Vacancy.Id" class="btn-brand mt-10 inline-flex">Подати заявку →</a>
</section>
```

- [ ] **Step 5: Run + commit**

```bash
git add CoreX/Pages/Vacancies/Detail.cshtml CoreX/Pages/Vacancies/Detail.cshtml.cs CoreX.UI.Tests/Pages/Vacancies/DetailTests.cs
git commit -m "Add /Vacancies/{id} detail page + TDD"
```

Expected: 50/50.

---

## Task 3 — `/Vacancies/{id}/Apply` form + `Applied` confirmation (TDD)

**Files:**
- Create: `CoreX/Pages/Vacancies/Models/ApplicationInput.cs`
- Create: `CoreX/Pages/Vacancies/Apply.cshtml`
- Create: `CoreX/Pages/Vacancies/Apply.cshtml.cs`
- Create: `CoreX/Pages/Vacancies/Applied.cshtml`
- Create: `CoreX/Pages/Vacancies/Applied.cshtml.cs`
- Test: `CoreX.UI.Tests/Pages/Vacancies/ApplyTests.cs`

Anonymous and authenticated paths both go through this form. Pre-fill name + email for authenticated users.

- [ ] **Step 1: Failing tests**

```csharp
using System.Net;
using CoreX.Application.ServiceInterfaces;
using CoreX.UI.Tests.TestSupport;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CoreX.UI.Tests.Pages.Vacancies;

public class ApplyTests : IClassFixture<CoreXFactory>
{
    private readonly CoreXFactory _factory;
    public ApplyTests(CoreXFactory factory) => _factory = factory;

    private async Task<Guid> FirstVacancyIdAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var vacancies = scope.ServiceProvider.GetRequiredService<IVacancyService>();
        var list = await vacancies.GetActiveAsync();
        return list[0].Id;
    }

    [Fact]
    public async Task Get_Apply_AnonymousShowsEmptyForm()
    {
        await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var id = await FirstVacancyIdAsync();
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/Vacancies/{id}/Apply");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("name=\"Input.FullName\"", body);
        Assert.Contains("name=\"Input.Email\"", body);
        Assert.Contains("name=\"Input.Phone\"", body);
        Assert.Contains("name=\"Input.Experience\"", body);
        Assert.Contains("__RequestVerificationToken", body);
    }

    [Fact]
    public async Task Post_Apply_Anonymous_CreatesApplication_AndRedirectsToApplied()
    {
        await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var id = await FirstVacancyIdAsync();

        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, $"/Vacancies/{id}/Apply");
        var post = AntiforgeryClient.BuildPost(
            $"/Vacancies/{id}/Apply",
            new Dictionary<string, string>
            {
                ["Input.FullName"] = "Анонімний Кандидат",
                ["Input.Email"] = "candidate@test",
                ["Input.Phone"] = "+380501234567",
                ["Input.Experience"] = "5 років викладання групових занять",
                ["Input.Message"] = "",
                ["Input.CVLink"] = "",
            },
            token, afCookie);

        var response = await client.SendAsync(post);

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.StartsWith("/Vacancies/Applied", response.Headers.Location?.AbsolutePath);
    }

    [Fact]
    public async Task Post_Apply_Authenticated_RedirectsToApplied()
    {
        await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var id = await FirstVacancyIdAsync();

        var email = $"applicant-{Guid.NewGuid():N}@test";
        await TestUsers.CreateAsync(_factory, email, role: "User", fullName: "Олена Кандидат");
        var client = await TestUsers.SignedInClientAsync(_factory, email);

        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, $"/Vacancies/{id}/Apply");
        var post = AntiforgeryClient.BuildPost(
            $"/Vacancies/{id}/Apply",
            new Dictionary<string, string>
            {
                ["Input.FullName"] = "Олена Кандидат",
                ["Input.Email"] = email,
                ["Input.Phone"] = "+380507654321",
                ["Input.Experience"] = "3 роки",
                ["Input.Message"] = "",
                ["Input.CVLink"] = "",
            },
            token, afCookie);

        var response = await client.SendAsync(post);

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.StartsWith("/Vacancies/Applied", response.Headers.Location?.AbsolutePath);
    }

    [Fact]
    public async Task Post_Apply_WithMissingExperience_ReturnsForm_WithError()
    {
        await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var id = await FirstVacancyIdAsync();

        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, $"/Vacancies/{id}/Apply");
        var post = AntiforgeryClient.BuildPost(
            $"/Vacancies/{id}/Apply",
            new Dictionary<string, string>
            {
                ["Input.FullName"] = "Кандидат",
                ["Input.Email"] = "k@t",
                ["Input.Phone"] = "+380501234567",
                ["Input.Experience"] = "",
                ["Input.Message"] = "",
                ["Input.CVLink"] = "",
            },
            token, afCookie);

        var response = await client.SendAsync(post);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Опишіть досвід", body);
    }
}
```

- [ ] **Step 2: Run — confirm 4 fail**

- [ ] **Step 3: Input model**

`CoreX/Pages/Vacancies/Models/ApplicationInput.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace CoreX.Pages.Vacancies.Models;

public class ApplicationInput
{
    [Required(ErrorMessage = "Введіть повне ім'я.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "Ім'я має містити від 3 до 150 символів.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть електронну пошту.")]
    [EmailAddress(ErrorMessage = "Введіть коректну електронну адресу.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть телефон.")]
    [Phone(ErrorMessage = "Введіть коректний номер телефону.")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Опишіть досвід.")]
    [StringLength(2000, MinimumLength = 3, ErrorMessage = "Опис досвіду має містити від 3 до 2000 символів.")]
    public string Experience { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Повідомлення не може перевищувати 2000 символів.")]
    public string? Message { get; set; }

    [Url(ErrorMessage = "Введіть коректне посилання на CV.")]
    [StringLength(500)]
    public string? CVLink { get; set; }
}
```

- [ ] **Step 4: Apply PageModel**

`CoreX/Pages/Vacancies/Apply.cshtml.cs`:

```csharp
using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using CoreX.Domain.IdentityEntities;
using CoreX.Pages.Vacancies.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Vacancies;

public class ApplyModel : PageModel
{
    private readonly IVacancyService _vacancies;
    private readonly IVacancyApplicationService _applications;
    private readonly UserManager<ApplicationUser> _users;

    public ApplyModel(
        IVacancyService vacancies,
        IVacancyApplicationService applications,
        UserManager<ApplicationUser> users)
    {
        _vacancies = vacancies;
        _applications = applications;
        _users = users;
    }

    public VacancyResponseDto Vacancy { get; private set; } = default!;

    [BindProperty]
    public ApplicationInput Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var v = await _vacancies.GetByIdAsync(id);
        if (v is null || !v.IsActive) return NotFound();
        Vacancy = v;

        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _users.GetUserAsync(User);
            if (user is not null)
            {
                Input.FullName = user.FullName;
                Input.Email = user.Email ?? string.Empty;
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        var v = await _vacancies.GetByIdAsync(id);
        if (v is null || !v.IsActive) return NotFound();
        Vacancy = v;

        if (!ModelState.IsValid)
            return Page();

        Guid? applicantId = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _users.GetUserAsync(User);
            applicantId = user?.Id;
        }

        Guid applicationId;
        try
        {
            applicationId = await _applications.ApplyAsync(new CreateVacancyApplicationDto
            {
                VacancyId = id,
                FullName = Input.FullName,
                Email = Input.Email,
                Phone = Input.Phone,
                Experience = Input.Experience,
                Message = string.IsNullOrWhiteSpace(Input.Message) ? null : Input.Message,
                CVLink = string.IsNullOrWhiteSpace(Input.CVLink) ? null : Input.CVLink,
            }, applicantId);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }

        return Redirect(Url.Page("/Vacancies/Applied", new { applicationId })!);
    }
}
```

(Note: we use `Redirect(Url.Page(...))` to emit an absolute URL — `RedirectToPage` returns a relative URL and the test assertion uses `Location?.AbsolutePath`, which throws on relative URIs. Phase 3 hit the same and used the same workaround.)

- [ ] **Step 5: Apply view**

`CoreX/Pages/Vacancies/Apply.cshtml`:

```cshtml
@page "/Vacancies/{id:guid}/Apply"
@model CoreX.Pages.Vacancies.ApplyModel
@{
    ViewData["Title"] = "Подати заявку";
}

<section class="max-w-xl mx-auto px-4 py-12 md:py-16">
    <p class="text-xs font-semibold tracking-[0.2em] uppercase text-brand-500">Подати заявку</p>
    <h1 class="mt-2 text-3xl md:text-4xl font-black uppercase tracking-tight">@Model.Vacancy.Title</h1>
    @if (!string.IsNullOrWhiteSpace(Model.Vacancy.ClubName))
    {
        <p class="mt-2 text-ink-500">@Model.Vacancy.ClubName</p>
    }

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
            <label asp-for="Input.Phone" class="block text-xs font-semibold uppercase tracking-wide text-ink-800">
                Телефон
            </label>
            <input asp-for="Input.Phone" type="tel" autocomplete="tel" required
                   placeholder="+380501234567"
                   class="mt-1 block w-full rounded-card border-ink-200 focus:border-brand-500 focus:ring-brand-500" />
            <span asp-validation-for="Input.Phone" class="mt-1 block text-sm text-danger"></span>
        </div>

        <div>
            <label asp-for="Input.Experience" class="block text-xs font-semibold uppercase tracking-wide text-ink-800">
                Досвід
            </label>
            <textarea asp-for="Input.Experience" rows="3" required
                      placeholder="Коротко про ваш досвід"
                      class="mt-1 block w-full rounded-card border-ink-200 focus:border-brand-500 focus:ring-brand-500"></textarea>
            <span asp-validation-for="Input.Experience" class="mt-1 block text-sm text-danger"></span>
        </div>

        <div>
            <label asp-for="Input.Message" class="block text-xs font-semibold uppercase tracking-wide text-ink-800">
                Повідомлення (опціонально)
            </label>
            <textarea asp-for="Input.Message" rows="3"
                      class="mt-1 block w-full rounded-card border-ink-200 focus:border-brand-500 focus:ring-brand-500"></textarea>
            <span asp-validation-for="Input.Message" class="mt-1 block text-sm text-danger"></span>
        </div>

        <div>
            <label asp-for="Input.CVLink" class="block text-xs font-semibold uppercase tracking-wide text-ink-800">
                Посилання на CV (опціонально)
            </label>
            <input asp-for="Input.CVLink" type="url" placeholder="https://"
                   class="mt-1 block w-full rounded-card border-ink-200 focus:border-brand-500 focus:ring-brand-500" />
            <span asp-validation-for="Input.CVLink" class="mt-1 block text-sm text-danger"></span>
        </div>

        <button type="submit" class="btn-brand w-full">Надіслати заявку</button>
    </form>
</section>
```

- [ ] **Step 6: Applied PageModel + view**

`CoreX/Pages/Vacancies/Applied.cshtml.cs`:

```csharp
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Vacancies;

public class AppliedModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid? ApplicationId { get; set; }

    public void OnGet() { }
}
```

`CoreX/Pages/Vacancies/Applied.cshtml`:

```cshtml
@page
@model CoreX.Pages.Vacancies.AppliedModel
@{
    ViewData["Title"] = "Заявку прийнято";
}

<section class="max-w-2xl mx-auto px-4 py-16 md:py-24 text-center">
    <p class="text-xs font-semibold tracking-[0.2em] uppercase text-brand-500">Заявку прийнято</p>
    <h1 class="mt-3 text-4xl md:text-5xl font-black uppercase tracking-tight">Дякуємо!</h1>
    <p class="mt-4 text-ink-500 max-w-lg mx-auto">
        Ми надіслали підтвердження на вашу електронну пошту. Команда клубу зв'яжеться з вами найближчим часом.
    </p>
    @if (Model.ApplicationId is not null)
    {
        <p class="mt-6 text-xs font-mono text-ink-500">Номер заявки: @Model.ApplicationId</p>
    }
    <div class="mt-10 flex gap-3 justify-center">
        <a href="/" class="btn-ghost">На головну</a>
        <a href="/Vacancies" class="btn-brand">Інші вакансії</a>
    </div>
</section>
```

- [ ] **Step 7: Run all Apply tests — confirm 4 pass + run full suite**

Expected: 54/54 passing (50 prior + 4 new).

- [ ] **Step 8: Commit**

```bash
git add CoreX/Pages/Vacancies/Apply.cshtml CoreX/Pages/Vacancies/Apply.cshtml.cs CoreX/Pages/Vacancies/Applied.cshtml CoreX/Pages/Vacancies/Applied.cshtml.cs CoreX/Pages/Vacancies/Models/ApplicationInput.cs CoreX.UI.Tests/Pages/Vacancies/ApplyTests.cs
git commit -m "Add /Vacancies/{id}/Apply form + Applied confirmation + TDD"
```

---

## Task 4 — End-to-end smoke + cleanup

- [ ] **Step 1: Build**

```bash
dotnet build CoreX.sln --nologo
```

Expected: 0 errors.

- [ ] **Step 2: Full test suite**

```bash
dotnet test CoreX.sln --nologo --no-build
```

Expected: 47 (prior) + 1 (Index) + 2 (Detail) + 4 (Apply) = **54 total, all passing**.

- [ ] **Step 3: Browser smoke**

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet run --project CoreX/CoreX.UI.csproj --no-build --no-launch-profile --urls "http://localhost:5055"
```

Walk through:
1. `/Vacancies` — list page renders (empty state if dev DB has no active vacancies).
2. `/Vacancies/{unknown-guid}` → 404.
3. With a real seeded vacancy id (or use `/api/vacancies?activeOnly=true` to find one): `/Vacancies/{id}` → detail page with "Подати заявку →" button.
4. Click "Подати заявку →" → form renders.
5. Submit form → redirect to `/Vacancies/Applied?applicationId=...`. Inspect stdout for the "Application received" `ConsoleEmailSender` log line addressed to the form's email.

Stop the app.

- [ ] **Step 4: `git status` clean**

---

## Phase 4 exit checklist

- [ ] `dotnet build CoreX.sln` → 0 errors.
- [ ] `dotnet test CoreX.sln` → 54 passing.
- [ ] `/Vacancies` lists every active vacancy.
- [ ] `/Vacancies/{id}` shows the title, description, requirements, deadline (if set), and an "Apply" button.
- [ ] `/Vacancies/{id}/Apply` accepts anonymous and authenticated submissions; both redirect to `/Vacancies/Applied`.
- [ ] After submission, the `ConsoleEmailSender` (dev) logs an "Application received" email to the applicant's email.
- [ ] `/api/vacancy-applications` still POSTs from external API consumers (no controller change, unchanged auth).

**Next phase:** Phase 5 — Admin panel (the biggest phase; dashboard + CRUD for ~10 entities + booking/application review queue with HTMX row actions + Owner-only Subscriptions/Discounts/Admin user registration).
