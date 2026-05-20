using CoreX.Application.DTO;
using CoreX.Domain.Entities;

namespace CoreX.Application.Mappers
{
    public static class VacancyMapper
    {
        public static VacancyResponseDto ToDto(Vacancy vacancy)
        {
            return new VacancyResponseDto
            {
                Id = vacancy.Id,
                ClubId = vacancy.ClubId,
                ClubName = vacancy.Club?.Name,

                Title = vacancy.Title,
                Description = vacancy.Description,
                Requirements = vacancy.Requirements,

                Salary = vacancy.Salary,
                IsActive = vacancy.IsActive,

                CreatedAt = vacancy.CreatedAt,
                ApplicationDeadline = vacancy.ApplicationDeadline,

                ApplicationsCount = vacancy.Applications.Count
            };
        }
    }
}
