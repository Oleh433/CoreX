using CoreX.Domain.Entities;
using CoreX.Domain.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreX.Infrastructure.Repositories
{
    public class TrainerRepository : ITrainerRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public TrainerRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Trainer>> GetAllAsync()
        {
            return await _dbContext.Trainers
                .Include(x => x.Club)
                .OrderBy(x => x.FullName)
                .ToListAsync();
        }

        public async Task<List<Trainer>> GetByClubIdAsync(Guid clubId)
        {
            return await _dbContext.Trainers
                .Where(x => x.ClubId == clubId)
                .OrderBy(x => x.FullName)
                .ToListAsync();
        }

        public async Task<Trainer?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Trainers
                .Include(x => x.Club)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task AddAsync(Trainer trainer)
        {
            await _dbContext.Trainers
                .AddAsync(trainer);
        }

        public void Delete(Trainer trainer)
        {
            _dbContext.Trainers
                .Remove(trainer);
        }

        public void Update(Trainer trainer)
        {
            _dbContext.Trainers.Update(trainer);
        }
    }
}
