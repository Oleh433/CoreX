using CoreX.Application.DTO;
using CoreX.Application.Mappers;
using CoreX.Application.ServiceInterfaces;
using CoreX.Domain;
using CoreX.Domain.Entities;
using CoreX.Domain.RepositoryInterfaces;

namespace CoreX.Application.Services
{
    public class DiscountService : IDiscountService
    {
        private readonly IDiscountRepository _discountRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DiscountService(
            IDiscountRepository discountRepository,
            IUnitOfWork unitOfWork)
        {
            _discountRepository = discountRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<DiscountResponseDto?> GetByIdAsync(Guid id)
        {
            var discount = await _discountRepository.GetByIdAsync(id);

            if (discount == null)
                return null;

            return DiscountMapper.ToDto(discount);
        }

        public async Task<List<DiscountResponseDto>> GetAllAsync()
        {
            var discounts = await _discountRepository.GetAllAsync();

            return discounts
                .Select(DiscountMapper.ToDto)
                .ToList();
        }

        public async Task<List<DiscountResponseDto>> GetActiveAsync()
        {
            var discounts = await _discountRepository.GetActiveAsync();

            return discounts
                .Select(DiscountMapper.ToDto)
                .ToList();
        }

        public async Task<Guid> CreateAsync(CreateDiscountDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new ArgumentException("Title is required.");

            if (dto.EndDate < dto.StartDate)
                throw new ArgumentException("EndDate must be >= StartDate.");

            var discount = new Discount(
                title: dto.Title,
                startDate: dto.StartDate,
                endDate: dto.EndDate,
                description: dto.Description,
                discountPercent: dto.DiscountPercent,
                conditions: dto.Conditions,
                promoCode: dto.PromoCode
            );

            await _discountRepository.AddAsync(discount);

            await _unitOfWork.SaveChangesAsync();

            return discount.Id;
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateDiscountDto dto)
        {
            var discount = await _discountRepository.GetByIdAsync(id);

            if (discount == null)
                return false;

            discount.Update(
                dto.Title,
                dto.StartDate,
                dto.EndDate,
                dto.Description,
                dto.DiscountPercent,
                dto.Conditions,
                dto.PromoCode,
                dto.IsActive
            );

            _discountRepository.Update(discount);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var discount = await _discountRepository.GetByIdAsync(id);

            if (discount == null)
                return false;

            _discountRepository.Delete(discount);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
