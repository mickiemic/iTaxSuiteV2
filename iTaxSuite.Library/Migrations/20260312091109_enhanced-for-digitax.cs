using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iTaxSuite.Library.Migrations
{
    /// <inheritdoc />
    public partial class enhancedfordigitax : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "APIKey",
                table: "TaxClient",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BaseCallback",
                table: "TaxClient",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeviceType",
                table: "TaxClient",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ExternalID",
                table: "TaxClient",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalID",
                table: "StockItem",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalID",
                table: "SalesTransact",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalURL",
                table: "SalesTransact",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OfflineURL",
                table: "SalesTransact",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalID",
                table: "SalesItem",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalID",
                table: "PurchTransact",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalID",
                table: "BranchVendor",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalID",
                table: "BranchUser",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalID",
                table: "BranchCustomer",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "APIKey",
                table: "TaxClient");

            migrationBuilder.DropColumn(
                name: "BaseCallback",
                table: "TaxClient");

            migrationBuilder.DropColumn(
                name: "DeviceType",
                table: "TaxClient");

            migrationBuilder.DropColumn(
                name: "ExternalID",
                table: "TaxClient");

            migrationBuilder.DropColumn(
                name: "ExternalID",
                table: "StockItem");

            migrationBuilder.DropColumn(
                name: "ExternalID",
                table: "SalesTransact");

            migrationBuilder.DropColumn(
                name: "ExternalURL",
                table: "SalesTransact");

            migrationBuilder.DropColumn(
                name: "OfflineURL",
                table: "SalesTransact");

            migrationBuilder.DropColumn(
                name: "ExternalID",
                table: "SalesItem");

            migrationBuilder.DropColumn(
                name: "ExternalID",
                table: "PurchTransact");

            migrationBuilder.DropColumn(
                name: "ExternalID",
                table: "BranchVendor");

            migrationBuilder.DropColumn(
                name: "ExternalID",
                table: "BranchUser");

            migrationBuilder.DropColumn(
                name: "ExternalID",
                table: "BranchCustomer");
        }
    }
}
