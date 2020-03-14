using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CodeEventsAPI.Data.Migrations {
  public partial class CodeEventsTable : Migration {
    protected override void Up(MigrationBuilder migrationBuilder) {
      migrationBuilder.CreateTable(
        "CodeEvents",
        table => new {
          Id = table.Column<long>()
            .Annotation("MySql:ValueGenerationStrategy",
              MySqlValueGenerationStrategy.IdentityColumn),
          Title = table.Column<string>(nullable: true),
          Description = table.Column<string>(nullable: true),
          Date = table.Column<DateTime>()
        },
        constraints: table => {
          table.PrimaryKey("PK_CodeEvents", x => x.Id);
        });
    }

    protected override void Down(MigrationBuilder migrationBuilder) {
      migrationBuilder.DropTable(
        "CodeEvents");
    }
  }
}