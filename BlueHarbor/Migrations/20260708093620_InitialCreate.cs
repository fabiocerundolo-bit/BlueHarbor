using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BlueHarbor.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Dimensione",
                columns: table => new
                {
                    SizeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SizeName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dimensione", x => x.SizeId);
                });

            migrationBuilder.CreateTable(
                name: "Ruolo",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ruolo", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "SystemStates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CurrentDay = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemStates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Banchina",
                columns: table => new
                {
                    BerthId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BerthName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SizeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Banchina", x => x.BerthId);
                    table.ForeignKey(
                        name: "FK_Banchina_Dimensione_SizeId",
                        column: x => x.SizeId,
                        principalTable: "Dimensione",
                        principalColumn: "SizeId",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateTable(
                name: "Utente",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utente", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Utente_Ruolo_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Ruolo",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Nave",
                columns: table => new
                {
                    ShipId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArrivalDay = table.Column<int>(type: "int", nullable: false),
                    DurationDays = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    IdListaNavi = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nave", x => x.ShipId);
                    table.ForeignKey(
                        name: "FK_Nave_ListaNavi_IdListaNavi",
                        column: x => x.IdListaNavi,
                        principalTable: "ListaNavi",
                        principalColumn: "IdListaNavi",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Nave_Utente_UserId",
                        column: x => x.UserId,
                        principalTable: "Utente",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Occupazione",
                columns: table => new
                {
                    OccupancyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartDay = table.Column<int>(type: "int", nullable: false),
                    ShipId = table.Column<int>(type: "int", nullable: false),
                    BerthId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Occupazione", x => x.OccupancyId);
                    table.ForeignKey(
                        name: "FK_Occupazione_Banchina_BerthId",
                        column: x => x.BerthId,
                        principalTable: "Banchina",
                        principalColumn: "BerthId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Occupazione_Nave_ShipId",
                        column: x => x.ShipId,
                        principalTable: "Nave",
                        principalColumn: "ShipId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Occupazione_Utente_UserId",
                        column: x => x.UserId,
                        principalTable: "Utente",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Dimensione",
                columns: new[] { "SizeId", "SizeName" },
                values: new object[,]
                {
                    { 1, "XL" },
                    { 2, "L" },
                    { 3, "M" },
                    { 4, "S" }
                });

            migrationBuilder.InsertData(
                table: "Ruolo",
                columns: new[] { "RoleId", "RoleName" },
                values: new object[,]
                {
                    { 1, "Operator" },
                    { 2, "Scheduler" }
                });

            migrationBuilder.InsertData(
                table: "SystemStates",
                columns: new[] { "Id", "CurrentDay" },
                values: new object[] { 1, 1 });

            migrationBuilder.InsertData(
                table: "Banchina",
                columns: new[] { "BerthId", "BerthName", "SizeId" },
                values: new object[,]
                {
                    { 1, "Berth XL1", 1 },
                    { 2, "Berth L1", 2 },
                    { 3, "Berth M1", 3 },
                    { 4, "Berth M2", 3 },
                    { 5, "Berth S1", 4 },
                    { 6, "Berth S2", 4 },
                    { 7, "Berth S3", 4 },
                    { 8, "Berth S4", 4 }
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

            migrationBuilder.InsertData(
                table: "Utente",
                columns: new[] { "UserId", "Email", "Name", "Password", "RoleId" },
                values: new object[] { 1, "admin@blueharbor.com", "Admin", "admin", 1 });

            migrationBuilder.CreateIndex(
                name: "IX_Banchina_SizeId",
                table: "Banchina",
                column: "SizeId");

            migrationBuilder.CreateIndex(
                name: "IX_ListaNavi_FK_Id_Dimensione",
                table: "ListaNavi",
                column: "FK_Id_Dimensione");

            migrationBuilder.CreateIndex(
                name: "IX_Nave_IdListaNavi",
                table: "Nave",
                column: "IdListaNavi");

            migrationBuilder.CreateIndex(
                name: "IX_Nave_UserId",
                table: "Nave",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Occupazione_BerthId",
                table: "Occupazione",
                column: "BerthId");

            migrationBuilder.CreateIndex(
                name: "IX_Occupazione_ShipId",
                table: "Occupazione",
                column: "ShipId");

            migrationBuilder.CreateIndex(
                name: "IX_Occupazione_UserId",
                table: "Occupazione",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Utente_RoleId",
                table: "Utente",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Occupazione");

            migrationBuilder.DropTable(
                name: "SystemStates");

            migrationBuilder.DropTable(
                name: "Banchina");

            migrationBuilder.DropTable(
                name: "Nave");

            migrationBuilder.DropTable(
                name: "ListaNavi");

            migrationBuilder.DropTable(
                name: "Utente");

            migrationBuilder.DropTable(
                name: "Dimensione");

            migrationBuilder.DropTable(
                name: "Ruolo");
        }
    }
}
