using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ListLift.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ListingDrafts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerUserId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ConditionDescription = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    VerifiedAttributes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    PriceAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CurrencyCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListingDrafts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChannelListings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ListingDraftId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Channel = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ExternalListingId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    SafeErrorCode = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelListings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChannelListings_ListingDrafts_ListingDraftId",
                        column: x => x.ListingDraftId,
                        principalTable: "ListingDrafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ListingPhotos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ListingDraftId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StorageKey = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    MediaType = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListingPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ListingPhotos_ListingDrafts_ListingDraftId",
                        column: x => x.ListingDraftId,
                        principalTable: "ListingDrafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChannelListings_ListingDraftId_Channel",
                table: "ChannelListings",
                columns: new[] { "ListingDraftId", "Channel" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ListingDrafts_ExpiresAtUtc",
                table: "ListingDrafts",
                column: "ExpiresAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_ListingDrafts_OwnerUserId_ExpiresAtUtc",
                table: "ListingDrafts",
                columns: new[] { "OwnerUserId", "ExpiresAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ListingPhotos_ListingDraftId_SortOrder",
                table: "ListingPhotos",
                columns: new[] { "ListingDraftId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ListingPhotos_StorageKey",
                table: "ListingPhotos",
                column: "StorageKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChannelListings");

            migrationBuilder.DropTable(
                name: "ListingPhotos");

            migrationBuilder.DropTable(
                name: "ListingDrafts");
        }
    }
}
