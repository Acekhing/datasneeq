using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataSneeq.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPrimaryKeyGenerationStrategy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MappingTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    TargetTable = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    MappingsJson = table.Column<string>(type: "TEXT", nullable: false),
                    LookupRulesJson = table.Column<string>(type: "TEXT", nullable: false),
                    PrimaryKeyGenerationStrategy = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MappingTemplates", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MappingTemplates");
        }
    }
}
