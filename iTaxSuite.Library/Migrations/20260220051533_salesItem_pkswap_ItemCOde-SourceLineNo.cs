using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iTaxSuite.Library.Migrations
{
    /// <inheritdoc />
    public partial class salesItem_pkswap_ItemCOdeSourceLineNo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SalesItem",
                table: "SalesItem");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SalesItem",
                table: "SalesItem",
                columns: new[] { "SalesTrxID", "SourceLineNo", "BranchCode" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SalesItem",
                table: "SalesItem");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SalesItem",
                table: "SalesItem",
                columns: new[] { "SalesTrxID", "ProductCode", "BranchCode" });
        }
    }
}
