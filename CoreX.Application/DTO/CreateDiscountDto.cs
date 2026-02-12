namespace CoreX.Application.DTO
{
    public class CreateDiscountDto
    {
        public string Title { get; set; } = default!;

        public string? Description { get; set; }

        public decimal? DiscountPercent { get; set; }

        public string? Conditions { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}
