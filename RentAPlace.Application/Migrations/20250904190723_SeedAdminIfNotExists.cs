using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace RentAPlace.Application.Migrations
{
    public partial class SeedAdminIfNotExists : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Safe SQL to insert admin only if not exists
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'admin@gmail.com')
BEGIN
    INSERT INTO Users (Id, FullName, Email, PasswordHash, Role, ResetToken, ResetTokenExpires)
    VALUES (
        '11111111-1111-1111-1111-111111111111',
        'Admin',
        'admin@gmail.com',
        '$2a$12$KIXQJ0dFvU2kpXkFq6X8EOrCmQojJqD4bF/DWakxH0hkQGpH3g/6.',
        2,  -- Admin
        NULL,
        NULL
    )
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM Users WHERE Id = '11111111-1111-1111-1111-111111111111';
");
        }
    }
}
