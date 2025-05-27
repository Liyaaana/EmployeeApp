using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeApp.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeAuditLogAndAuditRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "EmployeeCode",
                table: "AspNetUsers",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_AspNetUsers_EmployeeCode",
                table: "AspNetUsers",
                column: "EmployeeCode");

            migrationBuilder.CreateTable(
                name: "AuditLogViewModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    EmployeeCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PageVisited = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    actionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActionTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "EmployeeAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActionTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PageVisited = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeAuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeAuditLogs_AspNetUsers_EmployeeCode",
                        column: x => x.EmployeeCode,
                        principalTable: "AspNetUsers",
                        principalColumn: "EmployeeCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAuditLogs_EmployeeCode",
                table: "EmployeeAuditLogs",
                column: "EmployeeCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogViewModel");

            migrationBuilder.DropTable(
                name: "EmployeeAuditLogs");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_AspNetUsers_EmployeeCode",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeCode",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
