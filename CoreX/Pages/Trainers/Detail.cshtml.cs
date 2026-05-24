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
