using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iTaxSuite.Library.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAmountFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvData_SalesTransact_SalesTrxID",
                table: "SalesInvData");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SalesInvData",
                table: "SalesInvData");

            migrationBuilder.RenameTable(
                name: "SalesInvData",
                newName: "SalesTrxData");

            migrationBuilder.RenameColumn(
                name: "SrcDiscWTX",
                table: "SalesTransact",
                newName: "SrcTotTaxAmt");

            migrationBuilder.RenameColumn(
                name: "SrcAmtWTX",
                table: "SalesTransact",
                newName: "SrcTotAmtWTax");

            migrationBuilder.RenameColumn(
                name: "HomeDiscWTX",
                table: "SalesTransact",
                newName: "SrcTBaseAmt");

            migrationBuilder.RenameColumn(
                name: "HomeAmtWTX",
                table: "SalesTransact",
                newName: "SrcDiscAmt");

            migrationBuilder.RenameColumn(
                name: "TotalAmount",
                table: "PurchTransact",
                newName: "SrcTotTaxAmt");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "TaxClient",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "KES");

            migrationBuilder.AddColumn<decimal>(
                name: "HomeDiscAmt",
                table: "SalesTransact",
                type: "decimal(19,3)",
                precision: 19,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HomeTBaseAmt",
                table: "SalesTransact",
                type: "decimal(19,3)",
                precision: 19,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HomeTotAmtWTax",
                table: "SalesTransact",
                type: "decimal(19,3)",
                precision: 19,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HomeTotTaxAmt",
                table: "SalesTransact",
                type: "decimal(19,3)",
                precision: 19,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ReqType",
                table: "SalesTransact",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "DocExchRate",
                table: "PurchTransact",
                type: "decimal(19,3)",
                precision: 19,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "DocHomeCurr",
                table: "PurchTransact",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DocRateDate",
                table: "PurchTransact",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DocSrcCurr",
                table: "PurchTransact",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "HomeDiscAmt",
                table: "PurchTransact",
                type: "decimal(19,3)",
                precision: 19,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HomeTBaseAmt",
                table: "PurchTransact",
                type: "decimal(19,3)",
                precision: 19,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HomeTotAmtWTax",
                table: "PurchTransact",
                type: "decimal(19,3)",
                precision: 19,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HomeTotTaxAmt",
                table: "PurchTransact",
                type: "decimal(19,3)",
                precision: 19,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SrcDiscAmt",
                table: "PurchTransact",
                type: "decimal(19,3)",
                precision: 19,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SrcTBaseAmt",
                table: "PurchTransact",
                type: "decimal(19,3)",
                precision: 19,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SrcTotAmtWTax",
                table: "PurchTransact",
                type: "decimal(19,3)",
                precision: 19,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SalesTrxData",
                table: "SalesTrxData",
                column: "SalesTrxID");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesTrxData_SalesTransact_SalesTrxID",
                table: "SalesTrxData",
                column: "SalesTrxID",
                principalTable: "SalesTransact",
                principalColumn: "SalesTrxID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesTrxData_SalesTransact_SalesTrxID",
                table: "SalesTrxData");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SalesTrxData",
                table: "SalesTrxData");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "TaxClient");

            migrationBuilder.DropColumn(
                name: "HomeDiscAmt",
                table: "SalesTransact");

            migrationBuilder.DropColumn(
                name: "HomeTBaseAmt",
                table: "SalesTransact");

            migrationBuilder.DropColumn(
                name: "HomeTotAmtWTax",
                table: "SalesTransact");

            migrationBuilder.DropColumn(
                name: "HomeTotTaxAmt",
                table: "SalesTransact");

            migrationBuilder.DropColumn(
                name: "ReqType",
                table: "SalesTransact");

            migrationBuilder.DropColumn(
                name: "DocExchRate",
                table: "PurchTransact");

            migrationBuilder.DropColumn(
                name: "DocHomeCurr",
                table: "PurchTransact");

            migrationBuilder.DropColumn(
                name: "DocRateDate",
                table: "PurchTransact");

            migrationBuilder.DropColumn(
                name: "DocSrcCurr",
                table: "PurchTransact");

            migrationBuilder.DropColumn(
                name: "HomeDiscAmt",
                table: "PurchTransact");

            migrationBuilder.DropColumn(
                name: "HomeTBaseAmt",
                table: "PurchTransact");

            migrationBuilder.DropColumn(
                name: "HomeTotAmtWTax",
                table: "PurchTransact");

            migrationBuilder.DropColumn(
                name: "HomeTotTaxAmt",
                table: "PurchTransact");

            migrationBuilder.DropColumn(
                name: "SrcDiscAmt",
                table: "PurchTransact");

            migrationBuilder.DropColumn(
                name: "SrcTBaseAmt",
                table: "PurchTransact");

            migrationBuilder.DropColumn(
                name: "SrcTotAmtWTax",
                table: "PurchTransact");

            migrationBuilder.RenameTable(
                name: "SalesTrxData",
                newName: "SalesInvData");

            migrationBuilder.RenameColumn(
                name: "SrcTotTaxAmt",
                table: "SalesTransact",
                newName: "SrcDiscWTX");

            migrationBuilder.RenameColumn(
                name: "SrcTotAmtWTax",
                table: "SalesTransact",
                newName: "SrcAmtWTX");

            migrationBuilder.RenameColumn(
                name: "SrcTBaseAmt",
                table: "SalesTransact",
                newName: "HomeDiscWTX");

            migrationBuilder.RenameColumn(
                name: "SrcDiscAmt",
                table: "SalesTransact",
                newName: "HomeAmtWTX");

            migrationBuilder.RenameColumn(
                name: "SrcTotTaxAmt",
                table: "PurchTransact",
                newName: "TotalAmount");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SalesInvData",
                table: "SalesInvData",
                column: "SalesTrxID");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvData_SalesTransact_SalesTrxID",
                table: "SalesInvData",
                column: "SalesTrxID",
                principalTable: "SalesTransact",
                principalColumn: "SalesTrxID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
