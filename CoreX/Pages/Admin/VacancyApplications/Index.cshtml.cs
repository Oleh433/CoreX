using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using CoreX.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Admin.VacancyApplications;

public class IndexModel : PageModel
{
    private readonly IVacancyApplicationService _applications;
    public IndexModel(IVacancyApplicationService applications) => _applications = applications;

    public IReadOnlyList<VacancyApplicationResponseDto> Applications { get; private set; } = Array.Empty<VacancyApplicationResponseDto>();

    public async Task OnGetAsync() => Applications = await _applications.GetAllAsync();

    public async Task<IActionResult> OnPostStatusAsync(Guid id, string status)
    {
        if (!Enum.TryParse<VacancyApplicationStatus>(status, ignoreCase: false, out var parsed))
        {
            return BadRequest();
        }

        var ok = await _applications.ChangeStatusAsync(id, new ChangeVacancyApplicationStatusDto { Status = parsed });
        if (!ok) return NotFound();

        var updated = await _applications.GetByIdAsync(id);
        if (updated is null) return NotFound();

        return Partial("_ApplicationRow", updated);
    }
}
