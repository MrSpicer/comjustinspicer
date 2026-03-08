using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Comjustinspicer.CMS.Migrations.ContentZone
{
    /// <inheritdoc />
    public partial class AddContentZoneAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ContentZones_Name",
                table: "ContentZones");

            migrationBuilder.CreateTable(
                name: "ContentZoneAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SlotName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ContentZoneId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentPageMasterId = table.Column<Guid>(type: "uuid", nullable: true),
                    ParentZoneId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentZoneAssignments", x => x.Id);
                    table.CheckConstraint("CK_ContentZoneAssignments_OneParent", "(\"ParentPageMasterId\" IS NOT NULL AND \"ParentZoneId\" IS NULL) OR (\"ParentPageMasterId\" IS NULL AND \"ParentZoneId\" IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_ContentZoneAssignments_ContentZones_ContentZoneId",
                        column: x => x.ContentZoneId,
                        principalTable: "ContentZones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContentZoneAssignments_ContentZones_ParentZoneId",
                        column: x => x.ParentZoneId,
                        principalTable: "ContentZones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentZoneAssignments_ContentZoneId",
                table: "ContentZoneAssignments",
                column: "ContentZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentZoneAssignments_PageSlot",
                table: "ContentZoneAssignments",
                columns: new[] { "ParentPageMasterId", "SlotName" },
                unique: true,
                filter: "\"ParentPageMasterId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ContentZoneAssignments_ZoneSlot",
                table: "ContentZoneAssignments",
                columns: new[] { "ParentZoneId", "SlotName" },
                unique: true,
                filter: "\"ParentZoneId\" IS NOT NULL");

            // Data migration: populate assignments from existing path-based zone names
            // Pattern: page:{guid}/{slotName}#{ordinal}
            migrationBuilder.Sql(@"
INSERT INTO ""ContentZoneAssignments"" (""Id"", ""SlotName"", ""ContentZoneId"", ""ParentPageMasterId"", ""ParentZoneId"")
SELECT
    gen_random_uuid(),
    split_part(split_part(""Name"", '/', 2), '#', 1),
    ""Id"",
    split_part(split_part(""Name"", ':', 2), '/', 1)::uuid,
    NULL
FROM ""ContentZones""
WHERE ""Name"" ~ '^page:[0-9a-f-]{36}/.+#[0-9]+$'
ON CONFLICT DO NOTHING;
");

            // Update zone Name to human-readable slot name (strip the path prefix + ordinal)
            migrationBuilder.Sql(@"
UPDATE ""ContentZones""
SET ""Name"" = split_part(split_part(""Name"", '/', 2), '#', 1)
WHERE ""Name"" ~ '^page:[0-9a-f-]{36}/.+#[0-9]+$';
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentZoneAssignments");

            migrationBuilder.CreateIndex(
                name: "IX_ContentZones_Name",
                table: "ContentZones",
                column: "Name",
                unique: true);
        }
    }
}
