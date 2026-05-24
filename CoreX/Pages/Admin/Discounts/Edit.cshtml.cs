using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using CoreX.Pages.Admin.Discounts.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Admin.Discounts;

public class EditModel : PageModel
{
    private readonly IDiscountService _discounts;

    public EditModel(IDiscountService discounts) => _discounts = discounts;

    [BindProperty]
    public DiscountInput Input { get; set; } = new();

    public Guid Id { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        Id = id;
        var discount = await _discounts.GetByIdAsync(id);
        if (discount is null) return NotFound();

        Input = new DiscountInput
        {
            Title = discount.Title,
            Description = discount.Description,
            DiscountPercent = discount.DiscountPercent,
            Conditions = discount.Conditions,
            PromoCode = discount.PromoCode,
            StartDate = discount.StartDate ?? DateTime.UtcNow.Date,
            EndDate = discount.EndDate ?? DateTime.UtcNow.Date.AddDays(14),
            IsActive = discount.IsActive,
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        Id = id;
        if (!ModelState.IsValid) return Page();

        try
        {
            await _discounts.UpdateAsync(id, new UpdateDiscountDto
            {
                Title = Input.Title,
                Description = Input.Description,
                DiscountPercent = Input.DiscountPercent,
                Conditions = Input.Conditions,
                PromoCode = Input.PromoCode,
                StartDate = Input.StartDate,
                EndDate = Input.EndDate,
                IsActive = Input.IsActive,
            });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }

        // Emit an absolute URL — tests assert on Location?.AbsolutePath, which throws on
        // relative URIs (established Phase 3/4 workaround).
        var absoluteUrl = Url.Page("/Admin/Discounts/Index", pageHandler: null, values: null, protocol: Request.Scheme);
        return Redirect(absoluteUrl!);
    }
}
