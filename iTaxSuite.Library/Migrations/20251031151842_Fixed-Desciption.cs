using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iTaxSuite.Library.Migrations
{
    /// <inheritdoc />
    public partial class FixedDesciption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Desciption",
                table: "PurchTransact");

            migrationBuilder.RenameColumn(
                name: "Desciption",
                table: "SalesTransact",
                newName: "Description");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Description",
                table: "SalesTransact",
                newName: "Desciption");

            migrationBuilder.AddColumn<string>(
                name: "Desciption",
                table: "PurchTransact",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);
        }
    }
}
