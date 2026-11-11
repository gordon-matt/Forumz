using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Forumz.Data.Npgsql.Migrations;

/// <inheritdoc />
public partial class AddContentFormat : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.AddColumn<int>(
            name: "Format",
            schema: "forumz",
            table: "Posts",
            type: "integer",
            nullable: false,
            defaultValue: 0);

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropColumn(
            name: "Format",
            schema: "forumz",
            table: "Posts");
}
