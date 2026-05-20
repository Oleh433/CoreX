using CoreX.Application.DTO;
using CoreX.Application.Mappers;
using CoreX.Application.ServiceInterfaces;
using CoreX.Domain;
using CoreX.Domain.Entities;
using CoreX.Domain.RepositoryInterfaces;

namespace CoreX.Application.Services
{
    public class TrainerService : ITrainerService
    {
        private readonly ITrainerRepository _trainerRepository;
        private readonly IClubRepository _clubRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;

        public TrainerService(
            ITrainerRepository trainerRepository,
            IClubRepository clubRepository,
            IUnitOfWork unitOfWork,
            IEmailSender emailSender)
        {
            _trainerRepository = trainerRepository;
            _clubRepository = clubRepository;
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
        }

        public async Task<List<TrainerResponseDto>> GetAllAsync()
        {
            var trainers = await _trainerRepository.GetAllAsync();

            return trainers
                .Select(TrainerMapper.ToDto)
                .ToList();
        }

        public async Task<List<TrainerResponseDto>> GetByClubIdAsync(Guid clubId)
        {
            var trainers = await _trainerRepository.GetByClubIdAsync(clubId);

            return trainers
                .Select(TrainerMapper.ToDto)
                .ToList();
        }

        public async Task<TrainerResponseDto?> GetByIdAsync(Guid id)
        {
            var trainer = await _trainerRepository.GetByIdAsync(id);

            if (trainer == null)
                return null;

            return TrainerMapper.ToDto(trainer);
        }

        public async Task<Guid> CreateAsync(CreateTrainerDto dto)
        {
            var club = await _clubRepository.GetByIdAsync(dto.ClubId);

            if (club == null)
                throw new KeyNotFoundException("Club not found.");

            if (string.IsNullOrWhiteSpace(dto.FullName))
                throw new ArgumentException("FullName is required.");

            if (dto.ExperienceYears < 0)
                throw new ArgumentException("ExperienceYears cannot be negative.");

            var trainer = new Trainer(
                clubId: dto.ClubId,
                fullName: dto.FullName,
                specialization: dto.Specialization,
                experienceYears: dto.ExperienceYears,
                bio: dto.Bio,
                email: dto.Email,
                phone: dto.Phone
            );

            await _trainerRepository.AddAsync(trainer);

            await _unitOfWork.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(trainer.Email))
            {
                await _emailSender.SendAsync(
                    trainer.Email,
                    "Welcome to the team",
                    $"Hello {trainer.FullName}, you have been added as a trainer at {club.Name}.");
            }

            return trainer.Id;
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateTrainerDto dto)
        {
            var trainer = await _trainerRepository.GetByIdAsync(id);

            if (trainer == null)
                return false;

            trainer.Update(
                dto.FullName,
                dto.Specialization,
                dto.ExperienceYears,
                dto.Bio,
                dto.Email,
                dto.Phone
            );

            _trainerRepository.Update(trainer);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }


        public async Task<bool> DeleteAsync(Guid id)
        {
            var trainer = await _trainerRepository.GetByIdAsync(id);

            if (trainer == null)
                return false;

            _trainerRepository.Delete(trainer);

            await _unitOfWork.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(trainer.Email))
            {
                await _emailSender.SendAsync(
                    trainer.Email,
                    "Trainer account removed",
                    $"Hello {trainer.FullName}, your trainer record has been removed.");
            }

            return true;
        }
    }
}
