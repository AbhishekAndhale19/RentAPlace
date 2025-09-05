using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace RentAPlace.Application.Migrations
{
    public partial class FixAdminSeed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "FullName", "Email", "PasswordHash", "Role", "ResetToken", "ResetTokenExpires" },
                values: new object[]
                {
                    new Guid("11111111-1111-1111-1111-111111111111"),
                    "Admin",
                    "admin@gmail.com",
                    "$2a$12$KIXQJ0dFvU2kpXkFq6X8EOrCmQojJqD4bF/DWakxH0hkQGpH3g/6.",
                    2, // UserRole.Admin
                    null,
                    null
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));
        }
    }
}
