using CoreX.Application.ServiceInterfaces;
using CoreX.Application.Services;
using CoreX.Domain;
using CoreX.Domain.RepositoryInterfaces;
using CoreX.Infrastructure;
using CoreX.Infrastructure.Persistence;
using CoreX.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CoreX
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            builder.Services.AddScoped<IClubRepository, ClubRepository>();

            builder.Services.AddScoped<IBookingRepository, BookingRepository>();

            builder.Services.AddScoped<IDiscountRepository, DiscountRepository>();

            builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();

            builder.Services.AddScoped<IVacancyApplicationRepository, VacancyApplicationRepository>();

            builder.Services.AddScoped<IVacancyRepository, VacancyRepository>();

            builder.Services.AddScoped<ITrainerRepository, TrainerRepository>();

            builder.Services.AddScoped<IMembershipRepository, MembershipRepository>();


            builder.Services.AddScoped<IClubService, ClubService>();

            builder.Services.AddScoped<IBookingService, BookingService>();

            builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();

            builder.Services.AddScoped<IMembershipService, MembershipService>();

            builder.Services.AddScoped<IDiscountService, DiscountService>();

            builder.Services.AddScoped<ITrainerService, TrainerService>();

            builder.Services.AddScoped<IVacancyService, VacancyService>();

            builder.Services.AddScoped<IVacancyApplicationService, VacancyApplicationService>();


            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DatabaseConnectionString"));
            });

            var app = builder.Build();

            app.Run();
        }
    }
}
