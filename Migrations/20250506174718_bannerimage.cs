using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HouseOfWani.Migrations
{
    /// <inheritdoc />
    public partial class bannerimage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "BannerImages",
                newName: "SummerImage1");

            migrationBuilder.AddColumn<string>(
                name: "SummerImage2",
                table: "BannerImages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SummerImage2",
                table: "BannerImages");

            migrationBuilder.RenameColumn(
                name: "SummerImage1",
                table: "BannerImages",
                newName: "ImageUrl");
        }
    }
}
