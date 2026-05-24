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

        // Emit an absolute URL — the test asserts on Location?.AbsolutePath, which
        // throws on relative URIs (Phase 3 used the same workaround).
        var absoluteUrl = Url.Page("/Vacancies/Applied", pageHandler: null, values: new { applicationId }, protocol: Request.Scheme);
        return Redirect(absoluteUrl!);
    }
}
