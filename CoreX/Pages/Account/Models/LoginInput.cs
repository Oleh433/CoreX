using System.ComponentModel.DataAnnotations;

namespace CoreX.Pages.Account.Models;

public class LoginInput
{
    [Required(ErrorMessage = "Введіть електронну пошту.")]
    [EmailAddress(ErrorMessage = "Введіть коректну електронну адресу.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть пароль.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
