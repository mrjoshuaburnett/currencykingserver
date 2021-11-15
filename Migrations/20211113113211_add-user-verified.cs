using Microsoft.EntityFrameworkCore.Migrations;

namespace CurrencyKing.Migrations
{
    public partial class adduserverified : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "UserVerified",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserVerified",
                table: "Users");
        }
    }
}
