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
