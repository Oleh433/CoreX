namespace CoreX.Application.DTO
{
    public class UpdateClubDto
    {
        public string Name { get; set; } = default!;

        public string City { get; set; } = default!;

        public string Address { get; set; } = default!;

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public string? Description { get; set; }

        public string? Phone { get; set; }

        public string? Email { get; set; }

        public string? WorkingHours { get; set; }

        public string? PhotoUrl { get; set; }
    }
}
