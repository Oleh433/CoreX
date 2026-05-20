using CoreX.Domain.Entities;

namespace CoreX.Domain.RepositoryInterfaces
{
    public interface IInformationMaterialRepository
    {
        Task<InformationMaterial?> GetByIdAsync(Guid id);

        Task<List<InformationMaterial>> GetAllAsync();

        Task AddAsync(InformationMaterial material);

        void Update(InformationMaterial material);

        void Delete(InformationMaterial material);
    }
}
