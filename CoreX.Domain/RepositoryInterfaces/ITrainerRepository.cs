using CoreX.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreX.Domain.RepositoryInterfaces
{
    public interface ITrainerRepository
    {
        Task<Trainer?> GetByIdAsync(Guid id);

        Task<List<Trainer>> GetAllAsync();

        Task<List<Trainer>> GetByClubIdAsync(Guid clubId);

        Task AddAsync(Trainer trainer);

        void Update(Trainer trainer);

        void Delete(Trainer trainer);
    }
}
