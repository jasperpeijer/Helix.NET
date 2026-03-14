using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Helix.API.Migrations
{
    /// <inheritdoc />
    public partial class AddMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Gaps",
                table: "GenomicJobs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "IdentityPercentage",
                table: "GenomicJobs",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Matches",
                table: "GenomicJobs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Mismatches",
                table: "GenomicJobs",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Gaps",
                table: "GenomicJobs");

            migrationBuilder.DropColumn(
                name: "IdentityPercentage",
                table: "GenomicJobs");

            migrationBuilder.DropColumn(
                name: "Matches",
                table: "GenomicJobs");

            migrationBuilder.DropColumn(
                name: "Mismatches",
                table: "GenomicJobs");
        }
    }
}
