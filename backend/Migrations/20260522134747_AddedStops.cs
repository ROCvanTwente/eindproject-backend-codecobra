using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddedStops : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Stops",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QrCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Location_Nl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Location_En = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title_Nl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title_En = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description_Nl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description_En = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MediaUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Estimated_Duration = table.Column<int>(type: "int", nullable: false),
                    Map_X = table.Column<int>(type: "int", nullable: false),
                    Map_Y = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stops", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Stops");
        }
    }
}
