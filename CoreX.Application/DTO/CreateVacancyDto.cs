using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreX.Application.DTO
{
    public class CreateVacancyDto
    {
        public Guid ClubId { get; set; }

        public string Title { get; set; } = default!;

        public string Description { get; set; } = default!;

        public string Requirements { get; set; } = default!;

        public decimal? Salary { get; set; }
    }
}
