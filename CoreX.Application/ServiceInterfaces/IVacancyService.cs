using CoreX.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreX.Application.ServiceInterfaces
{
    public interface IVacancyService
    {
        Task<List<VacancyResponseDto>> GetAllAsync();

        Task<List<VacancyResponseDto>> GetActiveAsync();

        Task<List<VacancyResponseDto>> GetByClubIdAsync(Guid clubId);

        Task<VacancyResponseDto?> GetByIdAsync(Guid id);

        Task<Guid> CreateAsync(CreateVacancyDto dto);

        Task<bool> UpdateAsync(Guid id, UpdateVacancyDto dto);

        Task<bool> DeleteAsync(Guid id);

        Task<bool> DeactivateAsync(Guid id);

        Task<bool> ActivateAsync(Guid id);
    }
}
