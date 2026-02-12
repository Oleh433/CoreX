using CoreX.Application.DTO;

namespace CoreX.Application.ServiceInterfaces
{
    public interface IVacancyApplicationService
    {
        Task<List<VacancyApplicationResponseDto>> GetAllAsync();

        Task<VacancyApplicationResponseDto?> GetByIdAsync(Guid id);

        Task<List<VacancyApplicationResponseDto>> GetByVacancyIdAsync(Guid vacancyId);

        Task<List<VacancyApplicationResponseDto>> GetByApplicantIdAsync(Guid applicantId);

        Task<Guid> ApplyAsync(CreateVacancyApplicationDto dto);

        Task<bool> ChangeStatusAsync(Guid id, ChangeVacancyApplicationStatusDto dto);

        Task<bool> DeleteAsync(Guid id);
    }
}
