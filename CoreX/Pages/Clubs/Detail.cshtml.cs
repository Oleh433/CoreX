using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using CoreX.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Clubs;

public class DetailModel : PageModel
{
    private readonly IClubService _clubs;
    private readonly ITrainerService _trainers;
    private readonly IGroupClassService _groupClasses;
    private readonly IVacancyService _vacancies;
    private readonly ISubscriptionService _subscriptions;

    public DetailModel(
        IClubService clubs,
        ITrainerService trainers,
        IGroupClassService groupClasses,
        IVacancyService vacancies,
        ISubscriptionService subscriptions)
    {
        _clubs = clubs;
        _trainers = trainers;
        _groupClasses = groupClasses;
        _vacancies = vacancies;
        _subscriptions = subscriptions;
    }

    public ClubResponseDto Club { get; private set; } = default!;
    public IReadOnlyList<TrainerResponseDto> Trainers { get; private set; } = Array.Empty<TrainerResponseDto>();
    public IReadOnlyList<GroupClassResponseDto> GroupClasses { get; private set; } = Array.Empty<GroupClassResponseDto>();
    public IReadOnlyList<VacancyResponseDto> Vacancies { get; private set; } = Array.Empty<VacancyResponseDto>();
    public IReadOnlyList<SubscriptionResponseDto> Subscriptions { get; private set; } = Array.Empty<SubscriptionResponseDto>();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var club = await _clubs.GetByIdAsync(id);
        if (club is null) return NotFound();
        Club = club;
        return Page();
    }

    public async Task<IActionResult> OnGetTrainersAsync(Guid id)
    {
        if (!Request.IsHtmx()) return NotFound();
        Trainers = await _trainers.GetByClubIdAsync(id);
        return Partial("_TrainersList", this);
    }

    public async Task<IActionResult> OnGetGroupClassesAsync(Guid id)
    {
        if (!Request.IsHtmx()) return NotFound();
        GroupClasses = await _groupClasses.GetByClubIdAsync(id);
        return Partial("_GroupClassesList", this);
    }

    public async Task<IActionResult> OnGetVacanciesAsync(Guid id)
    {
        if (!Request.IsHtmx()) return NotFound();
        Vacancies = await _vacancies.GetByClubIdAsync(id);
        return Partial("_VacanciesList", this);
    }

    public async Task<IActionResult> OnGetMembershipsAsync(Guid id)
    {
        if (!Request.IsHtmx()) return NotFound();
        var all = await _subscriptions.GetByClubIdAsync(id);
        Subscriptions = all.Where(s => s.IsActive).ToList();
        return Partial("_MembershipsList", this);
    }
}
