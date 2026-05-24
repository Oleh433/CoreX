using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using CoreX.Pages.Account.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Account;

public class LoginModel : PageModel
{
    private readonly IUserService _users;

    public LoginModel(IUserService users) => _users = users;

    [BindProperty]
    public LoginInput Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    public void OnGet(string? returnUrl = null) => ReturnUrl = returnUrl;

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;

        if (!ModelState.IsValid)
            return Page();

        try
        {
            await _users.SignInAsync(new UserSignInRequest
            {
                Email = Input.Email,
                Password = Input.Password,
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            ModelState.AddModelError(string.Empty, MapSignInError(ex.Message));
            return Page();
        }

        return LocalRedirect(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);
    }

    private static string MapSignInError(string serviceMessage) => serviceMessage switch
    {
        "Account is temporarily locked due to multiple failed sign-in attempts."
            => "Акаунт тимчасово заблоковано. Спробуйте за 15 хвилин.",
        "Account is not allowed to sign in."
            => "Невірна електронна адреса або пароль.",
        _ => "Невірна електронна адреса або пароль.",
    };
}
