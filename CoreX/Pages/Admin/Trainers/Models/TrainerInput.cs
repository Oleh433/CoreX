using System.ComponentModel.DataAnnotations;

namespace CoreX.Pages.Admin.Trainers.Models;

public class TrainerInput
{
    [Required(ErrorMessage = "Оберіть клуб.")]
    public Guid ClubId { get; set; }

    [Required(ErrorMessage = "Введіть повне ім'я.")]
    [StringLength(150, MinimumLength = 3)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть спеціалізацію.")]
    [StringLength(100, MinimumLength = 2)]
    public string Specialization { get; set; } = string.Empty;

    [Range(0, 60, ErrorMessage = "Досвід має бути від 0 до 60 років.")]
    public int ExperienceYears { get; set; }

    [StringLength(2000)]
    public string? Bio { get; set; }

    [EmailAddress(ErrorMessage = "Введіть коректний email.")]
    [StringLength(150)]
    public string? Email { get; set; }

    [Phone(ErrorMessage = "Введіть коректний номер.")]
    [StringLength(30)]
    public string? Phone { get; set; }
}
