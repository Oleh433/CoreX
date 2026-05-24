using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Clubs;

public class DetailModel : PageModel
{
    private readonly IClubService _clubs;

    public DetailModel(IClubService clubs) => _clubs = clubs;

    public ClubResponseDto Club { get; private set; } = default!;

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var club = await _clubs.GetByIdAsync(id);
        if (club is null) return NotFound();

        Club = club;
        return Page();
    }
}
