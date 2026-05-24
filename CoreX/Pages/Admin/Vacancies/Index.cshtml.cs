using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Admin.Vacancies;

public class IndexModel : PageModel
{
    private readonly IVacancyService _vacancies;
    public IndexModel(IVacancyService vacancies) => _vacancies = vacancies;

    public IReadOnlyList<VacancyResponseDto> Vacancies { get; private set; } = Array.Empty<VacancyResponseDto>();

    public async Task OnGetAsync() => Vacancies = await _vacancies.GetAllAsync();

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        await _vacancies.DeleteAsync(id);
        return Content(string.Empty, "text/html"); // HTMX swaps the row to nothing
    }

    public async Task<IActionResult> OnPostActivateAsync(Guid id)
    {
        await _vacancies.ActivateAsync(id);
        var updated = await _vacancies.GetByIdAsync(id);
        if (updated is null) return NotFound();
        return Partial("_VacancyRow", updated);
    }

    public async Task<IActionResult> OnPostDeactivateAsync(Guid id)
    {
        await _vacancies.DeactivateAsync(id);
        var updated = await _vacancies.GetByIdAsync(id);
        if (updated is null) return NotFound();
        return Partial("_VacancyRow", updated);
    }
}
