using CoreX.Domain.Entities;

namespace CoreX.Domain.RepositoryInterfaces
{
    public interface ISubscriptionRepository
    {
        Task<Subscription?> GetByIdAsync(Guid id);

        Task<List<Subscription>> GetAllAsync();

        Task<List<Subscription>> GetByClubIdAsync(Guid clubId);

        Task AddAsync(Subscription subscription);

        void Update(Subscription subscription);

        void Delete(Subscription subscription);
    }
}
