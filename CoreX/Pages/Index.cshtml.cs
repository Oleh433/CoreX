using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages;

public class IndexModel : PageModel
{
    private readonly IClubService _clubs;
    public IndexModel(IClubService clubs) => _clubs = clubs;

    public IReadOnlyList<ClubResponseDto> Featured { get; private set; } = Array.Empty<ClubResponseDto>();

    public async Task OnGetAsync()
    {
        var all = await _clubs.GetAllAsync();
        Featured = all.Take(6).ToList();
    }
}
