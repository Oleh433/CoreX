using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using CoreX.Pages.Admin.Clubs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Admin.Clubs;

public class CreateModel : PageModel
{
    private readonly IClubService _clubs;
    public CreateModel(IClubService clubs) => _clubs = clubs;

    [BindProperty]
    public ClubInput Input { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        try
        {
            await _clubs.CreateAsync(new CreateClubDto
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
