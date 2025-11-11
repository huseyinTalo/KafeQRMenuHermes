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
            migrationBuilder.DropForeignKey(
                name: "FK_MenuCategories_Cafes_CafeId",
                table: "MenuCategories");

            migrationBuilder.AddColumn<Guid>(
                name: "MenuId",
                table: "ImageFiles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Menus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MenuName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ImageFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CafeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Menus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Menus_Cafes_CafeId",
                        column: x => x.CafeId,
                        principalTable: "Cafes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MenuMenuCategories",
                columns: table => new
                {
                    CategoriesOfMenuId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MenusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuMenuCategories", x => new { x.CategoriesOfMenuId, x.MenusId });
                    table.ForeignKey(
                        name: "FK_MenuMenuCategories_MenuCategories_CategoriesOfMenuId",
                        column: x => x.CategoriesOfMenuId,
                        principalTable: "MenuCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MenuMenuCategories_Menus_MenusId",
                        column: x => x.MenusId,
                        principalTable: "Menus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MenuMenuCategories_MenusId",
                table: "MenuMenuCategories",
                column: "MenusId");

            migrationBuilder.CreateIndex(
                name: "IX_Menus_CafeId",
                table: "Menus",
                column: "CafeId");

            migrationBuilder.AddForeignKey(
                name: "FK_MenuCategories_Cafes_CafeId",
                table: "MenuCategories",
                column: "CafeId",
                principalTable: "Cafes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenuCategories_Cafes_CafeId",
                table: "MenuCategories");

            migrationBuilder.DropTable(
                name: "MenuMenuCategories");

            migrationBuilder.DropTable(
                name: "Menus");

            migrationBuilder.DropColumn(
                name: "MenuId",
                table: "ImageFiles");

            migrationBuilder.AddForeignKey(
                name: "FK_MenuCategories_Cafes_CafeId",
                table: "MenuCategories",
                column: "CafeId",
                principalTable: "Cafes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
