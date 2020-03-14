using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CodeEventsAPI.Data.Migrations {
  public partial class UsersTable : Migration {
    protected override void Up(MigrationBuilder migrationBuilder) {
      migrationBuilder.CreateTable(
        "Users",
        table => new {
          Id = table.Column<long>()
            .Annotation("MySql:ValueGenerationStrategy",
              MySqlValueGenerationStrategy.IdentityColumn),
          Email = table.Column<string>(nullable: true),
          Password = table.Column<string>(nullable: true)
        },
        constraints: table => { table.PrimaryKey("PK_Users", x => x.Id); });
    }

    protected override void Down(MigrationBuilder migrationBuilder) {
      migrationBuilder.DropTable(
        "Users");
    }
  }
}