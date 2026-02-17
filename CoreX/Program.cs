using CoreX.Application.ServiceInterfaces;
using CoreX.Application.Services;
using CoreX.Domain;
using CoreX.Domain.IdentityEntities;
using CoreX.Domain.RepositoryInterfaces;
using CoreX.Infrastructure;
using CoreX.Infrastructure.Identity;
using CoreX.Infrastructure.Persistence;
using CoreX.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CoreX.Infrastructure.Identity;

namespace CoreX
{
    public class Program
    {
        public static async Task Main(string[] args)
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


            builder.Services.AddScoped<IdentityInitializer>();

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DatabaseConnectionString"));
            });

            builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders()
                        .AddUserStore<UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, Guid>>()
                            .AddRoleStore<RoleStore<ApplicationRole, ApplicationDbContext, Guid>>();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var initializer = scope.ServiceProvider.GetRequiredService<IdentityInitializer>();
                await initializer.CreateRolesAsync();
                await initializer.AddOwnerAsync();
            }

            app.UseRouting();

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
