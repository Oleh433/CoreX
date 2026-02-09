using CoreX.Domain.Entities;

namespace CoreX.Domain.RepositoryInterfaces
{
    public interface IDiscountRepository
    {
        Task<Discount?> GetByIdAsync(Guid id);

        Task<List<Discount>> GetAllAsync();

        Task<List<Discount>> GetActiveAsync();

        Task AddAsync(Discount discount);

        void Update(Discount discount);

        void Delete(Discount discount);
    }
}
