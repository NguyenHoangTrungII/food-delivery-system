using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AuthService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FunctionActions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CodeName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Module = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UrlPattern = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    HttpMethod = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FunctionScope = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FunctionActions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FunctionModules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CodeName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Level = table.Column<int>(type: "int", nullable: false),
                    ModuleType = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FunctionModules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FunctionModules_FunctionModules_ParentId",
                        column: x => x.ParentId,
                        principalTable: "FunctionModules",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Administrator = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsLocking = table.Column<bool>(type: "bit", nullable: false),
                    IsOnline = table.Column<bool>(type: "bit", nullable: false),
                    FailedPWAttempt = table.Column<int>(type: "int", nullable: false),
                    LastLogin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RefreshTokenExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FunctionID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Allowed = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePermissions_FunctionActions_FunctionID",
                        column: x => x.FunctionID,
                        principalTable: "FunctionActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "FunctionActions",
                columns: new[] { "Id", "ActionType", "CodeName", "CreatedBy", "CreatedOn", "FunctionScope", "HttpMethod", "IsActive", "ModifiedBy", "ModifiedOn", "Module", "Name", "UrlPattern" },
                values: new object[,]
                {
                    { new Guid("90000000-0000-0000-0000-000000000001"), "Create", "CREATE_ORDER", "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), "API", "POST", true, "", null, "OrderService", "Create Order", "/api/orders" },
                    { new Guid("90000000-0000-0000-0000-000000000002"), "View", "VIEW_ORDER_DETAILS", "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), "API", "GET", true, "", null, "OrderService", "View Order Details", "/api/orders/{id}" },
                    { new Guid("90000000-0000-0000-0000-000000000003"), "Update", "UPDATE_ORDER_STATUS", "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), "API", "PUT", true, "", null, "OrderService", "Update Order Status", "/api/orders/{id}/status" },
                    { new Guid("90000000-0000-0000-0000-000000000004"), "Delete", "CANCEL_ORDER", "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), "API", "DELETE", true, "", null, "OrderService", "Cancel Order", "/api/orders/{id}" },
                    { new Guid("90000000-0000-0000-0000-000000000005"), "Update", "RATE_ORDER", "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), "API", "POST", true, "", null, "OrderService", "Rate Order", "/api/orders/{id}/rate" },
                    { new Guid("90000000-0000-0000-0000-000000000006"), "Create", "CREATE_MENU", "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), "API", "POST", true, "", null, "MenuService", "Create Menu", "/api/menus" },
                    { new Guid("90000000-0000-0000-0000-000000000007"), "View", "VIEW_MENU", "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), "API", "GET", true, "", null, "MenuService", "View Menu", "/api/menus" },
                    { new Guid("90000000-0000-0000-0000-000000000008"), "Update", "UPDATE_MENU", "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), "API", "PUT", true, "", null, "MenuService", "Update Menu", "/api/menus/{id}" },
                    { new Guid("90000000-0000-0000-0000-000000000009"), "Delete", "DELETE_MENU", "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), "API", "DELETE", true, "", null, "MenuService", "Delete Menu", "/api/menus/{id}" },
                    { new Guid("90000000-0000-0000-0000-000000000010"), "Update", "ACCEPT_DELIVERY", "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), "API", "POST", true, "", null, "DeliveryPartnerService", "Accept Delivery", "/api/deliveries/{id}/accept" },
                    { new Guid("90000000-0000-0000-0000-000000000011"), "Update", "UPDATE_DELIVERY_STATUS", "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), "API", "PUT", true, "", null, "DeliveryPartnerService", "Update Delivery Status", "/api/deliveries/{id}/status" },
                    { new Guid("90000000-0000-0000-0000-000000000012"), "Update", "REASSIGN_DELIVERY", "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), "API", "PUT", true, "", null, "DeliveryPartnerService", "Reassign Delivery", "/api/deliveries/{id}/reassign" },
                    { new Guid("90000000-0000-0000-0000-000000000013"), "Manage", "MANAGE_ROLES", "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), "API", "POST", true, "", null, "AuthService", "Manage Roles", "/api/roles" },
                    { new Guid("90000000-0000-0000-0000-000000000014"), "Manage", "MANAGE_USERS", "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), "API", "POST", true, "", null, "AuthService", "Manage Users", "/api/users" }
                });

            migrationBuilder.InsertData(
                table: "FunctionModules",
                columns: new[] { "Id", "CodeName", "CreatedBy", "CreatedOn", "IsActive", "Level", "ModifiedBy", "ModifiedOn", "ModuleType", "Name", "ParentId" },
                values: new object[,]
                {
                    { new Guid("80000000-0000-0000-0000-000000000001"), "ORDER_MANAGEMENT", "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), true, 0, "", null, "M", "Order Management", null },
                    { new Guid("80000000-0000-0000-0000-000000000003"), "MENU_MANAGEMENT", "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), true, 0, "", null, "M", "Menu Management", null },
                    { new Guid("80000000-0000-0000-0000-000000000005"), "DELIVERY_MANAGEMENT", "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), true, 0, "", null, "M", "Delivery Management", null },
                    { new Guid("80000000-0000-0000-0000-000000000007"), "SYSTEM_SETTINGS", "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), true, 0, "", null, "S", "System Settings", null }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Administrator", "CreatedBy", "CreatedOn", "ModifiedBy", "ModifiedOn", "Name" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), false, "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), "", null, "Customer" },
                    { new Guid("20000000-0000-0000-0000-000000000002"), false, "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), "", null, "Driver" },
                    { new Guid("30000000-0000-0000-0000-000000000003"), false, "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), "", null, "Restaurant" },
                    { new Guid("50000000-0000-0000-0000-000000000005"), true, "system", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), "", null, "Admin" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "Email", "FailedPWAttempt", "IsLocking", "IsOnline", "LastLogin", "ModifiedBy", "ModifiedOn", "PasswordHash", "RefreshToken", "RefreshTokenExpiry", "Username" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), "admin", "john_doe@example.com", 0, false, false, null, "", null, "$2a$11$hashedpassword", "", null, "john_doe" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), "admin", "driver_1@example.com", 0, false, false, null, "", null, "$2a$11$hashedpassword", "", null, "driver_1" },
                    { new Guid("55555555-5555-5555-5555-555555555555"), new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), "system", "admin@example.com", 0, false, false, null, "", null, "$2a$11$hashedpassword", "", null, "admin" }
                });

            migrationBuilder.InsertData(
                table: "FunctionModules",
                columns: new[] { "Id", "CodeName", "CreatedBy", "CreatedOn", "IsActive", "Level", "ModifiedBy", "ModifiedOn", "ModuleType", "Name", "ParentId" },
                values: new object[,]
                {
                    { new Guid("80000000-0000-0000-0000-000000000002"), "ORDER_OPERATIONS", "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), true, 1, "", null, "G", "Order Operations", new Guid("80000000-0000-0000-0000-000000000001") },
                    { new Guid("80000000-0000-0000-0000-000000000004"), "MENU_OPERATIONS", "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), true, 1, "", null, "G", "Menu Operations", new Guid("80000000-0000-0000-0000-000000000003") },
                    { new Guid("80000000-0000-0000-0000-000000000006"), "DELIVERY_OPERATIONS", "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), true, 1, "", null, "G", "Delivery Operations", new Guid("80000000-0000-0000-0000-000000000005") }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "Id", "Allowed", "CreatedBy", "CreatedOn", "FunctionID", "ModifiedBy", "ModifiedOn", "RoleId" },
                values: new object[,]
                {
                    { new Guid("70000000-0000-0000-0000-000000000001"), true, "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), new Guid("90000000-0000-0000-0000-000000000001"), "", null, new Guid("10000000-0000-0000-0000-000000000001") },
                    { new Guid("70000000-0000-0000-0000-000000000002"), true, "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), new Guid("90000000-0000-0000-0000-000000000002"), "", null, new Guid("10000000-0000-0000-0000-000000000001") },
                    { new Guid("70000000-0000-0000-0000-000000000003"), false, "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), new Guid("90000000-0000-0000-0000-000000000003"), "", null, new Guid("10000000-0000-0000-0000-000000000001") },
                    { new Guid("70000000-0000-0000-0000-000000000004"), true, "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), new Guid("90000000-0000-0000-0000-000000000004"), "", null, new Guid("10000000-0000-0000-0000-000000000001") },
                    { new Guid("70000000-0000-0000-0000-000000000005"), true, "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), new Guid("90000000-0000-0000-0000-000000000005"), "", null, new Guid("10000000-0000-0000-0000-000000000001") },
                    { new Guid("70000000-0000-0000-0000-000000000006"), true, "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), new Guid("90000000-0000-0000-0000-000000000007"), "", null, new Guid("10000000-0000-0000-0000-000000000001") },
                    { new Guid("70000000-0000-0000-0000-000000000007"), true, "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), new Guid("90000000-0000-0000-0000-000000000002"), "", null, new Guid("20000000-0000-0000-0000-000000000002") },
                    { new Guid("70000000-0000-0000-0000-000000000008"), true, "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), new Guid("90000000-0000-0000-0000-000000000003"), "", null, new Guid("20000000-0000-0000-0000-000000000002") },
                    { new Guid("70000000-0000-0000-0000-000000000009"), true, "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), new Guid("90000000-0000-0000-0000-000000000004"), "", null, new Guid("20000000-0000-0000-0000-000000000002") },
                    { new Guid("70000000-0000-0000-0000-000000000010"), true, "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), new Guid("90000000-0000-0000-0000-000000000010"), "", null, new Guid("20000000-0000-0000-0000-000000000002") },
                    { new Guid("70000000-0000-0000-0000-000000000011"), true, "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), new Guid("90000000-0000-0000-0000-000000000011"), "", null, new Guid("20000000-0000-0000-0000-000000000002") },
                    { new Guid("70000000-0000-0000-0000-000000000012"), false, "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), new Guid("90000000-0000-0000-0000-000000000012"), "", null, new Guid("20000000-0000-0000-0000-000000000002") },
                    { new Guid("70000000-0000-0000-0000-000000000013"), true, "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), new Guid("90000000-0000-0000-0000-000000000002"), "", null, new Guid("30000000-0000-0000-0000-000000000003") },
                    { new Guid("70000000-0000-0000-0000-000000000014"), true, "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), new Guid("90000000-0000-0000-0000-000000000003"), "", null, new Guid("30000000-0000-0000-0000-000000000003") },
                    { new Guid("70000000-0000-0000-0000-000000000015"), true, "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), new Guid("90000000-0000-0000-0000-000000000006"), "", null, new Guid("30000000-0000-0000-0000-000000000003") },
                    { new Guid("70000000-0000-0000-0000-000000000016"), true, "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), new Guid("90000000-0000-0000-0000-000000000007"), "", null, new Guid("30000000-0000-0000-0000-000000000003") },
                    { new Guid("70000000-0000-0000-0000-000000000017"), true, "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), new Guid("90000000-0000-0000-0000-000000000008"), "", null, new Guid("30000000-0000-0000-0000-000000000003") },
                    { new Guid("70000000-0000-0000-0000-000000000018"), true, "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), new Guid("90000000-0000-0000-0000-000000000009"), "", null, new Guid("30000000-0000-0000-0000-000000000003") },
                    { new Guid("70000000-0000-0000-0000-000000000019"), true, "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), new Guid("90000000-0000-0000-0000-000000000001"), "", null, new Guid("50000000-0000-0000-0000-000000000005") },
                    { new Guid("70000000-0000-0000-0000-000000000020"), true, "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), new Guid("90000000-0000-0000-0000-000000000002"), "", null, new Guid("50000000-0000-0000-0000-000000000005") },
                    { new Guid("70000000-0000-0000-0000-000000000021"), true, "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), new Guid("90000000-0000-0000-0000-000000000003"), "", null, new Guid("50000000-0000-0000-0000-000000000005") },
                    { new Guid("70000000-0000-0000-0000-000000000022"), true, "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), new Guid("90000000-0000-0000-0000-000000000004"), "", null, new Guid("50000000-0000-0000-0000-000000000005") },
                    { new Guid("70000000-0000-0000-0000-000000000023"), true, "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), new Guid("90000000-0000-0000-0000-000000000013"), "", null, new Guid("50000000-0000-0000-0000-000000000005") },
                    { new Guid("70000000-0000-0000-0000-000000000024"), true, "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), new Guid("90000000-0000-0000-0000-000000000014"), "", null, new Guid("50000000-0000-0000-0000-000000000005") }
                });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "Id", "CreatedBy", "CreatedOn", "EndDate", "ModifiedBy", "ModifiedOn", "RoleId", "StartDate", "UserId" },
                values: new object[,]
                {
                    { new Guid("60000000-0000-0000-0000-000000000001"), "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), null, "", null, new Guid("10000000-0000-0000-0000-000000000001"), new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("60000000-0000-0000-0000-000000000002"), "admin", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), null, "", null, new Guid("20000000-0000-0000-0000-000000000002"), new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("60000000-0000-0000-0000-000000000003"), "system", new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), null, "", null, new Guid("50000000-0000-0000-0000-000000000005"), new DateTime(2025, 5, 28, 12, 24, 0, 0, DateTimeKind.Unspecified), new Guid("55555555-5555-5555-5555-555555555555") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_FunctionActions_CodeName",
                table: "FunctionActions",
                column: "CodeName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FunctionModules_CodeName",
                table: "FunctionModules",
                column: "CodeName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FunctionModules_ParentId",
                table: "FunctionModules",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_FunctionID",
                table: "RolePermissions",
                column: "FunctionID");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId",
                table: "RolePermissions",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId",
                table: "UserRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FunctionModules");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "FunctionActions");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
