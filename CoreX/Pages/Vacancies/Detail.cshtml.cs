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
