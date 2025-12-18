using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iTaxSuite.Library.Migrations
{
    /// <inheritdoc />
    public partial class extrasalesinvcolumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsProduction",
                table: "TaxClient",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "EtrTaxClass",
                table: "SalesTransact",
                type: "nvarchar(1)",
                maxLength: 1,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InternalData",
                table: "SalesTransact",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReceiptNumber",
                table: "SalesTransact",
                type: "int",
                maxLength: 64,
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ReceiptSignature",
                table: "SalesTransact",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SDCID",
                table: "SalesTransact",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsProduction",
                table: "TaxClient");

            migrationBuilder.DropColumn(
                name: "EtrTaxClass",
                table: "SalesTransact");

            migrationBuilder.DropColumn(
                name: "InternalData",
                table: "SalesTransact");

            migrationBuilder.DropColumn(
                name: "ReceiptNumber",
                table: "SalesTransact");

            migrationBuilder.DropColumn(
                name: "ReceiptSignature",
                table: "SalesTransact");

            migrationBuilder.DropColumn(
                name: "SDCID",
                table: "SalesTransact");
        }
    }
}
