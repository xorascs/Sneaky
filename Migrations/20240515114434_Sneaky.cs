using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sneaky.Migrations
{
    /// <inheritdoc />
    public partial class Sneaky : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Brand",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brand", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Comparison",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comparison", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Favourite",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Favourite", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Shoe",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BrandId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Images = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ComparisonId = table.Column<int>(type: "int", nullable: true),
                    FavouriteId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shoe", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shoe_Brand_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brand",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Shoe_Comparison_ComparisonId",
                        column: x => x.ComparisonId,
                        principalTable: "Comparison",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Shoe_Favourite_FavouriteId",
                        column: x => x.FavouriteId,
                        principalTable: "Favourite",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Role = table.Column<int>(type: "int", nullable: false),
                    ComparisonId = table.Column<int>(type: "int", nullable: true),
                    FavouriteId = table.Column<int>(type: "int", nullable: true),
                    Login = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShoeId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User_Comparison_ComparisonId",
                        column: x => x.ComparisonId,
                        principalTable: "Comparison",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_User_Favourite_FavouriteId",
                        column: x => x.FavouriteId,
                        principalTable: "Favourite",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_User_Shoe_ShoeId",
                        column: x => x.ShoeId,
                        principalTable: "Shoe",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Review",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateCommentTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Review", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Review_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShoeReview",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateCommentTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoeReview", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShoeReview_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShoeShoeReview",
                columns: table => new
                {
                    ReviewsId = table.Column<int>(type: "int", nullable: false),
                    ShoesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoeShoeReview", x => new { x.ReviewsId, x.ShoesId });
                    table.ForeignKey(
                        name: "FK_ShoeShoeReview_ShoeReview_ReviewsId",
                        column: x => x.ReviewsId,
                        principalTable: "ShoeReview",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShoeShoeReview_Shoe_ShoesId",
                        column: x => x.ShoesId,
                        principalTable: "Shoe",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Review_UserId",
                table: "Review",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Shoe_BrandId",
                table: "Shoe",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_Shoe_ComparisonId",
                table: "Shoe",
                column: "ComparisonId");

            migrationBuilder.CreateIndex(
                name: "IX_Shoe_FavouriteId",
                table: "Shoe",
                column: "FavouriteId");

            migrationBuilder.CreateIndex(
                name: "IX_ShoeReview_UserId",
                table: "ShoeReview",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ShoeShoeReview_ShoesId",
                table: "ShoeShoeReview",
                column: "ShoesId");

            migrationBuilder.CreateIndex(
                name: "IX_User_ComparisonId",
                table: "User",
                column: "ComparisonId");

            migrationBuilder.CreateIndex(
                name: "IX_User_FavouriteId",
                table: "User",
                column: "FavouriteId");

            migrationBuilder.CreateIndex(
                name: "IX_User_ShoeId",
                table: "User",
                column: "ShoeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Review");

            migrationBuilder.DropTable(
                name: "ShoeShoeReview");

            migrationBuilder.DropTable(
                name: "ShoeReview");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Shoe");

            migrationBuilder.DropTable(
                name: "Brand");

            migrationBuilder.DropTable(
                name: "Comparison");

            migrationBuilder.DropTable(
                name: "Favourite");
        }
    }
}
