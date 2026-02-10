using CoreX.Domain.Entities;

namespace CoreX.Domain.RepositoryInterfaces
{
    public interface IClubRepository
    {
        Task<Club?> GetByIdAsync(Guid id);

        Task<List<Club>> GetAllAsync();

        Task<List<Club>> GetByCityAsync(string city);

        Task AddAsync(Club club);

        void Update(Club club);

        void Delete(Club club);
    }
}
