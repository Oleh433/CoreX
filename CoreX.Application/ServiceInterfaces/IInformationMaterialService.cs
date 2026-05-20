using CoreX.Application.DTO;

namespace CoreX.Application.ServiceInterfaces
{
    public interface IInformationMaterialService
    {
        Task<InformationMaterialResponseDto?> GetByIdAsync(Guid id);

        Task<List<InformationMaterialResponseDto>> GetAllAsync();

        Task<Guid> CreateAsync(CreateInformationMaterialDto dto);

        Task<bool> UpdateAsync(Guid id, UpdateInformationMaterialDto dto);

        Task<bool> DeleteAsync(Guid id);
    }
}
