using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using CoreX.Pages.Admin.GroupClasses.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CoreX.Pages.Admin.GroupClasses;

public class CreateModel : PageModel
{
    private readonly IGroupClassService _classes;
    private readonly IClubService _clubs;
    private readonly ITrainerService _trainers;

    public CreateModel(IGroupClassService classes, IClubService clubs, ITrainerService trainers)
    {
        _classes = classes;
        _clubs = clubs;
        _trainers = trainers;
    }

    [BindProperty]
    public GroupClassInput Input { get; set; } = new();

    public IReadOnlyList<SelectListItem> ClubOptions { get; private set; } = Array.Empty<SelectListItem>();

    public IReadOnlyList<SelectListItem> TrainerOptions { get; private set; } = Array.Empty<SelectListItem>();

    public async Task OnGetAsync(Guid? clubId)
    {
        if (clubId is { } preselected)
        {
            Input.ClubId = preselected;
        }
        await LoadOptionsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadOptionsAsync();
            return Page();
        }

        try
        {
            await _classes.CreateAsync(new CreateGroupClassDto
            {
                ClubId = Input.ClubId,
                TrainerId = Input.TrainerId,
                Type = Input.Type,
                Description = Input.Description,
                Audience = Input.Audience,
                StartTime = Input.StartTime,
                DurationMinutes = Input.DurationMinutes,
                Capacity = Input.Capacity,
                Price = Input.Price,
            });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadOptionsAsync();
            return Page();
        }

        // Emit an absolute URL — tests assert on Location?.AbsolutePath, which throws on
        // relative URIs (established Phase 3/4 workaround).
        var absoluteUrl = Url.Page(
            "/Admin/GroupClasses/Index",
            pageHandler: null,
            values: new { clubId = Input.ClubId },
            protocol: Request.Scheme);
        return Redirect(absoluteUrl!);
    }

    private async Task LoadOptionsAsync()
    {
        var clubs = await _clubs.GetAllAsync();
        ClubOptions = clubs
            .Select(c => new SelectListItem(c.Name, c.Id.ToString()))
            .ToList();

        var trainers = await _trainers.GetAllAsync();
        TrainerOptions = trainers
            .Select(t => new SelectListItem($"{t.FullName} ({t.ClubName})", t.Id.ToString()))
            .ToList();
    }
}
