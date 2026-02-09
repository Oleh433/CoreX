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

            builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DatabaseConnectionString"));
            });

            var app = builder.Build();

            app.Run();
        }
    }
}
