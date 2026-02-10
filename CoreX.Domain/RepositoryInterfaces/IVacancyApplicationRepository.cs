using CoreX.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreX.Domain.RepositoryInterfaces
{
    public interface IVacancyApplicationRepository
    {
        Task<VacancyApplication?> GetByIdAsync(Guid id);

        Task<List<VacancyApplication>> GetAllAsync();

        Task<List<VacancyApplication>> GetByVacancyIdAsync(Guid vacancyId);

        Task<List<VacancyApplication>> GetByApplicantIdAsync(Guid applicantId);

        Task AddAsync(VacancyApplication application);

        void Update(VacancyApplication application);

        void Delete(VacancyApplication application);
    }
}
