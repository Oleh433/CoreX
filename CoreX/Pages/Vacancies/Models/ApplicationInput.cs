using System.ComponentModel.DataAnnotations;

namespace CoreX.Pages.Vacancies.Models;

public class ApplicationInput
{
    [Required(ErrorMessage = "Введіть повне ім'я.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "Ім'я має містити від 3 до 150 символів.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть електронну пошту.")]
    [EmailAddress(ErrorMessage = "Введіть коректну електронну адресу.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть телефон.")]
    [Phone(ErrorMessage = "Введіть коректний номер телефону.")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Опишіть досвід.")]
    [StringLength(2000, MinimumLength = 3, ErrorMessage = "Опис досвіду має містити від 3 до 2000 символів.")]
    public string Experience { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Повідомлення не може перевищувати 2000 символів.")]
    public string? Message { get; set; }

    [Url(ErrorMessage = "Введіть коректне посилання на CV.")]
    [StringLength(500)]
    public string? CVLink { get; set; }
}
