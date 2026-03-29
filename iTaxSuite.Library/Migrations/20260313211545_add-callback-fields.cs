using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iTaxSuite.Library.Migrations
{
    /// <inheritdoc />
    public partial class addcallbackfields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CallbackPayload",
                table: "SalesTrxData",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CallbackTime",
                table: "SalesTrxData",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CallbackPayload",
                table: "ProductData",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CallbackTime",
                table: "ProductData",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RespHeaders",
                table: "ApiRequestLog",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReqHeaders",
                table: "ApiRequestLog",
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(512)",
                oldMaxLength: 512,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Duration",
                table: "ApiRequestLog",
                type: "decimal(19,3)",
                precision: 19,
                scale: 3,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CallbackPayload",
                table: "SalesTrxData");

            migrationBuilder.DropColumn(
                name: "CallbackTime",
                table: "SalesTrxData");

            migrationBuilder.DropColumn(
                name: "CallbackPayload",
                table: "ProductData");

            migrationBuilder.DropColumn(
                name: "CallbackTime",
                table: "ProductData");

            migrationBuilder.AlterColumn<string>(
                name: "RespHeaders",
                table: "ApiRequestLog",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2048)",
                oldMaxLength: 2048,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReqHeaders",
                table: "ApiRequestLog",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1024)",
                oldMaxLength: 1024,
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Duration",
                table: "ApiRequestLog",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(19,3)",
                oldPrecision: 19,
                oldScale: 3);
        }
    }
}
