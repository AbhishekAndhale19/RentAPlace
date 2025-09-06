using Microsoft.EntityFrameworkCore;
using RentAPlace.Domain.Models;

namespace RentAPlace.Domain.Models
{
    public class RentAPlaceDbContext : DbContext
    {
        public RentAPlaceDbContext(DbContextOptions<RentAPlaceDbContext> options)
           : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Property> Properties { get; set; }

        public DbSet<PropertyImage> PropertyImages { get; set; }
        public DbSet<Reservation> Reservations { get; set; }

        public DbSet<Message> Messages { get; set; }

        public DbSet<Rating> Ratings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relations
            modelBuilder.Entity<Property>()
                .HasOne(p => p.Owner)
                .WithMany(u => u.Properties)
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PropertyImage>()
                .HasOne(pi => pi.Property)
                .WithMany(p => p.Images)
                .HasForeignKey(pi => pi.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Property)
                .WithMany()
                .HasForeignKey(r => r.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Property>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            // Message relationships
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.MessagesSent)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany(u => u.MessagesReceived)
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Property)
                .WithMany()
                .HasForeignKey(m => m.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            //ratings

            modelBuilder.Entity<Rating>()
    .HasOne(r => r.Property)
    .WithMany(p => p.Ratings)
    .HasForeignKey(r => r.PropertyId)
     .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Rating>()
                .HasOne(r => r.User)
                .WithMany(u => u.Ratings)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed admin user
            var adminId = new Guid("11111111-1111-1111-1111-111111111111");
            var adminPasswordHash = "$2a$12$KIXQJ0dFvU2kpXkFq6X8EOrCmQojJqD4bF/DWakxH0hkQGpH3g/6."; // "Admin@123"

            modelBuilder.Entity<User>().HasData(new User
            {
                Id = adminId,
                FullName = "Admin",
                Email = "admin@gmail.com",
                PasswordHash = adminPasswordHash,
                Role = UserRole.Admin,
                ResetToken = null,
                ResetTokenExpires = null
            });
        }
    }
}
