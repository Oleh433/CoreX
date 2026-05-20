using CoreX.Application.DTO;
using CoreX.Application.Mappers;
using CoreX.Application.ServiceInterfaces;
using CoreX.Domain;
using CoreX.Domain.Entities;
using CoreX.Domain.RepositoryInterfaces;

namespace CoreX.Application.Services
{
    public class VacancyService : IVacancyService
    {
        private readonly IVacancyRepository _vacancyRepository;
        private readonly IClubRepository _clubRepository;
        private readonly IUnitOfWork _unitOfWork;

        public VacancyService(
            IVacancyRepository vacancyRepository,
            IClubRepository clubRepository,
            IUnitOfWork unitOfWork)
        {
            _vacancyRepository = vacancyRepository;
            _clubRepository = clubRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<VacancyResponseDto>> GetAllAsync()
        {
            var vacancies = await _vacancyRepository.GetAllAsync();

            return vacancies
                .Select(VacancyMapper.ToDto)
                .ToList();
        }

        public async Task<List<VacancyResponseDto>> GetActiveAsync()
        {
            var vacancies = await _vacancyRepository.GetActiveAsync();

            return vacancies
                .Select(VacancyMapper.ToDto)
                .ToList();
        }

        public async Task<List<VacancyResponseDto>> GetByClubIdAsync(Guid clubId)
        {
            var vacancies = await _vacancyRepository.GetByClubIdAsync(clubId);

            return vacancies
                .Select(VacancyMapper.ToDto)
                .ToList();
        }

        public async Task<VacancyResponseDto?> GetByIdAsync(Guid id)
        {
            var vacancy = await _vacancyRepository.GetByIdAsync(id);

            if (vacancy == null)
                return null;

            return VacancyMapper.ToDto(vacancy);
        }

        public async Task<Guid> CreateAsync(CreateVacancyDto dto)
        {
            var club = await _clubRepository.GetByIdAsync(dto.ClubId);

            if (club == null)
                throw new KeyNotFoundException("Club not found.");

            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new ArgumentException("Title is required.");

            var vacancy = new Vacancy(
                clubId: dto.ClubId,
                title: dto.Title,
                description: dto.Description,
                requirements: dto.Requirements,
                salary: dto.Salary,
                applicationDeadline: dto.ApplicationDeadline
            );

            await _vacancyRepository.AddAsync(vacancy);

            await _unitOfWork.SaveChangesAsync();

            return vacancy.Id;
        }

       
        public async Task<bool> UpdateAsync(Guid id, UpdateVacancyDto dto)
        {
            var vacancy = await _vacancyRepository.GetByIdAsync(id);

            if (vacancy == null)
                return false;

            vacancy.Update(
                dto.Title,
                dto.Description,
                dto.Requirements,
                dto.Salary,
                dto.ApplicationDeadline
            );

            _vacancyRepository.Update(vacancy);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var vacancy = await _vacancyRepository.GetByIdAsync(id);

            if (vacancy == null)
                return false;
            _vacancyRepository.Delete(vacancy);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeactivateAsync(Guid id)
        {
            var vacancy = await _vacancyRepository.GetByIdAsync(id);

            if (vacancy == null)
                return false;

            vacancy.Deactivate();

            _vacancyRepository.Update(vacancy);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ActivateAsync(Guid id)
        {
            var vacancy = await _vacancyRepository.GetByIdAsync(id);

            if (vacancy == null)
                return false;

            vacancy.Activate();

            _vacancyRepository.Update(vacancy);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}

