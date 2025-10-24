using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Quasar.Identity.Persistence.Relational.EfCore.Migrations;

[DbContext(typeof(Quasar.Identity.Persistence.Relational.EfCore.IdentityReadModelContext))]
[Migration("20250101000001_IdentityInitial")]
public partial class IdentityInitial : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                Username = table.Column<string>(nullable: false),
                Email = table.Column<string>(nullable: false),
                PasswordHash = table.Column<string>(nullable: true),
                PasswordSalt = table.Column<string>(nullable: true)
            },
            constraints: table => { table.PrimaryKey("PK_Users", x => x.Id); });

        migrationBuilder.CreateTable(
            name: "Sessions",
            columns: table => new
            {
                SessionId = table.Column<Guid>(nullable: false),
                UserId = table.Column<Guid>(nullable: false),
                RefreshTokenHash = table.Column<string>(nullable: true),
                IssuedUtc = table.Column<DateTime>(nullable: false),
                ExpiresUtc = table.Column<DateTime>(nullable: false),
                RevokedUtc = table.Column<DateTime>(nullable: true)
            },
            constraints: table => { table.PrimaryKey("PK_Sessions", x => x.SessionId); });

        migrationBuilder.CreateTable(
            name: "Roles",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                Name = table.Column<string>(nullable: false)
            },
            constraints: table => { table.PrimaryKey("PK_Roles", x => x.Id); });

        migrationBuilder.CreateTable(
            name: "RolePermissions",
            columns: table => new
            {
                RoleId = table.Column<Guid>(nullable: false),
                Permission = table.Column<string>(nullable: false)
            },
            constraints: table => { table.PrimaryKey("PK_RolePermissions", x => new { x.RoleId, x.Permission }); });

        migrationBuilder.CreateTable(
            name: "UserRoles",
            columns: table => new
            {
                UserId = table.Column<Guid>(nullable: false),
                RoleId = table.Column<Guid>(nullable: false)
            },
            constraints: table => { table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId }); });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("UserRoles");
        migrationBuilder.DropTable("RolePermissions");
        migrationBuilder.DropTable("Roles");
        migrationBuilder.DropTable("Sessions");
        migrationBuilder.DropTable("Users");
    }
}
