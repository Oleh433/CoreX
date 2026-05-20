using CoreX.Application.DTO;

namespace CoreX.Application.ServiceInterfaces
{
    public interface IClubService
    {
        Task<ClubResponseDto?> GetByIdAsync(Guid id);

        Task<List<ClubResponseDto>> GetAllAsync();

        Task<List<ClubResponseDto>> GetByCityAsync(string city);

        Task<Guid> CreateAsync(CreateClubDto dto);

        Task<bool> UpdateAsync(Guid id, UpdateClubDto dto);

        Task<bool> DeleteAsync(Guid id);
    }
}
