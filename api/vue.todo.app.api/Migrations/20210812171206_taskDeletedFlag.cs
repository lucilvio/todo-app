using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace vue.todo.app.api.Migrations
{
    public partial class taskDeletedFlag : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("742b72eb-31d6-449a-93e1-1b63c456837d"));

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Tasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "Name", "Password" },
                values: new object[] { new Guid("c1d41604-2ee8-4065-aca4-aa86f025bbf4"), "admin@mail.com", "Admin", "123456" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("c1d41604-2ee8-4065-aca4-aa86f025bbf4"));

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Tasks");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "Name", "Password" },
                values: new object[] { new Guid("742b72eb-31d6-449a-93e1-1b63c456837d"), "admin@mail.com", "Admin", "123456" });
        }
    }
}
