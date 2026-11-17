using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iTaxSuite.Library.Migrations
{
    /// <inheritdoc />
    public partial class addItemClassCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ItemClassCode",
                columns: table => new
                {
                    ClassCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ClassName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TaxTypeCode = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    ClassLevel = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsMajorTarget = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemClassCode", x => x.ClassCode);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemClassCode");
        }
    }
}
