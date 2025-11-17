using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iTaxSuite.Library.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApiRequestLog",
                columns: table => new
                {
                    RequestID = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AppName = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Direction = table.Column<int>(type: "int", nullable: false),
                    Method = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    ReqPath = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    IPAddress = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ReqHeaders = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    QParams = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ReqPayload = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    RequestAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RequestBy = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    StatusCode = table.Column<int>(type: "int", nullable: false),
                    RespHeaders = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    RespPayload = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ResponseAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Duration = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiRequestLog", x => x.RequestID);
                });

            migrationBuilder.CreateTable(
                name: "EntityAttribute",
                columns: table => new
                {
                    AttributeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1001, 1"),
                    EntityType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    SearchKey = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    EntityKey = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    ExtraKey = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ExtraValue = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityAttribute", x => x.AttributeID);
                });

            migrationBuilder.CreateTable(
                name: "ExtSystConfig",
                columns: table => new
                {
                    SystemCode = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    SystemType = table.Column<int>(type: "int", nullable: false),
                    ApiAddress = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ExtApiAddress = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Username = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Password = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    AuthToken = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    LockColumn = table.Column<string>(type: "char(1)", maxLength: 1, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtSystConfig", x => x.SystemCode);
                    table.CheckConstraint("CK_SingleRowOnly", "[LockColumn] = 'X'");
                });

            migrationBuilder.CreateTable(
                name: "Product",
                columns: table => new
                {
                    ProductCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    IsStockable = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ItemClassCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    PackageUnit = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    QuantityUnit = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
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
                    table.PrimaryKey("PK_Product", x => x.ProductCode);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    RoleID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.RoleID);
                });

            migrationBuilder.CreateTable(
                name: "S300TaxAuthority",
                columns: table => new
                {
                    AuthorityKey = table.Column<string>(type: "nvarchar(48)", maxLength: 48, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    TaxType = table.Column<int>(type: "int", nullable: false),
                    TaxBase = table.Column<int>(type: "int", nullable: false),
                    LastMaintained = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_S300TaxAuthority", x => x.AuthorityKey);
                });

            migrationBuilder.CreateTable(
                name: "SyncChannel",
                columns: table => new
                {
                    ChannelId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    ChannelName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TriggerType = table.Column<int>(type: "int", nullable: false),
                    TriggerExpr = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    LastAccess = table.Column<DateTime>(type: "datetime2", nullable: true),
                    KeyCol = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    UniqIDCol = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    DateCol = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    SyncTrackExpr = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    LastTracked = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncChannel", x => x.ChannelId);
                });

            migrationBuilder.CreateTable(
                name: "SysUser",
                columns: table => new
                {
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DistributorCode = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: true),
                    EmployeeCode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    SalesPersonKey = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    ProcessMeta = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    LastLoginDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysUser", x => x.UserID);
                });

            migrationBuilder.CreateTable(
                name: "TaxClient",
                columns: table => new
                {
                    ClientCode = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ClientName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TaxNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    SystemCode = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    LockColumn = table.Column<string>(type: "char(1)", maxLength: 1, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxClient", x => x.ClientCode);
                    table.CheckConstraint("CK_SingleRowOnly1", "[LockColumn] = 'X'");
                    table.ForeignKey(
                        name: "FK_TaxClient_ExtSystConfig_SystemCode",
                        column: x => x.SystemCode,
                        principalTable: "ExtSystConfig",
                        principalColumn: "SystemCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductData",
                columns: table => new
                {
                    ProductCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    SourceStamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SourcePayload = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    RequestTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RequestPayload = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ResponseTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResponsePayload = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductData", x => x.ProductCode);
                    table.ForeignKey(
                        name: "FK_ProductData_Product_ProductCode",
                        column: x => x.ProductCode,
                        principalTable: "Product",
                        principalColumn: "ProductCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoleClaim",
                columns: table => new
                {
                    RoleClaimID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleClaim", x => x.RoleClaimID);
                    table.ForeignKey(
                        name: "FK_RoleClaim_Role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "RoleID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserClaim",
                columns: table => new
                {
                    UserClaimID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserClaim", x => x.UserClaimID);
                    table.ForeignKey(
                        name: "FK_UserClaim_SysUser_UserId",
                        column: x => x.UserId,
                        principalTable: "SysUser",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLogin",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLogin", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_UserLogin_SysUser_UserId",
                        column: x => x.UserId,
                        principalTable: "SysUser",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRole",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRole", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRole_Role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "RoleID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRole_SysUser_UserId",
                        column: x => x.UserId,
                        principalTable: "SysUser",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserToken",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserToken", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_UserToken_SysUser_UserId",
                        column: x => x.UserId,
                        principalTable: "SysUser",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientBranch",
                columns: table => new
                {
                    ClientCode = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    BranchCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    BranchName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EtrAddress = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    SecAddress = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    EtrSerialNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProductSeq = table.Column<int>(type: "int", nullable: false),
                    SaleInvoiceSeq = table.Column<int>(type: "int", nullable: false),
                    PurchInvoiceSeq = table.Column<int>(type: "int", nullable: false),
                    InitializedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InitRequest = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    InitResponse = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
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
                    table.PrimaryKey("PK_ClientBranch", x => new { x.ClientCode, x.BranchCode });
                    table.UniqueConstraint("AK_ClientBranch_BranchCode", x => x.BranchCode);
                    table.ForeignKey(
                        name: "FK_ClientBranch_TaxClient_ClientCode",
                        column: x => x.ClientCode,
                        principalTable: "TaxClient",
                        principalColumn: "ClientCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BranchCustomer",
                columns: table => new
                {
                    BranchCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    CustNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    CustName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    CustTaxNumber = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    RecordStatus = table.Column<int>(type: "int", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    Tries = table.Column<int>(type: "int", nullable: false),
                    LastTry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SourceStamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SourcePayload = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    RequestTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RequestPayload = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ResponseTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResponsePayload = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchCustomer", x => new { x.CustNumber, x.BranchCode });
                    table.ForeignKey(
                        name: "FK_BranchCustomer_ClientBranch_BranchCode",
                        column: x => x.BranchCode,
                        principalTable: "ClientBranch",
                        principalColumn: "BranchCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BranchUser",
                columns: table => new
                {
                    BranchCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    UserNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    UserPass = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    RecordStatus = table.Column<int>(type: "int", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    Tries = table.Column<int>(type: "int", nullable: false),
                    LastTry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SourceStamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SourcePayload = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    RequestTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RequestPayload = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ResponseTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResponsePayload = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchUser", x => new { x.UserNumber, x.BranchCode });
                    table.ForeignKey(
                        name: "FK_BranchUser_ClientBranch_BranchCode",
                        column: x => x.BranchCode,
                        principalTable: "ClientBranch",
                        principalColumn: "BranchCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EtimsTransact",
                columns: table => new
                {
                    BranchCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    ReqType = table.Column<int>(type: "int", nullable: false),
                    DocNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    DocStamp = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EtimsTrxID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1001, 1"),
                    SourceApp = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ReqAddress = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ReqKey = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ParentKey = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    IsNewRequest = table.Column<bool>(type: "bit", nullable: false),
                    NextSeqNumber = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_EtimsTransact", x => new { x.DocNumber, x.ReqType, x.BranchCode, x.DocStamp });
                    table.ForeignKey(
                        name: "FK_EtimsTransact_ClientBranch_BranchCode",
                        column: x => x.BranchCode,
                        principalTable: "ClientBranch",
                        principalColumn: "BranchCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchTransact",
                columns: table => new
                {
                    BranchCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    DocNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    PurchaseID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1001, 1"),
                    DocStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SourceApp = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    EtrSeqNumber = table.Column<int>(type: "int", nullable: false),
                    VendorNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    VendorName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    VendorTaxNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Desciption = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
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
                    table.PrimaryKey("PK_PurchTransact", x => new { x.DocNumber, x.BranchCode });
                    table.UniqueConstraint("AK_PurchTransact_PurchaseID", x => x.PurchaseID);
                    table.ForeignKey(
                        name: "FK_PurchTransact_ClientBranch_BranchCode",
                        column: x => x.BranchCode,
                        principalTable: "ClientBranch",
                        principalColumn: "BranchCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SalesTransact",
                columns: table => new
                {
                    BranchCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    DocNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SalesTrxID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1001, 1"),
                    DocStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DocType = table.Column<int>(type: "int", nullable: false),
                    SourceApp = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    EtrSeqNumber = table.Column<int>(type: "int", nullable: false),
                    CustNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    CustName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CustTaxNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Desciption = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    RefInvNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    DocSrcCurr = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    DocHomeCurr = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    DocExchRate = table.Column<decimal>(type: "decimal(19,3)", precision: 19, scale: 3, nullable: false),
                    DocRateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SrcAmtWTX = table.Column<decimal>(type: "decimal(19,3)", precision: 19, scale: 3, nullable: false),
                    HomeAmtWTX = table.Column<decimal>(type: "decimal(19,3)", precision: 19, scale: 3, nullable: false),
                    SrcDiscWTX = table.Column<decimal>(type: "decimal(19,3)", precision: 19, scale: 3, nullable: false),
                    HomeDiscWTX = table.Column<decimal>(type: "decimal(19,3)", precision: 19, scale: 3, nullable: false),
                    CUNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    RefCUNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    QRText = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    QRTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    QRImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_SalesTransact", x => new { x.DocNumber, x.BranchCode });
                    table.UniqueConstraint("AK_SalesTransact_SalesTrxID", x => x.SalesTrxID);
                    table.ForeignKey(
                        name: "FK_SalesTransact_ClientBranch_BranchCode",
                        column: x => x.BranchCode,
                        principalTable: "ClientBranch",
                        principalColumn: "BranchCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockItem",
                columns: table => new
                {
                    ProductCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    BranchCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    LastStockSave = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastMovement = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TaxItemCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    EtrSeqNumber = table.Column<int>(type: "int", nullable: false),
                    StockCount = table.Column<int>(type: "int", nullable: false),
                    CountTime = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_StockItem", x => new { x.ProductCode, x.BranchCode });
                    table.ForeignKey(
                        name: "FK_StockItem_ClientBranch_BranchCode",
                        column: x => x.BranchCode,
                        principalTable: "ClientBranch",
                        principalColumn: "BranchCode",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StockItem_Product_ProductCode",
                        column: x => x.ProductCode,
                        principalTable: "Product",
                        principalColumn: "ProductCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockMovement",
                columns: table => new
                {
                    ProductCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    BranchCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    MovementID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1001, 1"),
                    MovementType = table.Column<int>(type: "int", nullable: false),
                    EtrSeqNumber = table.Column<int>(type: "int", nullable: false),
                    SourceType = table.Column<int>(type: "int", nullable: false),
                    DocNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    DocDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
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
                    table.PrimaryKey("PK_StockMovement", x => new { x.ProductCode, x.BranchCode });
                    table.UniqueConstraint("AK_StockMovement_MovementID", x => x.MovementID);
                    table.ForeignKey(
                        name: "FK_StockMovement_ClientBranch_BranchCode",
                        column: x => x.BranchCode,
                        principalTable: "ClientBranch",
                        principalColumn: "BranchCode",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StockMovement_Product_ProductCode",
                        column: x => x.ProductCode,
                        principalTable: "Product",
                        principalColumn: "ProductCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchTrxData",
                columns: table => new
                {
                    PurchaseID = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    SourceStamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SourcePayload = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    RequestTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RequestPayload = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ResponseTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResponsePayload = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchTrxData", x => x.PurchaseID);
                    table.ForeignKey(
                        name: "FK_PurchTrxData_PurchTransact_PurchaseID",
                        column: x => x.PurchaseID,
                        principalTable: "PurchTransact",
                        principalColumn: "PurchaseID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SalesInvData",
                columns: table => new
                {
                    SalesTrxID = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    SourceStamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SourcePayload = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    RequestTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RequestPayload = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ResponseTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResponsePayload = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesInvData", x => x.SalesTrxID);
                    table.ForeignKey(
                        name: "FK_SalesInvData_SalesTransact_SalesTrxID",
                        column: x => x.SalesTrxID,
                        principalTable: "SalesTransact",
                        principalColumn: "SalesTrxID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockMovData",
                columns: table => new
                {
                    MovementID = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    SourceStamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SourcePayload = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    RequestTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RequestPayload = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ResponseTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResponsePayload = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockMovData", x => x.MovementID);
                    table.ForeignKey(
                        name: "FK_StockMovData_StockMovement_MovementID",
                        column: x => x.MovementID,
                        principalTable: "StockMovement",
                        principalColumn: "MovementID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BranchCustomer_BranchCode",
                table: "BranchCustomer",
                column: "BranchCode");

            migrationBuilder.CreateIndex(
                name: "IX_BranchUser_BranchCode",
                table: "BranchUser",
                column: "BranchCode");

            migrationBuilder.CreateIndex(
                name: "IX_EtimsTransact_BranchCode",
                table: "EtimsTransact",
                column: "BranchCode");

            migrationBuilder.CreateIndex(
                name: "IX_EtimsTransact_EtimsTrxID",
                table: "EtimsTransact",
                column: "EtimsTrxID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExtSystConfig_LockColumn",
                table: "ExtSystConfig",
                column: "LockColumn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchTransact_BranchCode",
                table: "PurchTransact",
                column: "BranchCode");

            migrationBuilder.CreateIndex(
                name: "IX_PurchTransact_PurchaseID",
                table: "PurchTransact",
                column: "PurchaseID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "Role",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RoleClaim_RoleId",
                table: "RoleClaim",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesTransact_BranchCode",
                table: "SalesTransact",
                column: "BranchCode");

            migrationBuilder.CreateIndex(
                name: "IX_SalesTransact_SalesTrxID",
                table: "SalesTransact",
                column: "SalesTrxID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockItem_BranchCode",
                table: "StockItem",
                column: "BranchCode");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovement_BranchCode",
                table: "StockMovement",
                column: "BranchCode");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovement_MovementID",
                table: "StockMovement",
                column: "MovementID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "SysUser",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "SysUser",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TaxClient_LockColumn",
                table: "TaxClient",
                column: "LockColumn",
                unique: true,
                filter: "[LockColumn] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TaxClient_SystemCode",
                table: "TaxClient",
                column: "SystemCode");

            migrationBuilder.CreateIndex(
                name: "IX_UserClaim_UserId",
                table: "UserClaim",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLogin_UserId",
                table: "UserLogin",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_RoleId",
                table: "UserRole",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiRequestLog");

            migrationBuilder.DropTable(
                name: "BranchCustomer");

            migrationBuilder.DropTable(
                name: "BranchUser");

            migrationBuilder.DropTable(
                name: "EntityAttribute");

            migrationBuilder.DropTable(
                name: "EtimsTransact");

            migrationBuilder.DropTable(
                name: "ProductData");

            migrationBuilder.DropTable(
                name: "PurchTrxData");

            migrationBuilder.DropTable(
                name: "RoleClaim");

            migrationBuilder.DropTable(
                name: "S300TaxAuthority");

            migrationBuilder.DropTable(
                name: "SalesInvData");

            migrationBuilder.DropTable(
                name: "StockItem");

            migrationBuilder.DropTable(
                name: "StockMovData");

            migrationBuilder.DropTable(
                name: "SyncChannel");

            migrationBuilder.DropTable(
                name: "UserClaim");

            migrationBuilder.DropTable(
                name: "UserLogin");

            migrationBuilder.DropTable(
                name: "UserRole");

            migrationBuilder.DropTable(
                name: "UserToken");

            migrationBuilder.DropTable(
                name: "PurchTransact");

            migrationBuilder.DropTable(
                name: "SalesTransact");

            migrationBuilder.DropTable(
                name: "StockMovement");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.DropTable(
                name: "SysUser");

            migrationBuilder.DropTable(
                name: "ClientBranch");

            migrationBuilder.DropTable(
                name: "Product");

            migrationBuilder.DropTable(
                name: "TaxClient");

            migrationBuilder.DropTable(
                name: "ExtSystConfig");
        }
    }
}
