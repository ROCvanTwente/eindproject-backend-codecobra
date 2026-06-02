using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddedGuid : Migration
    {
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			// 1. Drop the Primary Key constraint first
			migrationBuilder.DropPrimaryKey(
				name: "PK_TourStops",
				table: "TourStops");

			// 2. Drop the old integer Identity column
			migrationBuilder.DropColumn(
				name: "Id",
				table: "TourStops");

			// 3. Add the new Guid column
			migrationBuilder.AddColumn<Guid>(
				name: "Id",
				table: "TourStops",
				type: "uniqueidentifier",
				nullable: false,
				defaultValueSql: "NEWID()"); // Generates a new GUID automatically for new rows

			// 4. Re-create the Primary Key constraint on the new column
			migrationBuilder.AddPrimaryKey(
				name: "PK_TourStops",
				table: "TourStops",
				column: "Id");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			// Reverse the process for rolling back

			migrationBuilder.DropPrimaryKey(
				name: "PK_TourStops",
				table: "TourStops");

			migrationBuilder.DropColumn(
				name: "Id",
				table: "TourStops");

			migrationBuilder.AddColumn<int>(
				name: "Id",
				table: "TourStops",
				type: "int",
				nullable: false)
				.Annotation("SqlServer:Identity", "1, 1");

			migrationBuilder.AddPrimaryKey(
				name: "PK_TourStops",
				table: "TourStops",
				column: "Id");
		}
	}
}
