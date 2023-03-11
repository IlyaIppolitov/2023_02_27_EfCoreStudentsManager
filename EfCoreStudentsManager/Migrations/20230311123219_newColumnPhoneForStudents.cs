using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EfCoreStudentsManager.Migrations
{
    public partial class newColumnPhoneForStudents : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Students_Name_Email",
                table: "Students");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Students",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Students_Name_Phone_Email",
                table: "Students",
                columns: new[] { "Name", "Phone", "Email" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Students_Name_Phone_Email",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Students");

            migrationBuilder.CreateIndex(
                name: "IX_Students_Name_Email",
                table: "Students",
                columns: new[] { "Name", "Email" });
        }
    }
}
