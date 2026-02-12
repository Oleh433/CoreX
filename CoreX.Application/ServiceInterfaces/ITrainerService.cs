using CoreX.Application.DTO;

namespace CoreX.Application.ServiceInterfaces
{
    public interface ITrainerService
    {
        Task<List<TrainerResponseDto>> GetAllAsync();

        Task<List<TrainerResponseDto>> GetByClubIdAsync(Guid clubId);

        Task<TrainerResponseDto?> GetByIdAsync(Guid id);

        Task<Guid> CreateAsync(CreateTrainerDto dto);

        Task<bool> UpdateAsync(Guid id, UpdateTrainerDto dto);

        Task<bool> DeleteAsync(Guid id);
    }
}
