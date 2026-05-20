using CoreX.Application.DTO;
using CoreX.Domain.Entities;

namespace CoreX.Application.Mappers
{
    public static class VacancyApplicationMapper
    {
        public static VacancyApplicationResponseDto ToDto(VacancyApplication app)
        {
            return new VacancyApplicationResponseDto
            {
                Id = app.Id,
                VacancyId = app.VacancyId,
                VacancyTitle = app.Vacancy?.Title,

                ApplicantId = app.ApplicantId,

                FullName = app.FullName,
                Email = app.Email,
                Phone = app.Phone,

                Experience = app.Experience,
                Message = app.Message,
                CVLink = app.CVLink,

                Status = app.Status.ToString(),
                CreatedAt = app.CreatedAt
            };
        }
    }
}
