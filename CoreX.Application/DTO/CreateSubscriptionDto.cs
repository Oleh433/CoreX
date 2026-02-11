namespace CoreX.Application.DTO
{
    public class CreateSubscriptionDto
    {
        public Guid ClubId { get; set; }

        public string Title { get; set; } = default!;

        public decimal Price { get; set; }

        public int DurationDays { get; set; }

        public int? VisitsLimit { get; set; }

        public string? Description { get; set; }
    }
}
