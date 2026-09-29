using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bib_Hacienda.Infraestructura.Persistencia.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventosClinicos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PotreroId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NombreRes = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TipoEvento = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventosClinicos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Potreros",
                columns: table => new
                {
                    Identificacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Tipo_potrero = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Potreros", x => x.Identificacion);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ContrasenaHash = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Nombre);
                });

            migrationBuilder.CreateTable(
                name: "Reses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Peso = table.Column<long>(type: "bigint", nullable: false),
                    Edad = table.Column<int>(type: "integer", nullable: false),
                    PotreroId = table.Column<string>(type: "character varying(100)", nullable: false),
                    TipoRes = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reses_Potreros_PotreroId",
                        column: x => x.PotreroId,
                        principalTable: "Potreros",
                        principalColumn: "Identificacion",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Vacunas",
                columns: table => new
                {
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Lote = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Fecha_vencimiento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Fecha_aplicacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResId = table.Column<int>(type: "integer", nullable: true),
                    TipoVacuna = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    Periodo_aplicacion = table.Column<long>(type: "bigint", nullable: true),
                    PeriodoAtenuacion = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vacunas", x => new { x.Nombre, x.Lote });
                    table.ForeignKey(
                        name: "FK_Vacunas_Reses_ResId",
                        column: x => x.ResId,
                        principalTable: "Reses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ventas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PotreroId = table.Column<string>(type: "character varying(100)", nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Monto = table.Column<long>(type: "bigint", nullable: false),
                    ResId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ventas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ventas_Potreros_PotreroId",
                        column: x => x.PotreroId,
                        principalTable: "Potreros",
                        principalColumn: "Identificacion",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ventas_Reses_ResId",
                        column: x => x.ResId,
                        principalTable: "Reses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventosClinicos_PotreroId_NombreRes",
                table: "EventosClinicos",
                columns: new[] { "PotreroId", "NombreRes" });

            migrationBuilder.CreateIndex(
                name: "IX_Reses_PotreroId",
                table: "Reses",
                column: "PotreroId");

            migrationBuilder.CreateIndex(
                name: "IX_Vacunas_ResId",
                table: "Vacunas",
                column: "ResId");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_PotreroId",
                table: "Ventas",
                column: "PotreroId");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_ResId",
                table: "Ventas",
                column: "ResId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventosClinicos");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Vacunas");

            migrationBuilder.DropTable(
                name: "Ventas");

            migrationBuilder.DropTable(
                name: "Reses");

            migrationBuilder.DropTable(
                name: "Potreros");
        }
    }
}
