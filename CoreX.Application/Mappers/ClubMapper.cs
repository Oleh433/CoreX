using CoreX.Domain.Entities;
using CoreX.Application.DTO;

namespace CoreX.Application.Mappers
{
    public static class ClubMapper
    {
        public static ClubResponseDto ToDto(Club club)
        {
            return new ClubResponseDto
            {
                Id = club.Id,
                Name = club.Name,
                City = club.City,
                Address = club.Address,
                Description = club.Description,
                Phone = club.Phone,
                Email = club.Email,
                Latitude = club.Latitude,
                Longitude = club.Longitude
            };
        }
    }
}
