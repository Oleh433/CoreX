namespace CoreX.Application.DTO
{
    public enum Gender
    {
        Male = 0,
        Female = 1,
        Other = 2
    }

    public enum FitnessLevel
    {
        Beginner = 0,
        Intermediate = 1,
        Advanced = 2
    }

    public enum FitnessGoal
    {
        WeightLoss = 0,
        MuscleGain = 1,
        Endurance = 2,
        GeneralFitness = 3
    }

    public class TrainingPlanRequestDto
    {
        public Gender Gender { get; set; }

        public int Age { get; set; }

        public double WeightKg { get; set; }

        public double HeightCm { get; set; }

        public FitnessLevel Level { get; set; }

        public FitnessGoal Goal { get; set; }

        public int SessionsPerWeek { get; set; }
    }
}
