using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlueHarbor.Migrations
{
    /// <inheritdoc />
    public partial class AddListaNaviAndUpdateShipModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IdDimensione",
                table: "Dimensione",
                newName: "SizeId");

            migrationBuilder.RenameColumn(
                name: "NomeDimensione",
                table: "Dimensione",
                newName: "SizeName");

            migrationBuilder.RenameColumn(
                name: "IdRuolo",
                table: "Ruolo",
                newName: "RoleId");

            migrationBuilder.RenameColumn(
                name: "NomeRuolo",
                table: "Ruolo",
                newName: "RoleName");

            migrationBuilder.RenameColumn(
                name: "IdUtente",
                table: "Utente",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "Nome",
                table: "Utente",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "IdRuolo",
                table: "Utente",
                newName: "RoleId");

            migrationBuilder.RenameColumn(
                name: "IdBanchina",
                table: "Banchina",
                newName: "BerthId");

            migrationBuilder.RenameColumn(
                name: "NomeBanchina",
                table: "Banchina",
                newName: "BerthName");

            migrationBuilder.RenameColumn(
                name: "IdDimensione",
                table: "Banchina",
                newName: "SizeId");

            migrationBuilder.RenameColumn(
                name: "IdNave",
                table: "Nave",
                newName: "ShipId");

            migrationBuilder.RenameColumn(
                name: "NomeNave",
                table: "Nave",
                newName: "ShipName");

            migrationBuilder.RenameColumn(
                name: "GiornoArrivo",
                table: "Nave",
                newName: "ArrivalDay");

            migrationBuilder.RenameColumn(
                name: "DurataOccupazione",
                table: "Nave",
                newName: "DurationDays");

            migrationBuilder.RenameColumn(
                name: "Stato",
                table: "Nave",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "Note",
                table: "Nave",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "IdUtente",
                table: "Nave",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "IdOccupazione",
                table: "Occupazione",
                newName: "OccupancyId");

            migrationBuilder.RenameColumn(
                name: "GiornoInizio",
                table: "Occupazione",
                newName: "StartDay");

            migrationBuilder.RenameColumn(
                name: "IdNave",
                table: "Occupazione",
                newName: "ShipId");

            migrationBuilder.RenameColumn(
                name: "IdBanchina",
                table: "Occupazione",
                newName: "BerthId");

            migrationBuilder.RenameColumn(
                name: "IdUtente",
                table: "Occupazione",
                newName: "UserId");

            migrationBuilder.CreateTable(
                name: "ListaNavi",
                columns: table => new
                {
                    IdListaNavi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NomeNave = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FK_Id_Dimensione = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListaNavi", x => x.IdListaNavi);
                    table.ForeignKey(
                        name: "FK_ListaNavi_Dimensione_FK_Id_Dimensione",
                        column: x => x.FK_Id_Dimensione,
                        principalTable: "Dimensione",
                        principalColumn: "SizeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ListaNavi",
                columns: new[] { "IdListaNavi", "FK_Id_Dimensione", "NomeNave" },
                values: new object[,]
                {
                    { 1, 1, "MSC Splendida" },
                    { 2, 1, "Costa Favolosa" },
                    { 3, 2, "Norwegian Epic" },
                    { 4, 2, "Celebrity Reflection" },
                    { 5, 3, "Queen Mary 2" },
                    { 6, 3, "Disney Dream" },
                    { 7, 4, "Seabourn Odyssey" },
                    { 8, 4, "Wind Star" }
                });

            migrationBuilder.AddColumn<int>(
                name: "IdListaNavi",
                table: "Nave",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.Sql(@"
UPDATE Nave
SET IdListaNavi = CASE IdDimensione
    WHEN 1 THEN 1
    WHEN 2 THEN 3
    WHEN 3 THEN 5
    WHEN 4 THEN 7
    ELSE 1
END");

            migrationBuilder.CreateIndex(
                name: "IX_ListaNavi_FK_Id_Dimensione",
                table: "ListaNavi",
                column: "FK_Id_Dimensione");

            migrationBuilder.CreateIndex(
                name: "IX_Nave_IdListaNavi",
                table: "Nave",
                column: "IdListaNavi");

            migrationBuilder.AddForeignKey(
                name: "FK_Nave_ListaNavi_IdListaNavi",
                table: "Nave",
                column: "IdListaNavi",
                principalTable: "ListaNavi",
                principalColumn: "IdListaNavi",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Nave_ListaNavi_IdListaNavi",
                table: "Nave");

            migrationBuilder.DropIndex(
                name: "IX_Nave_IdListaNavi",
                table: "Nave");

            migrationBuilder.DropColumn(
                name: "IdListaNavi",
                table: "Nave");

            migrationBuilder.DropTable(
                name: "ListaNavi");

            migrationBuilder.RenameColumn(
                name: "SizeId",
                table: "Dimensione",
                newName: "IdDimensione");

            migrationBuilder.RenameColumn(
                name: "SizeName",
                table: "Dimensione",
                newName: "NomeDimensione");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "Ruolo",
                newName: "IdRuolo");

            migrationBuilder.RenameColumn(
                name: "RoleName",
                table: "Ruolo",
                newName: "NomeRuolo");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Utente",
                newName: "IdUtente");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Utente",
                newName: "Nome");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "Utente",
                newName: "IdRuolo");

            migrationBuilder.RenameColumn(
                name: "BerthId",
                table: "Banchina",
                newName: "IdBanchina");

            migrationBuilder.RenameColumn(
                name: "BerthName",
                table: "Banchina",
                newName: "NomeBanchina");

            migrationBuilder.RenameColumn(
                name: "SizeId",
                table: "Banchina",
                newName: "IdDimensione");

            migrationBuilder.RenameColumn(
                name: "ShipId",
                table: "Nave",
                newName: "IdNave");

            migrationBuilder.RenameColumn(
                name: "ShipName",
                table: "Nave",
                newName: "NomeNave");

            migrationBuilder.RenameColumn(
                name: "ArrivalDay",
                table: "Nave",
                newName: "GiornoArrivo");

            migrationBuilder.RenameColumn(
                name: "DurationDays",
                table: "Nave",
                newName: "DurataOccupazione");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Nave",
                newName: "Stato");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "Nave",
                newName: "Note");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Nave",
                newName: "IdUtente");

            migrationBuilder.RenameColumn(
                name: "OccupancyId",
                table: "Occupazione",
                newName: "IdOccupazione");

            migrationBuilder.RenameColumn(
                name: "StartDay",
                table: "Occupazione",
                newName: "GiornoInizio");

            migrationBuilder.RenameColumn(
                name: "ShipId",
                table: "Occupazione",
                newName: "IdNave");

            migrationBuilder.RenameColumn(
                name: "BerthId",
                table: "Occupazione",
                newName: "IdBanchina");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Occupazione",
                newName: "IdUtente");
        }
    }
}
