using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Forumz.Data.Sqlite.Migrations;

/// <inheritdoc />
public partial class AddContentFormat : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.AddColumn<int>(
            name: "Format",
            table: "Forumz_Posts",
            type: "INTEGER",
            nullable: false,
            defaultValue: 0);

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropColumn(
            name: "Format",
            table: "Forumz_Posts");
}
