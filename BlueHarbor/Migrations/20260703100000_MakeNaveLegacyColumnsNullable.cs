using BlueHarbor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlueHarbor.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(BlueHarborDbContext))]
    [Migration("20260703100000_MakeNaveLegacyColumnsNullable")]
    public partial class MakeNaveLegacyColumnsNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
ALTER TABLE [Nave]
ALTER COLUMN [ShipName] nvarchar(max) NULL;");

            migrationBuilder.Sql(@"
ALTER TABLE [Nave]
ALTER COLUMN [IdDimensione] int NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
ALTER TABLE [Nave]
ALTER COLUMN [ShipName] nvarchar(max) NOT NULL;");

            migrationBuilder.Sql(@"
ALTER TABLE [Nave]
ALTER COLUMN [IdDimensione] int NOT NULL;");
        }
    }
}