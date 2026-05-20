namespace CoreX.Application.DTO
{
    public class UpdateSubscriptionDto
    {
        public string Title { get; set; } = default!;

        public decimal Price { get; set; }

        public int DurationDays { get; set; }

        public int? VisitsLimit { get; set; }

        public string? Description { get; set; }
    }
}
