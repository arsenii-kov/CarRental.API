using Microsoft.EntityFrameworkCore;
using CarRental.API.Models;

namespace CarRental.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Car> Cars { get; set; }                
        public DbSet<Booking> Bookings { get; set; }
    }
}
