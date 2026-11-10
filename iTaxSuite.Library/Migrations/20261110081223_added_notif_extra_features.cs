using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iTaxSuite.Library.Migrations
{
    /// <inheritdoc />
    public partial class added_notif_extra_features : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExtraPayload",
                table: "SalesTrxData",
                type: "nvarchar(800)",
                maxLength: 800,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "QtyUnitCode",
                table: "SalesItem",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(8)",
                oldMaxLength: 8);

            migrationBuilder.AlterColumn<string>(
                name: "PkgUnitCode",
                table: "SalesItem",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(8)",
                oldMaxLength: 8);

            migrationBuilder.AddColumn<string>(
                name: "MappingPayload",
                table: "PurchTrxData",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MappingTime",
                table: "PurchTrxData",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsKitting",
                table: "Product",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "RespPayload",
                table: "ApiRequestLog",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReqPayload",
                table: "ApiRequestLog",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "ImportItem",
                columns: table => new
                {
                    BranchCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    ItemSeqNumber = table.Column<int>(type: "int", nullable: false),
                    ImportID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExternalID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ProductCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ImpItemCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ImpItemExternalID = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    TaskCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    DeclarationNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    DeclarationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ItemClassCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SupplierName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AgentName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    HSCode = table.Column<string>(type: "nvarchar(17)", maxLength: 17, nullable: true),
                    PkgUnitCode = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                    Package = table.Column<decimal>(type: "decimal(19,3)", precision: 19, scale: 3, nullable: false),
                    QtyUnitCode = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(19,3)", precision: 19, scale: 3, nullable: false),
                    GrossWeight = table.Column<double>(type: "float", nullable: false),
                    NetWeight = table.Column<double>(type: "float", nullable: false),
                    ImpItemStatusCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrigCountryCode = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    ExpCountryCode = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    InvForeignCurrencyAmount = table.Column<decimal>(type: "decimal(19,3)", precision: 19, scale: 3, nullable: false),
                    InvForeignCurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    InvForeignCurrencyExchangeRate = table.Column<decimal>(type: "decimal(19,3)", precision: 19, scale: 3, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    RecordStatus = table.Column<int>(type: "int", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    Tries = table.Column<int>(type: "int", nullable: false),
                    LastTry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SourceStamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SourcePayload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RequestPayload = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ResponseTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResponsePayload = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportItem", x => new { x.ItemSeqNumber, x.BranchCode });
                    table.ForeignKey(
                        name: "FK_ImportItem_ClientBranch_BranchCode",
                        column: x => x.BranchCode,
                        principalTable: "ClientBranch",
                        principalColumn: "BranchCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KitProduct",
                columns: table => new
                {
                    KitProdCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    BranchCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    CompProdCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    RecordStatus = table.Column<int>(type: "int", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    Tries = table.Column<int>(type: "int", nullable: false),
                    LastTry = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KitProduct", x => new { x.KitProdCode, x.CompProdCode, x.BranchCode });
                    table.ForeignKey(
                        name: "FK_KitProduct_StockItem_KitProdCode_BranchCode",
                        columns: x => new { x.KitProdCode, x.BranchCode },
                        principalTable: "StockItem",
                        principalColumns: new[] { "ProductCode", "BranchCode" });
                });

            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    NotificationID = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    BranchCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    ExternalID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    TargetType = table.Column<int>(type: "int", nullable: false),
                    TargetRoleId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TargetUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Message = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    LinkUrl = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    Level = table.Column<int>(type: "int", nullable: false),
                    NotificationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SourceStamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SourcePayload = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notification", x => new { x.NotificationID, x.BranchCode });
                    table.ForeignKey(
                        name: "FK_Notification_ClientBranch_BranchCode",
                        column: x => x.BranchCode,
                        principalTable: "ClientBranch",
                        principalColumn: "BranchCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseItem",
                columns: table => new
                {
                    PurchaseID = table.Column<int>(type: "int", nullable: false),
                    SourceLineNo = table.Column<int>(type: "int", nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ProductNae = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ExternalID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ItemSeqNumber = table.Column<int>(type: "int", nullable: false),
                    TaxTypeCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ItemClassCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    PkgUnitCode = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                    Package = table.Column<decimal>(type: "decimal(19,3)", precision: 19, scale: 3, nullable: false),
                    QtyUnitCode = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(19,3)", precision: 19, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(19,3)", precision: 19, scale: 3, nullable: false),
                    SupplyPrice = table.Column<decimal>(type: "decimal(19,3)", precision: 19, scale: 3, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(19,3)", precision: 19, scale: 3, nullable: false),
                    DiscountRate = table.Column<decimal>(type: "decimal(19,3)", precision: 19, scale: 3, nullable: false),
                    TaxableAmount = table.Column<decimal>(type: "decimal(19,3)", precision: 19, scale: 3, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(19,3)", precision: 19, scale: 3, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(19,3)", precision: 19, scale: 3, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseItem", x => new { x.PurchaseID, x.SourceLineNo });
                    table.ForeignKey(
                        name: "FK_PurchaseItem_PurchTransact_PurchaseID",
                        column: x => x.PurchaseID,
                        principalTable: "PurchTransact",
                        principalColumn: "PurchaseID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TISystemCode",
                columns: table => new
                {
                    CodeClass = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    ClassName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", maxLength: 1, nullable: false),
                    UserDefinedName1 = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    UserDefinedName2 = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    UserDefinedName3 = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TISystemCode", x => x.CodeClass);
                });

            migrationBuilder.CreateTable(
                name: "NotificationReceiver",
                columns: table => new
                {
                    NotificationID = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    BranchCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    IsDismissed = table.Column<bool>(type: "bit", nullable: false),
                    ReadDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationReceiver", x => new { x.NotificationID, x.UserID, x.BranchCode });
                    table.ForeignKey(
                        name: "FK_NotificationReceiver_Notification_NotificationID_BranchCode",
                        columns: x => new { x.NotificationID, x.BranchCode },
                        principalTable: "Notification",
                        principalColumns: new[] { "NotificationID", "BranchCode" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SystemCodeItem",
                columns: table => new
                {
                    CodeClass = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    CodeName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    UserDefinedCode1 = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    UserDefinedCode2 = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    UserDefinedCode3 = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemCodeItem", x => new { x.CodeClass, x.Code });
                    table.ForeignKey(
                        name: "FK_SystemCodeItem_TISystemCode_CodeClass",
                        column: x => x.CodeClass,
                        principalTable: "TISystemCode",
                        principalColumn: "CodeClass",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImportItem_BranchCode",
                table: "ImportItem",
                column: "BranchCode");

            migrationBuilder.CreateIndex(
                name: "IX_KitProduct_KitProdCode_BranchCode",
                table: "KitProduct",
                columns: new[] { "KitProdCode", "BranchCode" });

            migrationBuilder.CreateIndex(
                name: "IX_Notification_BranchCode",
                table: "Notification",
                column: "BranchCode");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationReceiver_NotificationID_BranchCode",
                table: "NotificationReceiver",
                columns: new[] { "NotificationID", "BranchCode" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImportItem");

            migrationBuilder.DropTable(
                name: "KitProduct");

            migrationBuilder.DropTable(
                name: "NotificationReceiver");

            migrationBuilder.DropTable(
                name: "PurchaseItem");

            migrationBuilder.DropTable(
                name: "SystemCodeItem");

            migrationBuilder.DropTable(
                name: "Notification");

            migrationBuilder.DropTable(
                name: "TISystemCode");

            migrationBuilder.DropColumn(
                name: "ExtraPayload",
                table: "SalesTrxData");

            migrationBuilder.DropColumn(
                name: "MappingPayload",
                table: "PurchTrxData");

            migrationBuilder.DropColumn(
                name: "MappingTime",
                table: "PurchTrxData");

            migrationBuilder.DropColumn(
                name: "IsKitting",
                table: "Product");

            migrationBuilder.AlterColumn<string>(
                name: "QtyUnitCode",
                table: "SalesItem",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(8)",
                oldMaxLength: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PkgUnitCode",
                table: "SalesItem",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(8)",
                oldMaxLength: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RespPayload",
                table: "ApiRequestLog",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReqPayload",
                table: "ApiRequestLog",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
