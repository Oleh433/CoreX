using CoreX.Domain.Entities;
using CoreX.Domain.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreX.Infrastructure.Repositories
{
    public class MembershipRepository : IMembershipRepository
    {
        private readonly ApplicationDbContext _context;

        public MembershipRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Membership?> GetByIdAsync(Guid id)
        {
            return await _context.Memberships
                .Include(x => x.Club)
                .Include(x => x.Subscription)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<Membership>> GetAllAsync()
        {
            return await _context.Memberships
                .AsNoTracking()
                .Include(x => x.Club)
                .Include(x => x.Subscription)
                .ToListAsync();
        }

        public async Task<List<Membership>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Memberships
                .Where(x => x.UserId == userId)
                .AsNoTracking()
                .Include(x => x.Club)
                .Include(x => x.Subscription)
                .ToListAsync();
        }

        public async Task<List<Membership>> GetByClubIdAsync(Guid clubId)
        {
            return await _context.Memberships
                .Where(x => x.ClubId == clubId)
                .AsNoTracking()
                .Include(x => x.Subscription)
                .ToListAsync();
        }

        public async Task<Membership?> GetActiveMembershipAsync(Guid userId, Guid clubId)
        {
            return await _context.Memberships
                .Where(x =>
                    x.UserId == userId &&
                    x.ClubId == clubId)
                .OrderByDescending(x => x.StartTime)
                .FirstOrDefaultAsync();
        }
        public async Task AddAsync(Membership membership)
        {
            await _context.Memberships.AddAsync(membership);
        }

        public void Update(Membership membership)
        {
            _context.Memberships.Update(membership);
        }

        public void Delete(Membership membership)
        {
            _context.Memberships.Remove(membership);
        }
    }
}
