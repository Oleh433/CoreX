using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.InformationMaterials;

public class IndexModel : PageModel
{
    private readonly IInformationMaterialService _materials;
    public IndexModel(IInformationMaterialService materials) => _materials = materials;

    public IReadOnlyList<InformationMaterialResponseDto> Materials { get; private set; } = Array.Empty<InformationMaterialResponseDto>();

    public async Task OnGetAsync() => Materials = await _materials.GetAllAsync();
}
