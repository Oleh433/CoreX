using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Admin.Users;

public class RegisterAdminModel : PageModel
{
    private readonly IUserService _users;
    public RegisterAdminModel(IUserService users) => _users = users;

    [BindProperty]
    public UserRegisterRequest Input { get; set; } = new()
    {
        FullName = string.Empty,
        Email = string.Empty,
        Password = string.Empty,
        ConfirmPassword = string.Empty,
        TermsAccepted = true, // pre-checked for admin creator
    };

    [TempData]
    public string? SuccessMessage { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        try
        {
            await _users.AdminRegisterAsync(Input);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, MapError(ex.Message));
            return Page();
        }

        SuccessMessage = $"Адмін {Input.Email} створений.";
        return RedirectToPage("/Admin/Users/RegisterAdmin");
    }

    private static string MapError(string serviceMessage) => serviceMessage switch
    {
        "A user with this email already exists." => "Користувач з такою електронною адресою вже існує.",
        "User with this email already exists." => "Користувач з такою електронною адресою вже існує.",
        _ => serviceMessage,
    };
}
