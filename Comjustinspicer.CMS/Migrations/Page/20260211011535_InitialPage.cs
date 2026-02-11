using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Comjustinspicer.CMS.Migrations.Page
{
    /// <inheritdoc />
    public partial class InitialPage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Route = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    ControllerName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    ConfigurationJson = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    Slug = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    LastModifiedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    PublicationDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PublicationEndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModificationDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsPublished = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsArchived = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    MasterId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false),
                    CustomFields = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pages_Route",
                table: "Pages",
                column: "Route",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pages");
        }
    }
}
