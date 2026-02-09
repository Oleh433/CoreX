using CoreX.Domain.Entities;
using CoreX.Domain.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreX.Infrastructure.Repositories
{
    public class DiscountRepository : IDiscountRepository
    {
        private readonly ApplicationDbContext _context;

        public DiscountRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Discount?> GetByIdAsync(Guid id)
        {
            return await _context.Discounts
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<Discount>> GetAllAsync()
        {
            return await _context.Discounts
                .AsNoTracking()
                .OrderByDescending(x => x.StartDate)
                .ToListAsync();
        }

        public async Task<List<Discount>> GetActiveAsync()
        {
            var now = DateTime.UtcNow;

            return await _context.Discounts
                .Where(x =>
                    x.IsActive &&
                    x.StartDate <= now &&
                    x.EndDate >= now)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task AddAsync(Discount discount)
        {
            await _context.Discounts.AddAsync(discount);
        }

        public void Update(Discount discount)
        {
            _context.Discounts.Update(discount);
        }

        public void Delete(Discount discount)
        {
            _context.Discounts.Remove(discount);
        }
    }
}
