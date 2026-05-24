using System.ComponentModel.DataAnnotations;
using CoreX.Domain.Entities;

namespace CoreX.Pages.Admin.GroupClasses.Models;

public class GroupClassInput
{
    [Required(ErrorMessage = "Оберіть клуб.")]
    public Guid ClubId { get; set; }

    public Guid? TrainerId { get; set; }

    [Required(ErrorMessage = "Введіть тип занять.")]
    [StringLength(100, MinimumLength = 2)]
    public string Type { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required]
    public GroupClassAudience Audience { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Range(5, 300, ErrorMessage = "Тривалість від 5 до 300 хвилин.")]
    public int DurationMinutes { get; set; }

    [Range(1, 200, ErrorMessage = "Місткість від 1 до 200.")]
    public int Capacity { get; set; }

    public decimal? Price { get; set; }
}
