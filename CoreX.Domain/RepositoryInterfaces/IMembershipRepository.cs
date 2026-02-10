using CoreX.Domain.Entities;

namespace CoreX.Domain.RepositoryInterfaces
{
    public interface IMembershipRepository
    {
        Task<Membership?> GetByIdAsync(Guid id);

        Task<List<Membership>> GetAllAsync();

        Task<List<Membership>> GetByUserIdAsync(Guid userId);

        Task<List<Membership>> GetByClubIdAsync(Guid clubId);

        Task<Membership?> GetActiveMembershipAsync(Guid userId, Guid clubId);

        Task AddAsync(Membership membership);

        void Update(Membership membership);

        void Delete(Membership membership);
    }
}
