using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iTaxSuite.Library.Migrations
{
    /// <inheritdoc />
    public partial class addparsesyntaxexpr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ParseSyntaxExpr",
                table: "SyncChannel",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParseSyntaxExpr",
                table: "SyncChannel");
        }
    }
}
