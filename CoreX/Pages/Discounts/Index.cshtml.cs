using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Discounts;

public class IndexModel : PageModel
{
    private readonly IDiscountService _discounts;
    public IndexModel(IDiscountService discounts) => _discounts = discounts;

    public IReadOnlyList<DiscountResponseDto> Discounts { get; private set; } = Array.Empty<DiscountResponseDto>();

    public async Task OnGetAsync() => Discounts = await _discounts.GetActiveAsync();
}
