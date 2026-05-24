using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using CoreX.Pages.Admin.Discounts.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Admin.Discounts;

public class CreateModel : PageModel
{
    private readonly IDiscountService _discounts;

    public CreateModel(IDiscountService discounts) => _discounts = discounts;

    [BindProperty]
    public DiscountInput Input { get; set; } = new();

    public void OnGet()
    {
        // Sensible defaults for the form so the user can submit immediately.
        Input.StartDate = DateTime.UtcNow.Date;
        Input.EndDate = DateTime.UtcNow.Date.AddDays(14);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        try
        {
            await _discounts.CreateAsync(new CreateDiscountDto
            {
                Title = Input.Title,
                Description = Input.Description,
                DiscountPercent = Input.DiscountPercent,
                Conditions = Input.Conditions,
                PromoCode = Input.PromoCode,
                StartDate = Input.StartDate,
                EndDate = Input.EndDate,
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
