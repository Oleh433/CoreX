using CoreX.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Account;

public class LogoutModel : PageModel
{
    private readonly IUserService _users;

    public LogoutModel(IUserService users) => _users = users;

    public IActionResult OnGet() => RedirectToPage("/Account/Login");

    public async Task<IActionResult> OnPostAsync()
    {
        await _users.SignOutAsync();
        return LocalRedirect("/");
    }
}
