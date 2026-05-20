using CoreX.Application.DTO;
using CoreX.Application.Mappers;
using CoreX.Application.ServiceInterfaces;
using CoreX.Domain;
using CoreX.Domain.Entities;
using CoreX.Domain.RepositoryInterfaces;

namespace CoreX.Application.Services
{
    public class VacancyApplicationService : IVacancyApplicationService
    {
        private readonly IVacancyApplicationRepository _applicationRepository;
        private readonly IVacancyRepository _vacancyRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;

        public VacancyApplicationService(
            IVacancyApplicationRepository applicationRepository,
            IVacancyRepository vacancyRepository,
            IUnitOfWork unitOfWork,
            IEmailSender emailSender)
        {
            _applicationRepository = applicationRepository;
            _vacancyRepository = vacancyRepository;
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
        }

        public async Task<List<VacancyApplicationResponseDto>> GetAllAsync()
        {
            var apps = await _applicationRepository.GetAllAsync();

            return apps
                .Select(VacancyApplicationMapper.ToDto)
                .ToList();
        }

        public async Task<VacancyApplicationResponseDto?> GetByIdAsync(Guid id)
        {
            var app = await _applicationRepository.GetByIdAsync(id);

            if (app == null)
                return null;

            return VacancyApplicationMapper.ToDto(app);
        }

        public async Task<List<VacancyApplicationResponseDto>> GetByVacancyIdAsync(Guid vacancyId)
        {
            var apps = await _applicationRepository.GetByVacancyIdAsync(vacancyId);

            return apps
                .Select(VacancyApplicationMapper.ToDto)
                .ToList();
        }

        public async Task<List<VacancyApplicationResponseDto>> GetByApplicantIdAsync(Guid applicantId)
        {
            var apps = await _applicationRepository.GetByApplicantIdAsync(applicantId);

            return apps
                .Select(VacancyApplicationMapper.ToDto)
                .ToList();
        }

        public async Task<Guid> ApplyAsync(CreateVacancyApplicationDto dto, Guid? applicantId = null)
        {
            var vacancy = await _vacancyRepository.GetByIdAsync(dto.VacancyId);

            if (vacancy == null)
                throw new KeyNotFoundException("Vacancy not found.");

            if (!vacancy.IsActive)
                throw new InvalidOperationException("Vacancy is not active.");

            var application = new VacancyApplication(
                vacancyId: dto.VacancyId,
                fullName: dto.FullName,
                email: dto.Email,
                phone: dto.Phone,
                experience: dto.Experience,
                applicantId: applicantId,
                message: dto.Message,
                cvLink: dto.CVLink
            );

            await _applicationRepository.AddAsync(application);

            await _unitOfWork.SaveChangesAsync();

            await _emailSender.SendAsync(
                application.Email,
                "Application received",
                $"Hello {application.FullName}, we have received your application for '{vacancy.Title}'. We will be in touch.");

            return application.Id;
        }

        public async Task<bool> ChangeStatusAsync(Guid id, ChangeVacancyApplicationStatusDto dto)
        {
            var application = await _applicationRepository.GetByIdAsync(id);

            if (application == null)
                return false;

            if (!Enum.IsDefined(typeof(VacancyApplicationStatus), dto.Status))
                throw new ArgumentException("Invalid status value.");

            var newStatus = dto.Status;

            application.ChangeStatus(newStatus);

            _applicationRepository.Update(application);

            await _unitOfWork.SaveChangesAsync();

            if (newStatus == VacancyApplicationStatus.Accepted || newStatus == VacancyApplicationStatus.Rejected)
            {
                var verb = newStatus == VacancyApplicationStatus.Accepted ? "accepted" : "rejected";

                await _emailSender.SendAsync(
                    application.Email,
                    $"Application {verb}",
                    $"Hello {application.FullName}, your application has been {verb}.");
            }

            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var application = await _applicationRepository.GetByIdAsync(id);

            if (application == null)
                return false;

            _applicationRepository.Delete(application);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
