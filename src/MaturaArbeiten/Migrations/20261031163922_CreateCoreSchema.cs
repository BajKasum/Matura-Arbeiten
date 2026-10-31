using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaturaArbeiten.Migrations
{
    /// <inheritdoc />
    public partial class CreateCoreSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "administrationslogin",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    vorname = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    nachname = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    passwort_hash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    fehlversuche = table.Column<int>(type: "int", nullable: false),
                    gesperrt_bis = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_administrationslogin", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "g3sus",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    vorname = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    nachname = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    passwort_hash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    passwort_geaendert = table.Column<bool>(type: "bit", nullable: false),
                    fehlversuche = table.Column<int>(type: "int", nullable: false),
                    gesperrt_bis = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_g3sus", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "praesentation",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    thema = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    datum = table.Column<DateOnly>(type: "date", nullable: false),
                    uhrzeit = table.Column<TimeOnly>(type: "time(0)", nullable: false),
                    raum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_praesentation", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "systemeinstellung",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    platzkontingent = table.Column<int>(type: "int", nullable: false),
                    eintragungsphase_offen = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_systemeinstellung", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "email_versand",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    g3sus_id = table.Column<int>(type: "int", nullable: false),
                    typ = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    gesendet_am = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_email_versand", x => x.id);
                    table.ForeignKey(
                        name: "FK_email_versand_g3sus_g3sus_id",
                        column: x => x.g3sus_id,
                        principalTable: "g3sus",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "eintragung",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    g3sus_id = table.Column<int>(type: "int", nullable: false),
                    praesentation_id = table.Column<int>(type: "int", nullable: false),
                    erstellt_am = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eintragung", x => x.id);
                    table.ForeignKey(
                        name: "FK_eintragung_g3sus_g3sus_id",
                        column: x => x.g3sus_id,
                        principalTable: "g3sus",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_eintragung_praesentation_praesentation_id",
                        column: x => x.praesentation_id,
                        principalTable: "praesentation",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "systemeinstellung",
                columns: new[] { "id", "eintragungsphase_offen", "platzkontingent" },
                values: new object[] { 1, true, 6 });

            migrationBuilder.CreateIndex(
                name: "IX_administrationslogin_email",
                table: "administrationslogin",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_eintragung_g3sus_id_praesentation_id",
                table: "eintragung",
                columns: new[] { "g3sus_id", "praesentation_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_eintragung_praesentation_id",
                table: "eintragung",
                column: "praesentation_id");

            migrationBuilder.CreateIndex(
                name: "IX_email_versand_g3sus_id",
                table: "email_versand",
                column: "g3sus_id");

            migrationBuilder.CreateIndex(
                name: "IX_g3sus_email",
                table: "g3sus",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "administrationslogin");

            migrationBuilder.DropTable(
                name: "eintragung");

            migrationBuilder.DropTable(
                name: "email_versand");

            migrationBuilder.DropTable(
                name: "systemeinstellung");

            migrationBuilder.DropTable(
                name: "praesentation");

            migrationBuilder.DropTable(
                name: "g3sus");
        }
    }
}
