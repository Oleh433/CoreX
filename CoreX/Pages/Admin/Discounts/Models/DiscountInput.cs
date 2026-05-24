using System.ComponentModel.DataAnnotations;

namespace CoreX.Pages.Admin.Discounts.Models;

public class DiscountInput
{
    [Required(ErrorMessage = "Введіть назву")]
    [StringLength(150, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Range(0, 100, ErrorMessage = "Відсоток від 0 до 100")]
    public decimal? DiscountPercent { get; set; }

    [StringLength(1000)]
    public string? Conditions { get; set; }

    [StringLength(50)]
    public string? PromoCode { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    // Only relevant on Edit. The Edit page pre-fills this from the entity in OnGet, and the
    // checkbox + hidden-input pair that Razor's tag helper renders for `asp-for=bool` lets
    // the browser POST `false` when unchecked. Default left at `false` (the C# default for
    // bool) so manually-crafted POSTs that omit the field also deactivate correctly.
    // The Create flow doesn't read this — entity ctor sets IsActive = true on creation.
    public bool IsActive { get; set; }
}
