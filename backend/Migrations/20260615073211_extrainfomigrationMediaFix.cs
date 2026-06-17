using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class extrainfomigrationMediaFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExtraInformationMedia");

            migrationBuilder.AddColumn<int>(
                name: "ExtraInformationId",
                table: "Medias",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Medias_ExtraInformationId",
                table: "Medias",
                column: "ExtraInformationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Medias_ExtraInformations_ExtraInformationId",
                table: "Medias",
                column: "ExtraInformationId",
                principalTable: "ExtraInformations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Medias_ExtraInformations_ExtraInformationId",
                table: "Medias");

            migrationBuilder.DropIndex(
                name: "IX_Medias_ExtraInformationId",
                table: "Medias");

            migrationBuilder.DropColumn(
                name: "ExtraInformationId",
                table: "Medias");

            migrationBuilder.CreateTable(
                name: "ExtraInformationMedia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExtraInformationId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtraInformationMedia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExtraInformationMedia_ExtraInformations_ExtraInformationId",
                        column: x => x.ExtraInformationId,
                        principalTable: "ExtraInformations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExtraInformationMedia_ExtraInformationId",
                table: "ExtraInformationMedia",
                column: "ExtraInformationId");
        }
    }
}
