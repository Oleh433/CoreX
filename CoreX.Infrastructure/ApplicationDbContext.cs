using Microsoft.EntityFrameworkCore;
using CoreX.Domain.Entities;

namespace CoreX.Infrastructure
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Club> Clubs { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<Membership> Memberships { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Trainer> Trainers { get; set; }
        public DbSet<Vacancy> Vacancies { get; set; }
        public DbSet<VacancyApplication> VacancyApplications { get; set; }

        public ApplicationDbContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Booking>().ToTable("Bookings");
            modelBuilder.Entity<Club>().ToTable("Clubs");
            modelBuilder.Entity<Discount>().ToTable("Discounts");
            modelBuilder.Entity<Membership>().ToTable("Memberships");
            modelBuilder.Entity<Subscription>().ToTable("Subscriptions");
            modelBuilder.Entity<Trainer>().ToTable("Trainers");
            modelBuilder.Entity<Vacancy>().ToTable("Vacancies");
            modelBuilder.Entity<VacancyApplication>().ToTable("VacancyApplications");
        }
    }
}
