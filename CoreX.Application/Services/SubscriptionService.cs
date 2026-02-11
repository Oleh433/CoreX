using CoreX.Application.DTO;
using CoreX.Application.Mappers;
using CoreX.Application.ServiceInterfaces;
using CoreX.Domain;
using CoreX.Domain.Entities;
using CoreX.Domain.RepositoryInterfaces;

namespace CoreX.Application.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public SubscriptionService(
            ISubscriptionRepository subscriptionRepository,
            IUnitOfWork unitOfWork)
        {
            _subscriptionRepository = subscriptionRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<SubscriptionResponseDto?> GetByIdAsync(Guid id)
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(id);

            if (subscription == null)
                return null;

            return SubscriptionMapper.ToDto(subscription);
        }

        public async Task<List<SubscriptionResponseDto>> GetByClubIdAsync(Guid clubId)
        {
            var subscriptions = await _subscriptionRepository.GetByClubIdAsync(clubId);

            return subscriptions
                .Select(SubscriptionMapper.ToDto)
                .ToList();
        }

        public async Task<Guid> CreateAsync(CreateSubscriptionDto dto)
        {
            if (dto.ClubId == Guid.Empty)
                throw new ArgumentException("ClubId is required.");

            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new ArgumentException("Title is required.");

            if (dto.Price <= 0)
                throw new ArgumentException("Price must be greater than 0.");

            if (dto.DurationDays <= 0)
                throw new ArgumentException("DurationDays must be greater than 0.");

            var subscription = new Subscription(
                title: dto.Title,
                clubId: dto.ClubId,
                price: dto.Price,
                durationDays: dto.DurationDays,
                visitsLimit: dto.VisitsLimit,
                description: dto.Description
            );

            await _subscriptionRepository.AddAsync(subscription);

            await _unitOfWork.SaveChangesAsync();

            return subscription.Id;
        }

        public async Task<bool> DeleteAsync(Guid subscriptionId)
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);

            if (subscription == null)
                return false;

            _subscriptionRepository.Delete(subscription);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
