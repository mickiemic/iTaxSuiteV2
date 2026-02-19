using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iTaxSuite.Library.Migrations
{
    /// <inheritdoc />
    public partial class addingSalesItemTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "AK_SalesTransact_SalesTrxID_BranchCode",
                table: "SalesTransact",
                columns: new[] { "SalesTrxID", "BranchCode" });

            migrationBuilder.CreateTable(
                name: "SalesItem",
                columns: table => new
                {
                    SalesTrxID = table.Column<int>(type: "int", nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    BranchCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    TaxItemCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    TaxTypeCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ItemTypeCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ItemSeqNumber = table.Column<int>(type: "int", nullable: false),
                    ItemClassCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    PkgUnitCode = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    Package = table.Column<decimal>(type: "decimal(19,3)", precision: 19, scale: 3, nullable: false),
                    QtyUnitCode = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(19,3)", precision: 19, scale: 3, nullable: false),
                    IsStockable = table.Column<bool>(type: "bit", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(19,3)", precision: 19, scale: 3, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(19,3)", precision: 19, scale: 3, nullable: false),
                    DiscountRate = table.Column<decimal>(type: "decimal(19,3)", precision: 19, scale: 3, nullable: false),
                    TaxableAmount = table.Column<decimal>(type: "decimal(19,3)", precision: 19, scale: 3, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(19,3)", precision: 19, scale: 3, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(19,3)", precision: 19, scale: 3, nullable: false),
                    SupplyPrice = table.Column<decimal>(type: "decimal(19,3)", precision: 19, scale: 3, nullable: false),
                    RecordStatus = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesItem", x => new { x.SalesTrxID, x.ProductCode, x.BranchCode });
                    table.ForeignKey(
                        name: "FK_SalesItem_SalesTransact_SalesTrxID_BranchCode",
                        columns: x => new { x.SalesTrxID, x.BranchCode },
                        principalTable: "SalesTransact",
                        principalColumns: new[] { "SalesTrxID", "BranchCode" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesItem_SalesTrxID_BranchCode",
                table: "SalesItem",
                columns: new[] { "SalesTrxID", "BranchCode" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalesItem");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_SalesTransact_SalesTrxID_BranchCode",
                table: "SalesTransact");
        }
    }
}
