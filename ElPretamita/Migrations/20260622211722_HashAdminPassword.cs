using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElPretamita.Migrations
{
    /// <inheritdoc />
    public partial class HashAdminPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Contrasena",
                value: "ac9689e2272427085e35b9d3e3e8bed88cb3434828b43b86fc0596cad4c6e270");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Contrasena",
                value: "admin1234");
        }
    }
}
