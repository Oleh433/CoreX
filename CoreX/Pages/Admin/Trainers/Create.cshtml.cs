using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using CoreX.Pages.Admin.Trainers.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CoreX.Pages.Admin.Trainers;

public class CreateModel : PageModel
{
    private readonly ITrainerService _trainers;
    private readonly IClubService _clubs;

    public CreateModel(ITrainerService trainers, IClubService clubs)
    {
        _trainers = trainers;
        _clubs = clubs;
    }

    [BindProperty]
    public TrainerInput Input { get; set; } = new();

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
            await _trainers.CreateAsync(new CreateTrainerDto
            {
                ClubId = Input.ClubId,
                FullName = Input.FullName,
                Specialization = Input.Specialization,
                ExperienceYears = Input.ExperienceYears,
                Bio = Input.Bio,
                Email = Input.Email,
                Phone = Input.Phone,
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
        var absoluteUrl = Url.Page("/Admin/Trainers/Index", pageHandler: null, values: null, protocol: Request.Scheme);
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
