using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreX.Domain.Entities
{
    public enum GroupClassAudience
    {
        Adults = 0,
        Kids = 1
    }

    public class GroupClass
    {
        [Key]
        public Guid Id { get; private set; }

        [Required]
        public Guid ClubId { get; private set; }

        [ForeignKey(nameof(ClubId))]
        public Club? Club { get; private set; }

        public Guid? TrainerId { get; private set; }

        [ForeignKey(nameof(TrainerId))]
        public Trainer? Trainer { get; private set; }

        [Required]
        [StringLength(80, MinimumLength = 2)]
        public string Type { get; private set; } = default!;

        [StringLength(500)]
        public string? Description { get; private set; }

        [Required]
        public GroupClassAudience Audience { get; private set; }

        [Required]
        public DateTime StartTime { get; private set; }

        [Required]
        [Range(15, 480)]
        public int DurationMinutes { get; private set; }

        [Required]
        [Range(1, 200)]
        public int Capacity { get; private set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 100000)]
        public decimal? Price { get; private set; }

        protected GroupClass() { }

        public GroupClass(
            Guid clubId,
            string type,
            GroupClassAudience audience,
            DateTime startTime,
            int durationMinutes,
            int capacity,
            Guid? trainerId = null,
            decimal? price = null,
            string? description = null)
        {
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Type is required.");

            if (durationMinutes <= 0)
                throw new ArgumentException("DurationMinutes must be greater than 0.");

            if (capacity <= 0)
                throw new ArgumentException("Capacity must be greater than 0.");

            Id = Guid.NewGuid();

            ClubId = clubId;
            TrainerId = trainerId;
            Type = type.Trim();
            Audience = audience;
            StartTime = startTime;
            DurationMinutes = durationMinutes;
            Capacity = capacity;
            Price = price;
            Description = description;
        }

        public void Update(
            string type,
            GroupClassAudience audience,
            DateTime startTime,
            int durationMinutes,
            int capacity,
            Guid? trainerId,
            decimal? price,
            string? description)
        {
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Type is required.");

            if (durationMinutes <= 0)
                throw new ArgumentException("DurationMinutes must be greater than 0.");

            if (capacity <= 0)
                throw new ArgumentException("Capacity must be greater than 0.");

            Type = type.Trim();
            Audience = audience;
            StartTime = startTime;
            DurationMinutes = durationMinutes;
            Capacity = capacity;
            TrainerId = trainerId;
            Price = price;
            Description = description;
        }
    }
}
