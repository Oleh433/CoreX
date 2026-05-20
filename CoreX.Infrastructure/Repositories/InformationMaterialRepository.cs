using CoreX.Domain.Entities;
using CoreX.Domain.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreX.Infrastructure.Repositories
{
    public class InformationMaterialRepository : IInformationMaterialRepository
    {
        private readonly ApplicationDbContext _context;

        public InformationMaterialRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<InformationMaterial?> GetByIdAsync(Guid id)
        {
            return await _context.InformationMaterials
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<InformationMaterial>> GetAllAsync()
        {
            return await _context.InformationMaterials
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(InformationMaterial material)
        {
            await _context.InformationMaterials.AddAsync(material);
        }

        public void Update(InformationMaterial material)
        {
            _context.InformationMaterials.Update(material);
        }

        public void Delete(InformationMaterial material)
        {
            _context.InformationMaterials.Remove(material);
        }
    }
}
