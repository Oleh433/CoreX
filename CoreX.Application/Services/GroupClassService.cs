using CoreX.Application.DTO;
using CoreX.Application.Mappers;
using CoreX.Application.ServiceInterfaces;
using CoreX.Domain;
using CoreX.Domain.Entities;
using CoreX.Domain.RepositoryInterfaces;

namespace CoreX.Application.Services
{
    public class GroupClassService : IGroupClassService
    {
        private readonly IGroupClassRepository _groupClassRepository;
        private readonly IClubRepository _clubRepository;
        private readonly IUnitOfWork _unitOfWork;

        public GroupClassService(
            IGroupClassRepository groupClassRepository,
            IClubRepository clubRepository,
            IUnitOfWork unitOfWork)
        {
            _groupClassRepository = groupClassRepository;
            _clubRepository = clubRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<GroupClassResponseDto?> GetByIdAsync(Guid id)
        {
            var groupClass = await _groupClassRepository.GetByIdAsync(id);

            if (groupClass == null)
                return null;

            return GroupClassMapper.ToDto(groupClass);
        }

        public async Task<List<GroupClassResponseDto>> GetByClubIdAsync(Guid clubId, GroupClassAudience? audience = null)
        {
            var classes = await _groupClassRepository.GetByClubIdAsync(clubId, audience);

            return classes.Select(GroupClassMapper.ToDto).ToList();
        }

        public async Task<Guid> CreateAsync(CreateGroupClassDto dto)
        {
            var club = await _clubRepository.GetByIdAsync(dto.ClubId);

            if (club == null)
                throw new KeyNotFoundException("Club not found.");

            var groupClass = new GroupClass(
                clubId: dto.ClubId,
                type: dto.Type,
                audience: dto.Audience,
                startTime: dto.StartTime,
                durationMinutes: dto.DurationMinutes,
                capacity: dto.Capacity,
                trainerId: dto.TrainerId,
                price: dto.Price,
                description: dto.Description
            );

            await _groupClassRepository.AddAsync(groupClass);

            await _unitOfWork.SaveChangesAsync();

            return groupClass.Id;
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateGroupClassDto dto)
        {
            var groupClass = await _groupClassRepository.GetByIdAsync(id);

            if (groupClass == null)
                return false;

            groupClass.Update(
                type: dto.Type,
                audience: dto.Audience,
                startTime: dto.StartTime,
                durationMinutes: dto.DurationMinutes,
                capacity: dto.Capacity,
                trainerId: dto.TrainerId,
                price: dto.Price,
                description: dto.Description
            );

            _groupClassRepository.Update(groupClass);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var groupClass = await _groupClassRepository.GetByIdAsync(id);

            if (groupClass == null)
                return false;

            _groupClassRepository.Delete(groupClass);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
