using CoreX.Domain.Entities;

namespace CoreX.Domain.RepositoryInterfaces
{
    public interface IGroupClassRepository
    {
        Task<GroupClass?> GetByIdAsync(Guid id);

        Task<List<GroupClass>> GetByClubIdAsync(Guid clubId, GroupClassAudience? audience = null);

        Task AddAsync(GroupClass groupClass);

        void Update(GroupClass groupClass);

        void Delete(GroupClass groupClass);
    }
}
