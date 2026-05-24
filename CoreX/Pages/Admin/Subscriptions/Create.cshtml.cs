using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using CoreX.Pages.Admin.Subscriptions.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CoreX.Pages.Admin.Subscriptions;

public class CreateModel : PageModel
{
    private readonly ISubscriptionService _subscriptions;
    private readonly IClubService _clubs;

    public CreateModel(ISubscriptionService subscriptions, IClubService clubs)
    {
        _subscriptions = subscriptions;
        _clubs = clubs;
    }

    [BindProperty]
    public SubscriptionInput Input { get; set; } = new();

    public IReadOnlyList<SelectListItem> ClubOptions { get; private set; } = Array.Empty<SelectListItem>();

    public async Task OnGetAsync() => await LoadClubsAsync();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadClubsAsync();
            return Page();
        }

        try
        {
            await _subscriptions.CreateAsync(new CreateSubscriptionDto
            {
                ClubId = Input.ClubId,
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
            await LoadClubsAsync();
            return Page();
        }

        // Emit an absolute URL — tests assert on Location?.AbsolutePath, which throws on
        // relative URIs (established Phase 3/4 workaround).
        var absoluteUrl = Url.Page("/Admin/Subscriptions/Index", pageHandler: null, values: null, protocol: Request.Scheme);
        return Redirect(absoluteUrl!);
    }

    private async Task LoadClubsAsync()
    {
        var clubs = await _clubs.GetAllAsync();
        ClubOptions = clubs
            .Select(c => new SelectListItem(c.Name, c.Id.ToString()))
            .ToList();
    }
}
