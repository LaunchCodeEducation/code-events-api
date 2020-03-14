using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CodeEventsAPI.Data.Migrations {
  public partial class MembersTable : Migration {
    protected override void Up(MigrationBuilder migrationBuilder) {
      migrationBuilder.CreateTable(
        "Members",
        table => new {
          Id = table.Column<long>()
            .Annotation("MySql:ValueGenerationStrategy",
              MySqlValueGenerationStrategy.IdentityColumn),
          UserId = table.Column<long>(),
          CodeEventId = table.Column<long>(),
          Role = table.Column<int>()
        },
        constraints: table => {
          table.PrimaryKey("PK_Members", x => x.Id);
          table.ForeignKey(
            "FK_Members_CodeEvents_CodeEventId",
            x => x.CodeEventId,
            "CodeEvents",
            "Id",
            onDelete: ReferentialAction.Cascade);
          table.ForeignKey(
            "FK_Members_Users_UserId",
            x => x.UserId,
            "Users",
            "Id",
            onDelete: ReferentialAction.Cascade);
        });

      migrationBuilder.CreateIndex(
        "IX_Members_UserId_CodeEventId",
        "Members",
        new[] {"UserId", "CodeEventId"},
        unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder) {
      migrationBuilder.DropTable(
        "Members");
    }
  }
}