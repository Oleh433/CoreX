namespace CoreX.Application.DTO
{
    public class DiscountResponseDto
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = default!;

        public string? Description { get; set; }

        public decimal? DiscountPercent { get; set; }

        public string? Conditions { get; set; }

        public string? PromoCode { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; }
    }
}
