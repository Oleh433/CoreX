using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using CoreX.Pages.Admin.Clubs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Admin.Clubs;

public class EditModel : PageModel
{
    private readonly IClubService _clubs;
    public EditModel(IClubService clubs) => _clubs = clubs;

    [BindProperty]
    public ClubInput Input { get; set; } = new();

    public Guid Id { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        Id = id;
        var club = await _clubs.GetByIdAsync(id);
        if (club is null) return NotFound();

        Input = new ClubInput
        {
            Name = club.Name,
            City = club.City,
            Address = club.Address,
            Description = club.Description,
            Phone = club.Phone,
            Email = club.Email,
            WorkingHours = club.WorkingHours,
            PhotoUrl = club.PhotoUrl,
            Latitude = club.Latitude,
            Longitude = club.Longitude,
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        Id = id;
        if (!ModelState.IsValid) return Page();

        try
        {
            await _clubs.UpdateAsync(id, new UpdateClubDto
            {
                Name = Input.Name,
                City = Input.City,
                Address = Input.Address,
                Description = Input.Description,
                Phone = Input.Phone,
                Email = Input.Email,
                WorkingHours = Input.WorkingHours,
                PhotoUrl = Input.PhotoUrl,
                Latitude = Input.Latitude,
                Longitude = Input.Longitude,
            });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }

        // Emit an absolute URL — tests assert on Location?.AbsolutePath, which throws on
        // relative URIs (established Phase 3/4 workaround).
        var absoluteUrl = Url.Page("/Admin/Clubs/Index", pageHandler: null, values: null, protocol: Request.Scheme);
        return Redirect(absoluteUrl!);
    }
}
