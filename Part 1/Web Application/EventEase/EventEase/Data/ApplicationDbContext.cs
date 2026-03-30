using Microsoft.EntityFrameworkCore;
using EventEase.Models;

namespace EventEase.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Venue> Venues { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the relationship between Booking and Venue
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Venue)
                .WithMany(v => v.Bookings)
                .HasForeignKey(b => b.VenueId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            // Configure the relationship between Booking and Event
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Event)
                .WithMany(e => e.Bookings)
                .HasForeignKey(b => b.EventId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            // Add index for faster queries
            modelBuilder.Entity<Booking>()
                .HasIndex(b => new { b.VenueId, b.BookingDate })
                .IsUnique(false);

            // Seed some initial data for testing
            modelBuilder.Entity<Venue>().HasData(
                new Venue { VenueId = 1, VenueName = "Grand Hall", Location = "Downtown", Capacity = 500, ImageUrl = "https://via.placeholder.com/300x200?text=Grand+Hall" },
                new Venue { VenueId = 2, VenueName = "Garden Pavilion", Location = "Central Park", Capacity = 200, ImageUrl = "https://via.placeholder.com/300x200?text=Garden+Pavilion" },
                new Venue { VenueId = 3, VenueName = "Conference Center", Location = "Business District", Capacity = 300, ImageUrl = "https://via.placeholder.com/300x200?text=Conference+Center" }
            );

            modelBuilder.Entity<Event>().HasData(
                new Event { EventId = 1, EventName = "Wedding Celebration", Description = "A beautiful wedding ceremony", StartDate = DateTime.Now.AddDays(30), EndDate = DateTime.Now.AddDays(30).AddHours(6), ImageUrl = "https://via.placeholder.com/300x200?text=Wedding" },
                new Event { EventId = 2, EventName = "Corporate Conference", Description = "Annual tech conference", StartDate = DateTime.Now.AddDays(45), EndDate = DateTime.Now.AddDays(47), ImageUrl = "https://via.placeholder.com/300x200?text=Conference" },
                new Event { EventId = 3, EventName = "Birthday Party", Description = "50th birthday celebration", StartDate = DateTime.Now.AddDays(15), EndDate = DateTime.Now.AddDays(15).AddHours(4), ImageUrl = "https://via.placeholder.com/300x200?text=Birthday" }
            );
        }
    }
}