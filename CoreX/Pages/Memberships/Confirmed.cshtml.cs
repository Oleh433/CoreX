using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Memberships;

public class ConfirmedModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid? BookingId { get; set; }

    public void OnGet() { }
}
