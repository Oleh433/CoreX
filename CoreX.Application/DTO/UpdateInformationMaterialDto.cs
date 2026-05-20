namespace CoreX.Application.DTO
{
    public class UpdateInformationMaterialDto
    {
        public string Title { get; set; } = default!;

        public string Body { get; set; } = default!;

        public string? Category { get; set; }
    }
}
