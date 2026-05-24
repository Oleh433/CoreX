using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Admin.InformationMaterials;

public class IndexModel : PageModel
{
    private readonly IInformationMaterialService _materials;
    public IndexModel(IInformationMaterialService materials) => _materials = materials;

    public IReadOnlyList<InformationMaterialResponseDto> Materials { get; private set; } = Array.Empty<InformationMaterialResponseDto>();

    public async Task OnGetAsync() => Materials = await _materials.GetAllAsync();

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        await _materials.DeleteAsync(id);
        return Content(string.Empty, "text/html"); // HTMX swaps the row to nothing
    }
}
