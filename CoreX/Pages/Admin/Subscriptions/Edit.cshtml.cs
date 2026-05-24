using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using CoreX.Pages.Admin.Subscriptions.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Admin.Subscriptions;

public class EditModel : PageModel
{
    private readonly ISubscriptionService _subscriptions;
    private readonly IClubService _clubs;

    public EditModel(ISubscriptionService subscriptions, IClubService clubs)
    {
        _subscriptions = subscriptions;
        _clubs = clubs;
    }

    [BindProperty]
    public SubscriptionInput Input { get; set; } = new();

    public Guid Id { get; private set; }

    public string? ClubName { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        Id = id;
        var sub = await _subscriptions.GetByIdAsync(id);
        if (sub is null) return NotFound();

        var club = await _clubs.GetByIdAsync(sub.ClubId);
        ClubName = club?.Name;
        Input = new SubscriptionInput
        {
            ClubId = sub.ClubId,
            Title = sub.Title,
            Price = sub.Price,
            DurationDays = sub.DurationDays,
            VisitsLimit = sub.VisitsLimit,
            Description = sub.Description,
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        Id = id;
        // ClubId isn't editable on this page, so its [Required] constraint can fail with a
        // bogus binding. Suppress that field's validation — we don't read it for the update.
        ModelState.Remove(nameof(Input) + "." + nameof(Input.ClubId));

        if (!ModelState.IsValid)
        {
            var current = await _subscriptions.GetByIdAsync(id);
            if (current is not null)
            {
                var club = await _clubs.GetByIdAsync(current.ClubId);
                ClubName = club?.Name;
            }
            return Page();
        }

        try
        {
            await _subscriptions.UpdateAsync(id, new UpdateSubscriptionDto
            {
                Title = Input.Title,
                Price = Input.Price,
                DurationDays = Input.DurationDays,
                VisitsLimit = Input.VisitsLimit,
                Description = Input.Description,
            });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var current = await _subscriptions.GetByIdAsync(id);
            if (current is not null)
            {
                var club = await _clubs.GetByIdAsync(current.ClubId);
                ClubName = club?.Name;
            }
            return Page();
        }

        // Emit an absolute URL — tests assert on Location?.AbsolutePath, which throws on
        // relative URIs (established Phase 3/4 workaround).
        var absoluteUrl = Url.Page("/Admin/Subscriptions/Index", pageHandler: null, values: null, protocol: Request.Scheme);
        return Redirect(absoluteUrl!);
    }
}
