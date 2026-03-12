using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProRental.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Baseline migration — all tables already exist (created via schema.sql).
            // This entry exists only to establish the EF Core migration history starting point.
            // Running: dotnet ef database update  will mark this as applied without touching the DB.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally empty — to roll back, drop and re-create the database manually.
        }
    }
}
