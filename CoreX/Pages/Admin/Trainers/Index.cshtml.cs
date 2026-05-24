using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Admin.Trainers;

public class IndexModel : PageModel
{
    private readonly ITrainerService _trainers;
    public IndexModel(ITrainerService trainers) => _trainers = trainers;

    public IReadOnlyList<TrainerResponseDto> Trainers { get; private set; } = Array.Empty<TrainerResponseDto>();

    public async Task OnGetAsync() => Trainers = await _trainers.GetAllAsync();

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        await _trainers.DeleteAsync(id);
        return Content(string.Empty, "text/html"); // HTMX swaps the row to nothing
    }
}
