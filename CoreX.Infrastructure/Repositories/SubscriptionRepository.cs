using CoreX.Domain.Entities;
using CoreX.Domain.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreX.Infrastructure.Repositories
{
    public class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public SubscriptionRepository(ApplicationDbContext context)
        {
            _dbContext = context;
        }

        public async Task<Subscription?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Subscriptions
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<Subscription>> GetAllAsync()
        {
            return await _dbContext.Subscriptions
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Subscription>> GetByClubIdAsync(Guid clubId)
        {
            return await _dbContext.Subscriptions
                .Where(x => x.ClubId == clubId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task AddAsync(Subscription subscription)
        {
            await _dbContext.Subscriptions.AddAsync(subscription);
        }

        public void Update(Subscription subscription)
        {
            _dbContext.Subscriptions.Update(subscription);
        }

        public void Delete(Subscription subscription)
        {
            _dbContext.Subscriptions.Remove(subscription);
        }
    }

}
