using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Admin.Discounts;

public class IndexModel : PageModel
{
    private readonly IDiscountService _discounts;

    public IndexModel(IDiscountService discounts) => _discounts = discounts;

    public IReadOnlyList<DiscountResponseDto> Discounts { get; private set; } = Array.Empty<DiscountResponseDto>();

    public async Task OnGetAsync() => Discounts = await _discounts.GetAllAsync();

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        await _discounts.DeleteAsync(id);
        return Content(string.Empty, "text/html"); // HTMX swaps the row to nothing
    }
}
