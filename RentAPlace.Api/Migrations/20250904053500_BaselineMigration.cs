using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentAPlace.Api.Migrations
{
    /// <inheritdoc />
    public partial class BaselineMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Baseline migration – no schema changes applied.
            // Just marks current database schema as already in sync.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Nothing to rollback for baseline.
        }
    }
}
