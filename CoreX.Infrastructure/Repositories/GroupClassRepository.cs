using CoreX.Domain.Entities;
using CoreX.Domain.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreX.Infrastructure.Repositories
{
    public class GroupClassRepository : IGroupClassRepository
    {
        private readonly ApplicationDbContext _context;

        public GroupClassRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<GroupClass?> GetByIdAsync(Guid id)
        {
            return await _context.GroupClasses
                .Include(x => x.Trainer)
                .Include(x => x.Club)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<GroupClass>> GetByClubIdAsync(Guid clubId, GroupClassAudience? audience = null)
        {
            var query = _context.GroupClasses
                .Where(x => x.ClubId == clubId);

            if (audience.HasValue)
                query = query.Where(x => x.Audience == audience.Value);

            return await query
                .Include(x => x.Trainer)
                .AsNoTracking()
                .OrderBy(x => x.StartTime)
                .ToListAsync();
        }

        public async Task AddAsync(GroupClass groupClass)
        {
            await _context.GroupClasses.AddAsync(groupClass);
        }

        public void Update(GroupClass groupClass)
        {
            _context.GroupClasses.Update(groupClass);
        }

        public void Delete(GroupClass groupClass)
        {
            _context.GroupClasses.Remove(groupClass);
        }
    }
}
