using CoreX.Domain.Entities;

namespace CoreX.Domain.RepositoryInterfaces
{
    public interface IVacancyRepository
    {
        Task<Vacancy?> GetByIdAsync(Guid id);

        Task<List<Vacancy>> GetAllAsync();

        Task<List<Vacancy>> GetActiveAsync();

        Task<List<Vacancy>> GetByClubIdAsync(Guid clubId);

        Task AddAsync(Vacancy vacancy);

        void Update(Vacancy vacancy);

        void Delete(Vacancy vacancy);
    }
}
