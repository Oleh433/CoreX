using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Admin.Subscriptions;

public class IndexModel : PageModel
{
    private readonly ISubscriptionService _subscriptions;
    private readonly IClubService _clubs;

    public IndexModel(ISubscriptionService subscriptions, IClubService clubs)
    {
        _subscriptions = subscriptions;
        _clubs = clubs;
    }

    public IReadOnlyList<SubscriptionRow> Rows { get; private set; } = Array.Empty<SubscriptionRow>();

    public async Task OnGetAsync()
    {
        var subs = await _subscriptions.GetAllAsync();
        var clubs = await _clubs.GetAllAsync();
        var clubNameById = clubs.ToDictionary(c => c.Id, c => c.Name);
        Rows = subs
            .Select(s => new SubscriptionRow(s, clubNameById.GetValueOrDefault(s.ClubId, "—")))
            .ToList();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        await _subscriptions.DeleteAsync(id);
        return Content(string.Empty, "text/html"); // HTMX swaps the row to nothing
    }

    public async Task<IActionResult> OnPostActivateAsync(Guid id)
    {
        await _subscriptions.ActivateAsync(id);
        return await BuildRowPartialAsync(id);
    }

    public async Task<IActionResult> OnPostDeactivateAsync(Guid id)
    {
        await _subscriptions.DeactivateAsync(id);
        return await BuildRowPartialAsync(id);
    }

    private async Task<IActionResult> BuildRowPartialAsync(Guid id)
    {
        var updated = await _subscriptions.GetByIdAsync(id);
        if (updated is null) return NotFound();
        var club = await _clubs.GetByIdAsync(updated.ClubId);
        return Partial("_SubscriptionRow", new SubscriptionRow(updated, club?.Name ?? "—"));
    }

    public sealed record SubscriptionRow(SubscriptionResponseDto Subscription, string ClubName);
}
