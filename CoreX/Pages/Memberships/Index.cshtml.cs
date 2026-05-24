using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Memberships;

public class IndexModel : PageModel
{
    private readonly ISubscriptionService _subscriptions;
    private readonly IClubService _clubs;

    public IndexModel(ISubscriptionService subscriptions, IClubService clubs)
    {
        _subscriptions = subscriptions;
        _clubs = clubs;
    }

    [BindProperty(SupportsGet = true)]
    public Guid? ClubId { get; set; }

    public ClubResponseDto? Club { get; private set; }
    public IReadOnlyList<SubscriptionResponseDto> Subscriptions { get; private set; } = Array.Empty<SubscriptionResponseDto>();

    public async Task OnGetAsync()
    {
        if (ClubId is null) return;

        Club = await _clubs.GetByIdAsync(ClubId.Value);
        if (Club is null) return;

        var all = await _subscriptions.GetByClubIdAsync(ClubId.Value);
        Subscriptions = all.Where(s => s.IsActive).ToList();
    }
}
