using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using CoreX.Pages.Admin.Vacancies.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Admin.Vacancies;

public class EditModel : PageModel
{
    private readonly IVacancyService _vacancies;
    public EditModel(IVacancyService vacancies) => _vacancies = vacancies;

    [BindProperty]
    public VacancyInput Input { get; set; } = new();

    public Guid Id { get; private set; }

    public string? ClubName { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        Id = id;
        var vacancy = await _vacancies.GetByIdAsync(id);
        if (vacancy is null) return NotFound();

        ClubName = vacancy.ClubName;
        Input = new VacancyInput
        {
            ClubId = vacancy.ClubId,
            Title = vacancy.Title,
            Description = vacancy.Description,
            Requirements = vacancy.Requirements,
            Salary = vacancy.Salary,
            ApplicationDeadline = vacancy.ApplicationDeadline,
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        Id = id;
        // ClubId isn't editable on this page, so its [Required] constraint can fail with a
        // bogus binding. Suppress that field's validation — we don't read it for the update.
        ModelState.Remove(nameof(Input) + "." + nameof(Input.ClubId));

        if (!ModelState.IsValid)
        {
            var current = await _vacancies.GetByIdAsync(id);
            ClubName = current?.ClubName;
            return Page();
        }

        try
        {
            await _vacancies.UpdateAsync(id, new UpdateVacancyDto
            {
                Title = Input.Title,
                Description = Input.Description,
                Requirements = Input.Requirements,
                Salary = Input.Salary,
                ApplicationDeadline = Input.ApplicationDeadline,
            });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var current = await _vacancies.GetByIdAsync(id);
            ClubName = current?.ClubName;
            return Page();
        }

        // Emit an absolute URL — tests assert on Location?.AbsolutePath, which throws on
        // relative URIs (established Phase 3/4 workaround).
        var absoluteUrl = Url.Page("/Admin/Vacancies/Index", pageHandler: null, values: null, protocol: Request.Scheme);
        return Redirect(absoluteUrl!);
    }
}
