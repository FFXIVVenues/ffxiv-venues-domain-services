using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FFXIVVenues.DomainData.Migrations
{
    /// <inheritdoc />
    public partial class AddFlagRresolution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Resolution",
                schema: "VenueFlags",
                table: "Flag",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ResolutionDate",
                schema: "VenueFlags",
                table: "Flag",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<long>(
                name: "ResolvedBy",
                schema: "VenueFlags",
                table: "Flag",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Resolution",
                schema: "VenueFlags",
                table: "Flag");

            migrationBuilder.DropColumn(
                name: "ResolutionDate",
                schema: "VenueFlags",
                table: "Flag");

            migrationBuilder.DropColumn(
                name: "ResolvedBy",
                schema: "VenueFlags",
                table: "Flag");
        }
    }
}
