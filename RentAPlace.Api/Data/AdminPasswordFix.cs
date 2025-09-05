using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RentAPlace.Domain.Models;

namespace RentAPlace.Api
{
    public static class AdminPasswordFix
    {
        public static async Task FixAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<RentAPlaceDbContext>();

            var adminEmail = "admin@gmail.com";
            var admin = await db.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
            if (admin != null)
            {
                // Reset admin password to correct hash
                admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");
                await db.SaveChangesAsync();
                Console.WriteLine("Admin password updated successfully.");
            }
            else
            {
                Console.WriteLine("Admin user not found.");
            }
        }
    }
}
