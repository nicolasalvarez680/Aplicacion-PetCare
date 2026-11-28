using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable
namespace PetCare.Infrastructure.Migrations;
public partial class InventarioLotes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(name: "InventarioDescontado", table: "Facturas", type: "bit", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<int>(name: "Existencias", table: "Productos", type: "int", nullable: true);
        migrationBuilder.Sql("UPDATE dbo.Productos SET Existencias = 0 WHERE Tipo = 'LoteMedicamento' AND Existencias IS NULL;");
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "Existencias", table: "Productos");
        migrationBuilder.DropColumn(name: "InventarioDescontado", table: "Facturas");
    }
}
