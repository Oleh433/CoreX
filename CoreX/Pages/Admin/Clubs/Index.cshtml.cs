using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Admin.Clubs;

public class IndexModel : PageModel
{
    private readonly IClubService _clubs;
    public IndexModel(IClubService clubs) => _clubs = clubs;

    public IReadOnlyList<ClubResponseDto> Clubs { get; private set; } = Array.Empty<ClubResponseDto>();

    public async Task OnGetAsync() => Clubs = await _clubs.GetAllAsync();

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        await _clubs.DeleteAsync(id);
        return Content(string.Empty, "text/html"); // HTMX swaps the row to nothing
    }
}
