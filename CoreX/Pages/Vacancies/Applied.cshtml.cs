using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Vacancies;

public class AppliedModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid? ApplicationId { get; set; }

    public void OnGet() { }
}
