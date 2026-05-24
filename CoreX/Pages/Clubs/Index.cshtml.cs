using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Clubs;

public class IndexModel : PageModel
{
    private readonly IClubService _clubs;

    public IndexModel(IClubService clubs) => _clubs = clubs;

    [BindProperty(SupportsGet = true)]
    public string? City { get; set; }

    public IReadOnlyList<ClubResponseDto> Clubs { get; private set; } = Array.Empty<ClubResponseDto>();

    public async Task OnGetAsync()
    {
        Clubs = string.IsNullOrWhiteSpace(City)
            ? await _clubs.GetAllAsync()
            : await _clubs.GetByCityAsync(City);
    }
}
