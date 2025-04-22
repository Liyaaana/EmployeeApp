using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeApp.Migrations
{
    public partial class AddRole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add the 'Role' column to the AspNetUsers table
            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "AspNetUsers", // Or your custom user table name
                nullable: true); // Set nullable as per your requirement
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove the 'Role' column if the migration is rolled back
            migrationBuilder.DropColumn(
                name: "Role",
                table: "AspNetUsers"); // Or your custom user table name
        }
    }
}
