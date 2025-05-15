using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HouseOfWani.Migrations
{
    /// <inheritdoc />
    public partial class dealOftheweek : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProductName",
                table: "DealOfTheWeeks",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                table: "DealOfTheWeeks",
                newName: "DealEndTime");

            migrationBuilder.AddColumn<string>(
                name: "DealTitle",
                table: "DealOfTheWeeks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "DealOfTheWeeks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Subtitle",
                table: "DealOfTheWeeks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "BlogPosts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DealTitle",
                table: "DealOfTheWeeks");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "DealOfTheWeeks");

            migrationBuilder.DropColumn(
                name: "Subtitle",
                table: "DealOfTheWeeks");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "DealOfTheWeeks",
                newName: "ProductName");

            migrationBuilder.RenameColumn(
                name: "DealEndTime",
                table: "DealOfTheWeeks",
                newName: "EndDate");

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "BlogPosts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
