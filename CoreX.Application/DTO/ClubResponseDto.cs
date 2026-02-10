namespace CoreX.Application.DTO
{
    public class ClubResponseDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = default!;

        public string City { get; set; } = default!;

        public string Address { get; set; } = default!;

        public string? Description { get; set; }

        public string? Phone { get; set; }

        public string? Email { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }
    }
}
