using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iTaxSuite.Library.Migrations
{
    /// <inheritdoc />
    public partial class updateuseraccountstructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DistributorCode",
                table: "SysUser");

            migrationBuilder.DropColumn(
                name: "SalesPersonKey",
                table: "SysUser");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DistributorCode",
                table: "SysUser",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SalesPersonKey",
                table: "SysUser",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);
        }
    }
}
