using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using CoreX.Pages.Account.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly IUserService _users;

    public RegisterModel(IUserService users) => _users = users;

    [BindProperty]
    public RegisterInput Input { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        try
        {
            await _users.UserRegisterAsync(new UserRegisterRequest
            {
                FullName = Input.FullName,
                Email = Input.Email,
                Password = Input.Password,
                ConfirmPassword = Input.ConfirmPassword,
                TermsAccepted = Input.TermsAccepted,
            });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, MapRegisterError(ex.Message));
            return Page();
        }

        try
        {
            await _users.SignInAsync(new UserSignInRequest
            {
                Email = Input.Email,
                Password = Input.Password,
            });
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToPage("/Account/Login");
        }

        return LocalRedirect("/");
    }

    private static string MapRegisterError(string serviceMessage) => serviceMessage switch
    {
        // Plan-spec strings
        "A user with this email already exists." => "Користувач з такою електронною адресою вже існує.",
        "Passwords do not match." => "Паролі не співпадають.",
        "You must accept the terms of use." => "Потрібно прийняти умови використання.",
        // Actual UserService.RegisterAsync strings
        "User with this email already exists." => "Користувач з такою електронною адресою вже існує.",
        "Password and ConfirmPassword do not match." => "Паролі не співпадають.",
        "Terms of use must be accepted." => "Потрібно прийняти умови використання.",
        _ => "Не вдалося створити акаунт. Спробуйте ще раз.",
    };
}
