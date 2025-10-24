using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Quasar.Samples.BasicApi.Migrations;

[DbContext(typeof(Quasar.Samples.BasicApi.SampleReadModelContext))]
[Migration("20250101000000_InitialReadModels")]
public partial class InitialReadModels : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Counters",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                Count = table.Column<int>(nullable: false)
            },
            constraints: table => { table.PrimaryKey("PK_Counters", x => x.Id); });

        migrationBuilder.CreateTable(
            name: "Carts",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                TotalItems = table.Column<int>(nullable: false)
            },
            constraints: table => { table.PrimaryKey("PK_Carts", x => x.Id); });

        migrationBuilder.CreateTable(
            name: "CartProducts",
            columns: table => new
            {
                ProductId = table.Column<Guid>(nullable: false),
                Quantity = table.Column<int>(nullable: false)
            },
            constraints: table => { table.PrimaryKey("PK_CartProducts", x => x.ProductId); });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("CartProducts");
        migrationBuilder.DropTable("Carts");
        migrationBuilder.DropTable("Counters");
    }
}
