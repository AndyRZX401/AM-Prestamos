using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElPretamita.Migrations
{
    /// <inheritdoc />
    public partial class AgregarMoraYPenalidad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaVencimientoSiguiente",
                table: "Sams",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPenalidadAcumulada",
                table: "Sams",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorPenalidadAtraso",
                table: "Sams",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "AplicaMoraFija",
                table: "Prestamos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaVencimientoSiguiente",
                table: "Prestamos",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "TotalMoraAcumulada",
                table: "Prestamos",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorMora",
                table: "Prestamos",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaVencimientoSiguiente",
                table: "Sams");

            migrationBuilder.DropColumn(
                name: "TotalPenalidadAcumulada",
                table: "Sams");

            migrationBuilder.DropColumn(
                name: "ValorPenalidadAtraso",
                table: "Sams");

            migrationBuilder.DropColumn(
                name: "AplicaMoraFija",
                table: "Prestamos");

            migrationBuilder.DropColumn(
                name: "FechaVencimientoSiguiente",
                table: "Prestamos");

            migrationBuilder.DropColumn(
                name: "TotalMoraAcumulada",
                table: "Prestamos");

            migrationBuilder.DropColumn(
                name: "ValorMora",
                table: "Prestamos");
        }
    }
}
