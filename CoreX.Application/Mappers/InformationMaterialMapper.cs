using CoreX.Application.DTO;
using CoreX.Domain.Entities;

namespace CoreX.Application.Mappers
{
    public static class InformationMaterialMapper
    {
        public static InformationMaterialResponseDto ToDto(InformationMaterial material)
        {
            return new InformationMaterialResponseDto
            {
                Id = material.Id,
                Title = material.Title,
                Body = material.Body,
                Category = material.Category,
                CreatedAt = material.CreatedAt,
                UpdatedAt = material.UpdatedAt
            };
        }
    }
}
