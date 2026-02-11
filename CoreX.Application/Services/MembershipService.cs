using CoreX.Application.DTO;
using CoreX.Application.Mappers;
using CoreX.Application.ServiceInterfaces;
using CoreX.Domain;
using CoreX.Domain.Entities;
using CoreX.Domain.RepositoryInterfaces;

namespace CoreX.Application.Services
{
    public class MembershipService : IMembershipService
    {
        private readonly IMembershipRepository _membershipRepository;
        private readonly IUnitOfWork _unitOfWork;

        public MembershipService(
            IMembershipRepository membershipRepository,
            IUnitOfWork unitOfWork)
        {
            _membershipRepository = membershipRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<MembershipResponseDto?> GetByIdAsync(Guid id)
        {
            var membership = await _membershipRepository.GetByIdAsync(id);

            if (membership == null)
                return null;

            return MembershipMapper.ToDto(membership);
        }

        public async Task<List<MembershipResponseDto>> GetAllAsync()
        {
            var memberships = await _membershipRepository.GetAllAsync();

            return memberships
                .Select(MembershipMapper.ToDto)
                .ToList();
        }

        public async Task<List<MembershipResponseDto>> GetByUserIdAsync(Guid userId)
        {
            var memberships = await _membershipRepository.GetByUserIdAsync(userId);

            return memberships
                .Select(MembershipMapper.ToDto)
                .ToList();
        }

        public async Task<List<MembershipResponseDto>> GetByClubIdAsync(Guid clubId)
        {
            var memberships = await _membershipRepository.GetByClubIdAsync(clubId);

            return memberships
                .Select(MembershipMapper.ToDto)
                .ToList();
        }

        public async Task<Guid> CreateAsync(CreateMembershipDto dto)
        {
            if (dto.UserId == Guid.Empty)
                throw new ArgumentException("UserId is required.");

            if (dto.ClubId == Guid.Empty)
                throw new ArgumentException("ClubId is required.");

            var existing = await _membershipRepository
                .GetActiveMembershipAsync(dto.UserId, dto.ClubId);

            if (existing != null)
                throw new InvalidOperationException("User already has active membership in this club.");

            var membership = new Membership(
                userId: dto.UserId,
                clubId: dto.ClubId,
                subscriptionId: dto.SubscriptionId
            );

            await _membershipRepository.AddAsync(membership);

            await _unitOfWork.SaveChangesAsync();

            return membership.Id;
        }

        public async Task<bool> DeleteAsync(Guid membershipId)
        {
            var membership = await _membershipRepository.GetByIdAsync(membershipId);

            if (membership == null)
                return false;

            _membershipRepository.Delete(membership);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
