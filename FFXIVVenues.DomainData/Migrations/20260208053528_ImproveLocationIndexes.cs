using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FFXIVVenues.DomainData.Migrations
{
    /// <inheritdoc />
    public partial class ImproveLocationIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "Address",
                schema: "Venues",
                table: "Locations");

            migrationBuilder.CreateIndex(
                name: "DCAddress",
                schema: "Venues",
                table: "Locations",
                columns: new[] { "DataCenter", "World", "District", "Ward", "Plot", "Apartment", "Room", "Subdivision" });

            migrationBuilder.CreateIndex(
                name: "WorldAddress",
                schema: "Venues",
                table: "Locations",
                columns: new[] { "World", "District", "Ward", "Plot", "Apartment", "Room", "Subdivision" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "DCAddress",
                schema: "Venues",
                table: "Locations");

            migrationBuilder.DropIndex(
                name: "WorldAddress",
                schema: "Venues",
                table: "Locations");

            migrationBuilder.CreateIndex(
                name: "Address",
                schema: "Venues",
                table: "Locations",
                columns: new[] { "DataCenter", "World", "District", "Ward", "Plot", "Apartment", "Room" });
        }
    }
}
