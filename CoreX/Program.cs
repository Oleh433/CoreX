using CoreX.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CoreX
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DatabaseConnectionString"));
            });

            var app = builder.Build();

            app.Run();
        }
    }
}
