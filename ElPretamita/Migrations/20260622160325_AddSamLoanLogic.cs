using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElPretamita.Migrations
{
    /// <inheritdoc />
    public partial class AddSamLoanLogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ValorPenalidadAtraso",
                table: "Sams",
                newName: "TotalPagar");

            migrationBuilder.RenameColumn(
                name: "TotalPenalidadAcumulada",
                table: "Sams",
                newName: "SaldoRestante");

            migrationBuilder.RenameColumn(
                name: "Balance",
                table: "Sams",
                newName: "MontoOriginal");

            migrationBuilder.AddColumn<int>(
                name: "CantidadSemanas",
                table: "Sams",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "CuotaSemanal",
                table: "Sams",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Sams",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "InteresPorcentaje",
                table: "Sams",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "PagosSam",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SamId = table.Column<int>(type: "int", nullable: false),
                    MontoPagado = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FechaPago = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagosSam", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PagosSam_Sams_SamId",
                        column: x => x.SamId,
                        principalTable: "Sams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PagosSam_SamId",
                table: "PagosSam",
                column: "SamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PagosSam");

            migrationBuilder.DropColumn(
                name: "CantidadSemanas",
                table: "Sams");

            migrationBuilder.DropColumn(
                name: "CuotaSemanal",
                table: "Sams");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Sams");

            migrationBuilder.DropColumn(
                name: "InteresPorcentaje",
                table: "Sams");

            migrationBuilder.RenameColumn(
                name: "TotalPagar",
                table: "Sams",
                newName: "ValorPenalidadAtraso");

            migrationBuilder.RenameColumn(
                name: "SaldoRestante",
                table: "Sams",
                newName: "TotalPenalidadAcumulada");

            migrationBuilder.RenameColumn(
                name: "MontoOriginal",
                table: "Sams",
                newName: "Balance");
        }
    }
}
