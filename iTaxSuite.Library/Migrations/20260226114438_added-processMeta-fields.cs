using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iTaxSuite.Library.Migrations
{
    /// <inheritdoc />
    public partial class addedprocessMetafields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProcessMeta",
                table: "TaxClient",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceApp",
                table: "Product",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProcessMeta",
                table: "ExtSystConfig",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProcessMeta",
                table: "ClientBranch",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProcessMeta",
                table: "TaxClient");

            migrationBuilder.DropColumn(
                name: "SourceApp",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "ProcessMeta",
                table: "ExtSystConfig");

            migrationBuilder.DropColumn(
                name: "ProcessMeta",
                table: "ClientBranch");
        }
    }
}
