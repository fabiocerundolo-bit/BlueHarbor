using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlueHarbor.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomShipName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomName",
                table: "Nave",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomName",
                table: "Nave");
        }
    }
}
