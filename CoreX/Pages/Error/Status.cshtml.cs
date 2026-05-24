using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Error;

[AllowAnonymous]
public class StatusModel : PageModel
{
    public int Code { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    public IActionResult OnGet(int code)
    {
        Code = code;
        (Title, Description) = code switch
        {
            404 => ("Сторінку не знайдено", "Можливо, ви помилились у посиланні, або сторінку було видалено."),
            403 => ("Доступ заборонено", "У вас немає прав для перегляду цієї сторінки."),
            _ => ($"Помилка {code}", "Сталася непередбачувана помилка."),
        };
        Response.StatusCode = code;
        return Page();
    }
}
