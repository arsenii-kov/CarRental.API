using Microsoft.EntityFrameworkCore;
using CarRental.API.Models;

namespace CarRental.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Car> Cars { get; set; }                
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            var initialCars = new Car[]
            {
                new Car
                {
                    Id = 1,
                    Brand = "Toyota",
                    Model = "Camry",
                    Year = 2022,
                    DailyPrice = 45.0m,
                    IsAvailable = true
                },
                new Car
                {
                    Id = 2,
                    Brand = "BMW",
                    Model = "320i",
                    Year = 2023,
                    DailyPrice = 75.0m,
                    IsAvailable = true
                },
                new Car
                {
                    Id = 3,
                    Brand = "Tesla",
                    Model = "Model 3",
                    Year = 2023,
                    DailyPrice = 85.0m,
                    IsAvailable = true
                }
            };
            modelBuilder.Entity<Car>().HasData(initialCars);

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    FullName = "Admin",
                    Email = "admin@carrental.com",
                    PasswordHash = "$2b$10$cRMoEQLHvqs4D4IkaWxnf.DcYnzBEjlyF/kVpwOEGIVKQOunKEQWm",
                    Role = UserRole.Admin,
                }
            );
        }

    }
}
