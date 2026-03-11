using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Comjustinspicer.CMS.Migrations.ContentZone
{
    /// <inheritdoc />
    public partial class AddContentZoneVersioning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ContentZoneItems");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                table: "ContentZoneItems");

            // Fix existing items that have MasterId = Guid.Empty
            migrationBuilder.Sql(
                """UPDATE "ContentZoneItems" SET "MasterId" = "Id" WHERE "MasterId" = '00000000-0000-0000-0000-000000000000';""");

            // Fix existing zones that have MasterId = Guid.Empty
            migrationBuilder.Sql(
                """UPDATE "ContentZones" SET "MasterId" = "Id" WHERE "MasterId" = '00000000-0000-0000-0000-000000000000';""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ContentZoneItems",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAt",
                table: "ContentZoneItems",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
