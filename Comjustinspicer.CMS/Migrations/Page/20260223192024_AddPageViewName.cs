using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Comjustinspicer.CMS.Migrations.Page
{
    /// <inheritdoc />
    public partial class AddPageViewName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ViewName",
                table: "Pages",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ViewName",
                table: "Pages");
        }
    }
}
