using CoreX.Application.DTO;
using CoreX.Domain.Entities;

namespace CoreX.Application.ServiceInterfaces
{
    public interface IGroupClassService
    {
        Task<GroupClassResponseDto?> GetByIdAsync(Guid id);

        Task<List<GroupClassResponseDto>> GetByClubIdAsync(Guid clubId, GroupClassAudience? audience = null);

        Task<Guid> CreateAsync(CreateGroupClassDto dto);

        Task<bool> UpdateAsync(Guid id, UpdateGroupClassDto dto);

        Task<bool> DeleteAsync(Guid id);
    }
}
