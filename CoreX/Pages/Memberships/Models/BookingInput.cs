using System.ComponentModel.DataAnnotations;

namespace CoreX.Pages.Memberships.Models;

public class BookingInput
{
    [Required(ErrorMessage = "Введіть повне ім'я.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Ім'я має містити від 3 до 100 символів.")]
    public string ContactFullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть електронну пошту.")]
    [EmailAddress(ErrorMessage = "Введіть коректну електронну адресу.")]
    public string ContactEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть телефон.")]
    [Phone(ErrorMessage = "Введіть коректний номер телефону.")]
    public string ContactPhone { get; set; } = string.Empty;
}
