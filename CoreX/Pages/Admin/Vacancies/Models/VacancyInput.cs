using System.ComponentModel.DataAnnotations;

namespace CoreX.Pages.Admin.Vacancies.Models;

public class VacancyInput
{
    [Required(ErrorMessage = "Оберіть клуб")]
    public Guid ClubId { get; set; }

    [Required(ErrorMessage = "Введіть заголовок")]
    [StringLength(150, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть опис")]
    [StringLength(5000, MinimumLength = 10)]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть вимоги")]
    [StringLength(5000, MinimumLength = 5)]
    public string Requirements { get; set; } = string.Empty;

    [Range(0, 1_000_000)]
    public decimal? Salary { get; set; }

    public DateTime? ApplicationDeadline { get; set; }
}
