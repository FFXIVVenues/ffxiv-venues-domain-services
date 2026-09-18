using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FFXIVVenues.DomainData.Migrations
{
    /// <inheritdoc />
    public partial class AddOpeningsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Opening",
                schema: "Venues",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    VenueId = table.Column<string>(type: "text", nullable: false),
                    Start = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    End = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Opening", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Opening_Start",
                schema: "Venues",
                table: "Opening",
                column: "Start");

            migrationBuilder.CreateIndex(
                name: "IX_Opening_VenueId",
                schema: "Venues",
                table: "Opening",
                column: "VenueId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Opening",
                schema: "Venues");
        }
    }
}
