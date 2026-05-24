using CoreX.Application.ServiceInterfaces;
using CoreX.Application.Services;
using CoreX.Domain;
using CoreX.Domain.IdentityEntities;
using CoreX.Domain.RepositoryInterfaces;
using CoreX.Infrastructure;
using CoreX.Infrastructure.Email;
using CoreX.Infrastructure.Identity;
using CoreX.Infrastructure.Persistence;
using CoreX.Infrastructure.Repositories;
using CoreX.UI.Filters;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.WebEncoders;
using System.Globalization;
using System.Text.Unicode;

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

            builder.Services.AddScoped<IGroupClassRepository, GroupClassRepository>();

            builder.Services.AddScoped<IInformationMaterialRepository, InformationMaterialRepository>();


            builder.Services.AddScoped<IClubService, ClubService>();

            builder.Services.AddScoped<IBookingService, BookingService>();

            builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();

            builder.Services.AddScoped<IMembershipService, MembershipService>();

            builder.Services.AddScoped<IDiscountService, DiscountService>();

            builder.Services.AddScoped<ITrainerService, TrainerService>();

            builder.Services.AddScoped<IVacancyService, VacancyService>();

            builder.Services.AddScoped<IVacancyApplicationService, VacancyApplicationService>();

            builder.Services.AddScoped<IGroupClassService, GroupClassService>();

            builder.Services.AddScoped<IInformationMaterialService, InformationMaterialService>();

            builder.Services.AddScoped<ITrainingPlanService, TrainingPlanService>();

            builder.Services.AddScoped<IUserService, UserService>();

            builder.Services.AddScoped<IEmailSender, ConsoleEmailSender>();


            builder.Services.AddScoped<IdentityInitializer>();

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddControllers(options =>
            {
                options.Filters.Add<GlobalExceptionFilter>();
            });

            builder.Services.AddRazorPages(options =>
            {
                options.Conventions.AuthorizeFolder("/Admin", "AdminOrOwner");
                options.Conventions.AuthorizeFolder("/Account", "AuthenticatedOnly");
                options.Conventions.AllowAnonymousToPage("/Account/Login");
                options.Conventions.AllowAnonymousToPage("/Account/Register");
                options.Conventions.AuthorizePage("/Admin/Subscriptions/Index", "OwnerOnly");
                options.Conventions.AuthorizePage("/Admin/Subscriptions/Create", "OwnerOnly");
                options.Conventions.AuthorizePage("/Admin/Subscriptions/Edit", "OwnerOnly");
                options.Conventions.AuthorizePage("/Admin/Discounts/Index", "OwnerOnly");
                options.Conventions.AuthorizePage("/Admin/Discounts/Create", "OwnerOnly");
                options.Conventions.AuthorizePage("/Admin/Discounts/Edit", "OwnerOnly");
                options.Conventions.AuthorizePage("/Admin/Users/RegisterAdmin", "OwnerOnly");
            })
            .AddViewLocalization();

            builder.Services.AddAuthorization(o =>
            {
                o.AddPolicy("AdminOrOwner",      p => p.RequireRole("Admin", "Owner"));
                o.AddPolicy("OwnerOnly",         p => p.RequireRole("Owner"));
                o.AddPolicy("AuthenticatedOnly", p => p.RequireAuthenticatedUser());
            });

            // Allow Cyrillic + Basic Latin to render unescaped in HTML output (validation
            // errors, tag-helper-emitted content). Without this, HtmlEncoder.Default
            // escapes UA letters as numeric entities (&#x41D; etc.), which breaks both
            // visual output and substring assertions in tests.
            builder.Services.Configure<WebEncoderOptions>(o =>
            {
                o.TextEncoderSettings = new System.Text.Encodings.Web.TextEncoderSettings(
                    UnicodeRanges.BasicLatin,
                    UnicodeRanges.Cyrillic);
            });

            builder.Services.AddLocalization(o => o.ResourcesPath = "Resources");
            builder.Services.Configure<RequestLocalizationOptions>(o =>
            {
                var supported = new[] { new CultureInfo("uk"), new CultureInfo("en") };
                o.DefaultRequestCulture = new RequestCulture("uk");
                o.SupportedCultures = supported;
                o.SupportedUICultures = supported;
            });

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DatabaseConnectionString"));
            });

            builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
                {
                    options.Lockout.AllowedForNewUsers = true;
                    options.Lockout.MaxFailedAccessAttempts = 5;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

                    options.Password.RequiredLength = 8;
                    options.Password.RequireDigit = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireNonAlphanumeric = false;

                    options.User.RequireUniqueEmail = true;

                    options.SignIn.RequireConfirmedEmail = false;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders()
                        .AddUserStore<UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, Guid>>()
                            .AddRoleStore<RoleStore<ApplicationRole, ApplicationDbContext, Guid>>();

            builder.Services.ConfigureApplicationCookie(o =>
            {
                o.LoginPath = "/Account/Login";
                o.AccessDeniedPath = "/Error/403";
            });

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var initializer = scope.ServiceProvider.GetRequiredService<IdentityInitializer>();
                await initializer.CreateRolesAsync();
                await initializer.AddOwnerAsync();
            }

            app.UseRouting();

            app.UseExceptionHandler("/Error");
            app.UseStatusCodePagesWithReExecute("/Error/{0}");

            app.UseStaticFiles();

            app.UseRequestLocalization();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();
            app.MapRazorPages();

            app.Run();
        }
    }
}
