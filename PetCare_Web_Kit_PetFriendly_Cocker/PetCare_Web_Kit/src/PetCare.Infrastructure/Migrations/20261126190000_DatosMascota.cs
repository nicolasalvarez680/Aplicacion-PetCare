using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable
namespace PetCare.Infrastructure.Migrations;

public partial class DatosMascota : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // No se adivina la especie ni la raza de los registros anteriores.
        migrationBuilder.AddColumn<string>(name: "TipoMascota", table: "Servicios", type: "nvarchar(10)", maxLength: 10, nullable: true);
        migrationBuilder.AddColumn<string>(name: "RazaMascota", table: "Servicios", type: "nvarchar(80)", maxLength: 80, nullable: true);
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "RazaMascota", table: "Servicios");
        migrationBuilder.DropColumn(name: "TipoMascota", table: "Servicios");
    }
}
