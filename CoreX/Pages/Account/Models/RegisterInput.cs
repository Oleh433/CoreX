using System.ComponentModel.DataAnnotations;

namespace CoreX.Pages.Account.Models;

public class RegisterInput
{
    [Required(ErrorMessage = "Введіть повне ім'я.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Ім'я має містити від 3 до 100 символів.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть електронну пошту.")]
    [EmailAddress(ErrorMessage = "Введіть коректну електронну адресу.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть пароль.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Пароль має містити щонайменше 8 символів.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Підтвердіть пароль.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Паролі не співпадають.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Range(typeof(bool), "true", "true", ErrorMessage = "Потрібно прийняти умови використання.")]
    public bool TermsAccepted { get; set; }
}
