using CoreX.Application.DTO;

namespace CoreX.Application.ServiceInterfaces
{
    public interface ISubscriptionService
    {
        Task<SubscriptionResponseDto?> GetByIdAsync(Guid id);

        Task<List<SubscriptionResponseDto>> GetAllAsync();

        Task<List<SubscriptionResponseDto>> GetByClubIdAsync(Guid clubId);

        Task<Guid> CreateAsync(CreateSubscriptionDto dto);

        Task<bool> UpdateAsync(Guid id, UpdateSubscriptionDto dto);

        Task<bool> ActivateAsync(Guid id);

        Task<bool> DeactivateAsync(Guid id);

        Task<bool> DeleteAsync(Guid subscriptionId);
    }
}
