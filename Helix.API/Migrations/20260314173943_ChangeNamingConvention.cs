using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Helix.API.Migrations
{
    /// <inheritdoc />
    public partial class ChangeNamingConvention : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GenomicJobs");

            migrationBuilder.CreateTable(
                name: "SmithWatermanAlignmentJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SequenceA = table.Column<string>(type: "text", nullable: false),
                    SequenceB = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    FinalScore = table.Column<int>(type: "integer", nullable: true),
                    AlignedSequenceA = table.Column<string>(type: "text", nullable: true),
                    AlignedSequenceB = table.Column<string>(type: "text", nullable: true),
                    IdentityPercentage = table.Column<double>(type: "double precision", nullable: true),
                    Matches = table.Column<int>(type: "integer", nullable: true),
                    Mismatches = table.Column<int>(type: "integer", nullable: true),
                    Gaps = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmithWatermanAlignmentJobs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SmithWatermanAlignmentJobs");

            migrationBuilder.CreateTable(
                name: "GenomicJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AlignedSequenceA = table.Column<string>(type: "text", nullable: true),
                    AlignedSequenceB = table.Column<string>(type: "text", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FinalScore = table.Column<int>(type: "integer", nullable: true),
                    Gaps = table.Column<int>(type: "integer", nullable: true),
                    IdentityPercentage = table.Column<double>(type: "double precision", nullable: true),
                    Matches = table.Column<int>(type: "integer", nullable: true),
                    Mismatches = table.Column<int>(type: "integer", nullable: true),
                    SequenceA = table.Column<string>(type: "text", nullable: false),
                    SequenceB = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GenomicJobs", x => x.Id);
                });
        }
    }
}
