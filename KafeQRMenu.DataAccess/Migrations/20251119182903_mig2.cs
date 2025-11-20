using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KafeQRMenu.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class mig2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DisplayDate",
                table: "Menus",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 11, 19, 21, 29, 2, 266, DateTimeKind.Local).AddTicks(2077));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayDate",
                table: "Menus");
        }
    }
}
