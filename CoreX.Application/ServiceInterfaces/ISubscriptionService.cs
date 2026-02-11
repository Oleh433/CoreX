using CoreX.Application.DTO;

namespace CoreX.Application.ServiceInterfaces
{
    public interface ISubscriptionService
    {
        Task<SubscriptionResponseDto?> GetByIdAsync(Guid id);

        Task<List<SubscriptionResponseDto>> GetByClubIdAsync(Guid clubId);

        Task<Guid> CreateAsync(CreateSubscriptionDto dto);

        Task<bool> DeleteAsync(Guid subscriptionId);
    }
}
