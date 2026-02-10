using CoreX.Domain.Entities;
using CoreX.Domain.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreX.Infrastructure.Repositories
{
    public class VacancyApplicationRepository : IVacancyApplicationRepository
    {
        private readonly ApplicationDbContext _context;

        public VacancyApplicationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<VacancyApplication?> GetByIdAsync(Guid id)
        {
            return await _context.VacancyApplications
                .Include(x => x.Vacancy)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<VacancyApplication>> GetAllAsync()
        {
            return await _context.VacancyApplications
                .AsNoTracking()
                .Include(x => x.Vacancy)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<VacancyApplication>> GetByVacancyIdAsync(Guid vacancyId)
        {
            return await _context.VacancyApplications
                .Where(x => x.VacancyId == vacancyId)
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<VacancyApplication>> GetByApplicantIdAsync(Guid applicantId)
        {
            return await _context.VacancyApplications
                .Where(x => x.ApplicantId == applicantId)
                .AsNoTracking()
                .Include(x => x.Vacancy)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(VacancyApplication application)
        {
            await _context.VacancyApplications.AddAsync(application);
        }

        public void Update(VacancyApplication application)
        {
            _context.VacancyApplications.Update(application);
        }

        public void Delete(VacancyApplication application)
        {
            _context.VacancyApplications.Remove(application);
        }
    }

}
