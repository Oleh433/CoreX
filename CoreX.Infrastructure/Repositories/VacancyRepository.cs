using CoreX.Domain.Entities;
using CoreX.Domain.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreX.Infrastructure.Repositories
{
    public class VacancyRepository : IVacancyRepository
    {
        private readonly ApplicationDbContext _context;

        public VacancyRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Vacancy?> GetByIdAsync(Guid id)
        {
            return await _context.Vacancies
                .Include(x => x.Club)
                .Include(x => x.Applications)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<Vacancy>> GetAllAsync()
        {
            return await _context.Vacancies
                .AsNoTracking()
                .Include(x => x.Club)
                .OrderByDescending(x => x.Id)
                .ToListAsync();
        }

        public async Task<List<Vacancy>> GetActiveAsync()
        {
            return await _context.Vacancies
                .Where(x => x.IsActive)
                .AsNoTracking()
                .Include(x => x.Club)
                .OrderByDescending(x => x.Id)
                .ToListAsync();
        }

        public async Task<List<Vacancy>> GetByClubIdAsync(Guid clubId)
        {
            return await _context.Vacancies
                .Where(x => x.ClubId == clubId)
                .AsNoTracking()
                .OrderByDescending(x => x.Id)
                .ToListAsync();
        }

        public async Task AddAsync(Vacancy vacancy)
        {
            await _context.Vacancies.AddAsync(vacancy);
        }

        public void Update(Vacancy vacancy)
        {
            _context.Vacancies.Update(vacancy);
        }

        public void Delete(Vacancy vacancy)
        {
            _context.Vacancies.Remove(vacancy);
        }
    }
}
