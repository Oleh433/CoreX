using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Admin.GroupClasses;

public class IndexModel : PageModel
{
    private readonly IGroupClassService _classes;
    private readonly IClubService _clubs;

    public IndexModel(IGroupClassService classes, IClubService clubs)
    {
        _classes = classes;
        _clubs = clubs;
    }

    [BindProperty(SupportsGet = true)]
    public Guid? ClubId { get; set; }

    public IReadOnlyList<ClubResponseDto> Clubs { get; private set; } = Array.Empty<ClubResponseDto>();

    public ClubResponseDto? SelectedClub { get; private set; }

    public IReadOnlyList<GroupClassResponseDto> Classes { get; private set; } = Array.Empty<GroupClassResponseDto>();

    public async Task OnGetAsync()
    {
        Clubs = await _clubs.GetAllAsync();

        if (ClubId is null)
        {
            return;
        }

        SelectedClub = await _clubs.GetByIdAsync(ClubId.Value);
        if (SelectedClub is null)
        {
            return;
        }

        Classes = await _classes.GetByClubIdAsync(ClubId.Value);
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        await _classes.DeleteAsync(id);
        return Content(string.Empty, "text/html"); // HTMX swaps the row to nothing
    }
}
