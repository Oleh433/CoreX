using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Trainers;

public class IndexModel : PageModel
{
    private readonly ITrainerService _trainers;
    public IndexModel(ITrainerService trainers) => _trainers = trainers;

    public IReadOnlyList<TrainerResponseDto> Trainers { get; private set; } = Array.Empty<TrainerResponseDto>();

    public async Task OnGetAsync() => Trainers = await _trainers.GetAllAsync();
}
