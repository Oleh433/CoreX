using System.ComponentModel.DataAnnotations;

namespace CoreX.Pages.Admin.InformationMaterials.Models;

public class MaterialInput
{
    [Required(ErrorMessage = "Введіть заголовок")]
    [StringLength(200, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть текст")]
    [StringLength(20000, MinimumLength = 10)]
    public string Body { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Category { get; set; }
}
