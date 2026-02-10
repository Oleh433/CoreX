using CoreX.Domain.Entities;
using CoreX.Domain.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreX.Infrastructure.Repositories
{
    public class ClubRepository : IClubRepository
    {
        private readonly ApplicationDbContext _context;

        public ClubRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Club?> GetByIdAsync(Guid id)
        {
            return await _context.Clubs
                .Include(x => x.Trainers)
                .Include(x => x.Subscriptions)
                .Include(x => x.Vacancies)
                .Include(x => x.Memberships)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<Club>> GetAllAsync()
        {
            return await _context.Clubs
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<List<Club>> GetByCityAsync(string city)
        {
            return await _context.Clubs
                .Where(x => x.City.ToLower() == city.ToLower())
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task AddAsync(Club club)
        {
            await _context.Clubs.AddAsync(club);
        }

        public void Update(Club club)
        {
            _context.Clubs.Update(club);
        }

        public void Delete(Club club)
        {
            _context.Clubs.Remove(club);
        }
    }
}
