using CoreX.Application.DTO;
using CoreX.Domain.Entities;

namespace CoreX.Application.Mappers
{
    public static class DiscountMapper
    {
        public static DiscountResponseDto ToDto(Discount discount)
        {
            return new DiscountResponseDto
            {
                Id = discount.Id,
                Title = discount.Title,
                Description = discount.Description,
                DiscountPercent = discount.DiscountPercent,
                Conditions = discount.Conditions,
                StartDate = discount.StartDate,
                EndDate = discount.EndDate,
                IsActive = discount.IsActive
            };
        }
    }
}
