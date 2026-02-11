using CoreX.Application.DTO;

namespace CoreX.Application.ServiceInterfaces
{
    public interface IMembershipService
    {
        Task<MembershipResponseDto?> GetByIdAsync(Guid id);

        Task<List<MembershipResponseDto>> GetAllAsync();

        Task<List<MembershipResponseDto>> GetByUserIdAsync(Guid userId);

        Task<List<MembershipResponseDto>> GetByClubIdAsync(Guid clubId);

        Task<Guid> CreateAsync(CreateMembershipDto dto);

        Task<bool> DeleteAsync(Guid membershipId);
    }
}
