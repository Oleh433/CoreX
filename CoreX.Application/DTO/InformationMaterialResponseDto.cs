namespace CoreX.Application.DTO
{
    public class InformationMaterialResponseDto
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = default!;

        public string Body { get; set; } = default!;

        public string? Category { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
