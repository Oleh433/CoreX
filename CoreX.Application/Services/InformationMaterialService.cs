using CoreX.Application.DTO;
using CoreX.Application.Mappers;
using CoreX.Application.ServiceInterfaces;
using CoreX.Domain;
using CoreX.Domain.Entities;
using CoreX.Domain.RepositoryInterfaces;

namespace CoreX.Application.Services
{
    public class InformationMaterialService : IInformationMaterialService
    {
        private readonly IInformationMaterialRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public InformationMaterialService(
            IInformationMaterialRepository repository,
            IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<InformationMaterialResponseDto?> GetByIdAsync(Guid id)
        {
            var material = await _repository.GetByIdAsync(id);

            if (material == null)
                return null;

            return InformationMaterialMapper.ToDto(material);
        }

        public async Task<List<InformationMaterialResponseDto>> GetAllAsync()
        {
            var materials = await _repository.GetAllAsync();

            return materials.Select(InformationMaterialMapper.ToDto).ToList();
        }

        public async Task<Guid> CreateAsync(CreateInformationMaterialDto dto)
        {
            var material = new InformationMaterial(dto.Title, dto.Body, dto.Category);

            await _repository.AddAsync(material);

            await _unitOfWork.SaveChangesAsync();

            return material.Id;
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateInformationMaterialDto dto)
        {
            var material = await _repository.GetByIdAsync(id);

            if (material == null)
                return false;

            material.Update(dto.Title, dto.Body, dto.Category);

            _repository.Update(material);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var material = await _repository.GetByIdAsync(id);

            if (material == null)
                return false;

            _repository.Delete(material);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
