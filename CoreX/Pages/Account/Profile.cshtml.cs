using CoreX.Domain.IdentityEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Account;

public class ProfileModel : PageModel
{
    private readonly UserManager<ApplicationUser> _users;

    public ProfileModel(UserManager<ApplicationUser> users) => _users = users;

    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _users.GetUserAsync(User);
        if (user is null)
            return RedirectToPage("/Account/Login");

        FullName = user.FullName;
        Email = user.Email ?? string.Empty;
        return Page();
    }
}
