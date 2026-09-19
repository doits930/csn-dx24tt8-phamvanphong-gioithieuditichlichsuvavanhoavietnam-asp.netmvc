using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiTichVietNam.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRelicDescriptionNoAccent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DescriptionNoAccent",
                table: "Relics",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DescriptionNoAccent",
                table: "Relics");
        }
    }
}
