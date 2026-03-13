using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataSneeq.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDuplicateKeyColumnsToTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DuplicateKeyColumnsJson",
                table: "MappingTemplates",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DuplicateKeyColumnsJson",
                table: "MappingTemplates");
        }
    }
}
