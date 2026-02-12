using CoreX.Application.DTO;

namespace CoreX.Application.ServiceInterfaces
{
    public interface IDiscountService
    {
        Task<DiscountResponseDto?> GetByIdAsync(Guid id);

        Task<List<DiscountResponseDto>> GetAllAsync();

        Task<List<DiscountResponseDto>> GetActiveAsync();

        Task<Guid> CreateAsync(CreateDiscountDto dto);

        Task<bool> UpdateAsync(Guid id, UpdateDiscountDto dto);

        Task<bool> DeleteAsync(Guid id);
    }
}
