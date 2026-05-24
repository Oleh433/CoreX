using System.ComponentModel.DataAnnotations;

namespace CoreX.Pages.Admin.Subscriptions.Models;

public class SubscriptionInput
{
    [Required(ErrorMessage = "Оберіть клуб")]
    public Guid ClubId { get; set; }

    [Required(ErrorMessage = "Введіть назву")]
    [StringLength(150, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [Range(0, 100_000, ErrorMessage = "Ціна від 0 до 100 000")]
    public decimal Price { get; set; }

    [Range(1, 3650, ErrorMessage = "Тривалість від 1 до 3650 днів")]
    public int DurationDays { get; set; }

    [Range(1, 1000)]
    public int? VisitsLimit { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }
}
