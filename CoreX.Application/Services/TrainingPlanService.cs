using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;

namespace CoreX.Application.Services
{
    public class TrainingPlanService : ITrainingPlanService
    {
        private static readonly string[] WeekDays =
        {
            "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"
        };

        public TrainingPlanResponseDto Generate(TrainingPlanRequestDto request)
        {
            Validate(request);

            var sessionsPerWeek = Math.Clamp(request.SessionsPerWeek, 1, 7);

            var dayIndexes = DistributeDays(sessionsPerWeek);
            var duration = DurationMinutes(request.Level);
            var time = SuggestedTime(request.Goal);
            var workouts = WorkoutsFor(request.Goal, request.Level);

            var sessions = new List<TrainingSessionDto>(sessionsPerWeek);

            for (int i = 0; i < dayIndexes.Count; i++)
            {
                var workout = workouts[i % workouts.Count];

                sessions.Add(new TrainingSessionDto
                {
                    Day = WeekDays[dayIndexes[i]],
                    Time = time,
                    Title = workout.title,
                    Description = workout.description,
                    DurationMinutes = duration
                });
            }

            return new TrainingPlanResponseDto
            {
                Sessions = sessions,
                Recommendations = BuildRecommendations(request)
            };
        }

        private static void Validate(TrainingPlanRequestDto request)
        {
            if (request.Age < 14 || request.Age > 90)
                throw new ArgumentException("Age must be between 14 and 90.");

            if (request.WeightKg < 30 || request.WeightKg > 300)
                throw new ArgumentException("WeightKg must be between 30 and 300.");

            if (request.HeightCm < 120 || request.HeightCm > 230)
                throw new ArgumentException("HeightCm must be between 120 and 230.");

            if (request.SessionsPerWeek < 1 || request.SessionsPerWeek > 7)
                throw new ArgumentException("SessionsPerWeek must be between 1 and 7.");
        }

        private static List<int> DistributeDays(int sessions)
        {
            var step = 7.0 / sessions;
            var days = new List<int>(sessions);

            for (int i = 0; i < sessions; i++)
            {
                days.Add((int)Math.Round(i * step) % 7);
            }

            return days;
        }

        private static int DurationMinutes(FitnessLevel level) => level switch
        {
            FitnessLevel.Beginner => 45,
            FitnessLevel.Intermediate => 60,
            FitnessLevel.Advanced => 75,
            _ => 60
        };

        private static string SuggestedTime(FitnessGoal goal) => goal switch
        {
            FitnessGoal.WeightLoss => "07:30",
            FitnessGoal.MuscleGain => "18:00",
            FitnessGoal.Endurance => "08:00",
            _ => "18:30"
        };

        private static List<(string title, string description)> WorkoutsFor(FitnessGoal goal, FitnessLevel level)
        {
            return goal switch
            {
                FitnessGoal.WeightLoss => new()
                {
                    ("HIIT cardio", "20 min interval cardio + core circuit"),
                    ("Full-body strength", "Compound lifts, moderate weight, 12-15 reps"),
                    ("Steady-state cardio", "Cycling or jogging at 65% max HR"),
                    ("Mobility + core", "Yoga flow with planks and twists")
                },
                FitnessGoal.MuscleGain => new()
                {
                    ("Push day", "Bench, overhead press, triceps"),
                    ("Pull day", "Rows, pull-ups, biceps"),
                    ("Leg day", "Squats, Romanian deadlifts, lunges"),
                    ("Accessory + core", "Isolation work + weighted plank work")
                },
                FitnessGoal.Endurance => new()
                {
                    ("Long aerobic", "Zone-2 cardio 60-90 min"),
                    ("Tempo run/ride", "Sustained effort at lactate threshold"),
                    ("Intervals", "8x400m or 6x3min hard"),
                    ("Strength support", "Bodyweight + core, low-volume legs")
                },
                _ => new()
                {
                    ("Full-body strength", "8-10 compound exercises, moderate intensity"),
                    ("Mixed cardio", "30-40 min varied cardio"),
                    ("Mobility + core", "Stretching, foam roll, planks"),
                    ("Functional circuit", "Kettlebell + bodyweight circuit")
                }
            };
        }

        private static List<string> BuildRecommendations(TrainingPlanRequestDto request)
        {
            var recs = new List<string>
            {
                "Warm up 5-10 minutes before each session and cool down with stretching.",
                "Aim for 7-9 hours of sleep and at least 2L of water per day."
            };

            if (request.Goal == FitnessGoal.WeightLoss)
                recs.Add("Maintain a moderate calorie deficit (~10-20%) and prioritise protein intake.");

            if (request.Goal == FitnessGoal.MuscleGain)
                recs.Add("Eat in a slight surplus and consume 1.6-2.2g of protein per kg bodyweight.");

            if (request.Level == FitnessLevel.Beginner)
                recs.Add("Focus on form before adding load. Start with the lower end of the rep range.");

            if (request.Age >= 50)
                recs.Add("Add an extra mobility / recovery day and consider a medical check-up.");

            var bmi = request.WeightKg / Math.Pow(request.HeightCm / 100.0, 2);

            if (bmi >= 30)
                recs.Add("Start with low-impact cardio (cycling, swimming) before adding running.");

            return recs;
        }
    }
}
