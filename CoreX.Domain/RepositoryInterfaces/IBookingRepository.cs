using CoreX.Domain.Entities;

namespace CoreX.Domain.RepositoryInterfaces
{
    public interface IBookingRepository
    {
        Task<Booking?> GetByIdAsync(Guid id);

        Task<List<Booking>> GetAllAsync();

        Task<List<Booking>> GetByUserIdAsync(Guid userId);

        Task<List<Booking>> GetByClubIdAsync(Guid clubId);

        Task<List<Booking>> GetActiveBookingsAsync();

        Task AddAsync(Booking booking);

        void Update(Booking booking);

        void Delete(Booking booking);
    }
}
