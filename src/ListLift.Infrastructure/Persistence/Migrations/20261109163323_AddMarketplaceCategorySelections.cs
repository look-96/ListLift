using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ListLift.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMarketplaceCategorySelections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ListingCategorySelections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ListingDraftId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Channel = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CategoryName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CategoryPath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    SelectedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListingCategorySelections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ListingCategorySelections_ListingDrafts_ListingDraftId",
                        column: x => x.ListingDraftId,
                        principalTable: "ListingDrafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ListingCategorySelections_ListingDraftId_Channel",
                table: "ListingCategorySelections",
                columns: new[] { "ListingDraftId", "Channel" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ListingCategorySelections");
        }
    }
}
