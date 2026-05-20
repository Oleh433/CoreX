using System.ComponentModel.DataAnnotations;

namespace CoreX.Domain.Entities
{
    public class InformationMaterial
    {
        [Key]
        public Guid Id { get; private set; }

        [Required]
        [StringLength(200, MinimumLength = 2)]
        public string Title { get; private set; } = default!;

        [Required]
        public string Body { get; private set; } = default!;

        [StringLength(80)]
        public string? Category { get; private set; }

        [Required]
        public DateTime CreatedAt { get; private set; }

        public DateTime? UpdatedAt { get; private set; }

        protected InformationMaterial() { }

        public InformationMaterial(string title, string body, string? category = null)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title is required.");

            if (string.IsNullOrWhiteSpace(body))
                throw new ArgumentException("Body is required.");

            Id = Guid.NewGuid();
            Title = title.Trim();
            Body = body.Trim();
            Category = category?.Trim();
            CreatedAt = DateTime.UtcNow;
        }

        public void Update(string title, string body, string? category)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title is required.");

            if (string.IsNullOrWhiteSpace(body))
                throw new ArgumentException("Body is required.");

            Title = title.Trim();
            Body = body.Trim();
            Category = category?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
