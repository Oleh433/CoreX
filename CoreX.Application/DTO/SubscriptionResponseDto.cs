namespace CoreX.Application.DTO
{
    public class SubscriptionResponseDto
    {
        public Guid Id { get; set; }

        public Guid ClubId { get; set; }

        public string Title { get; set; } = default!;

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public int DurationDays { get; set; }

        public int? VisitsLimit { get; set; }

        public bool IsActive { get; set; }
    }
}
