using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CodeEventsAPI.Data.Migrations {
  public partial class DBInit : Migration {
    protected override void Up(MigrationBuilder migrationBuilder) {
      migrationBuilder.CreateTable(
        name: "CodeEvents",
        columns: table => new {
          Id = table.Column<long>(nullable: false)
            .Annotation(
              "MySql:ValueGenerationStrategy",
              MySqlValueGenerationStrategy.IdentityColumn
            ),
          Title = table.Column<string>(nullable: true),
          Description = table.Column<string>(nullable: true),
          Date = table.Column<DateTime>(nullable: false)
        },
        constraints: table => { table.PrimaryKey("PK_CodeEvents", x => x.Id); }
      );

      migrationBuilder.CreateTable(
        name: "Users",
        columns: table => new {
          Id = table.Column<long>(nullable: false)
            .Annotation(
              "MySql:ValueGenerationStrategy",
              MySqlValueGenerationStrategy.IdentityColumn
            ),
          AzureOId = table.Column<string>(nullable: true),
          Username = table.Column<string>(nullable: true)
        },
        constraints: table => { table.PrimaryKey("PK_Users", x => x.Id); }
      );

      migrationBuilder.CreateTable(
        name: "Members",
        columns: table => new {
          Id = table.Column<long>(nullable: false)
            .Annotation(
              "MySql:ValueGenerationStrategy",
              MySqlValueGenerationStrategy.IdentityColumn
            ),
          Role = table.Column<int>(nullable: false),
          UserId = table.Column<long>(nullable: false),
          CodeEventId = table.Column<long>(nullable: false)
        },
        constraints: table => {
          table.PrimaryKey("PK_Members", x => x.Id);
          table.ForeignKey(
            name: "FK_Members_CodeEvents_CodeEventId",
            column: x => x.CodeEventId,
            principalTable: "CodeEvents",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade
          );
          table.ForeignKey(
            name: "FK_Members_Users_UserId",
            column: x => x.UserId,
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade
          );
        }
      );

      migrationBuilder.CreateIndex(
        name: "IX_Members_CodeEventId",
        table: "Members",
        column: "CodeEventId"
      );

      migrationBuilder.CreateIndex(
        name: "IX_Members_UserId_CodeEventId",
        table: "Members",
        columns: new[] { "UserId", "CodeEventId" },
        unique: true
      );

      migrationBuilder.CreateIndex(
        name: "IX_Users_AzureOId",
        table: "Users",
        column: "AzureOId",
        unique: true
      );

      migrationBuilder.CreateIndex(
        name: "IX_Users_Username",
        table: "Users",
        column: "Username",
        unique: true
      );
    }

    protected override void Down(MigrationBuilder migrationBuilder) {
      migrationBuilder.DropTable(name: "Members");

      migrationBuilder.DropTable(name: "CodeEvents");

      migrationBuilder.DropTable(name: "Users");
    }
  }
}
