using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class QrCodeNotRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TourStops_QRCodes_QRCodeId",
                table: "TourStops");

            migrationBuilder.AlterColumn<int>(
                name: "QRCodeId",
                table: "TourStops",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_TourStops_QRCodes_QRCodeId",
                table: "TourStops",
                column: "QRCodeId",
                principalTable: "QRCodes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TourStops_QRCodes_QRCodeId",
                table: "TourStops");

            migrationBuilder.AlterColumn<int>(
                name: "QRCodeId",
                table: "TourStops",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TourStops_QRCodes_QRCodeId",
                table: "TourStops",
                column: "QRCodeId",
                principalTable: "QRCodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
