using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FFXIVVenues.DomainData.Migrations
{
    /// <inheritdoc />
    public partial class AddFavorites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VenueViews",
                schema: "VenueMetrics");

            migrationBuilder.EnsureSchema(
                name: "Patronage");

            migrationBuilder.AddColumn<int>(
                name: "Broadcasted",
                schema: "Venues",
                table: "Opening",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Favorite",
                schema: "Patronage",
                columns: table => new
                {
                    UserId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    VenueId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Favorite", x => new { x.UserId, x.VenueId });
                    table.ForeignKey(
                        name: "FK_Favorite_Venues_VenueId",
                        column: x => x.VenueId,
                        principalSchema: "Venues",
                        principalTable: "Venues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Favorite_UserId",
                schema: "Patronage",
                table: "Favorite",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Favorite_VenueId",
                schema: "Patronage",
                table: "Favorite",
                column: "VenueId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Favorite",
                schema: "Patronage");

            migrationBuilder.DropColumn(
                name: "Broadcasted",
                schema: "Venues",
                table: "Opening");

            migrationBuilder.EnsureSchema(
                name: "VenueMetrics");

            migrationBuilder.CreateTable(
                name: "VenueViews",
                schema: "VenueMetrics",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    VenueId = table.Column<string>(type: "text", nullable: false),
                    At = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VenueViews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VenueViews_Venues_VenueId",
                        column: x => x.VenueId,
                        principalSchema: "Venues",
                        principalTable: "Venues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VenueViews_VenueId",
                schema: "VenueMetrics",
                table: "VenueViews",
                column: "VenueId");
        }
    }
}
