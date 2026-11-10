using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ListLift.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddListingDraftQuantity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "ListingDrafts",
                type: "int",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "ListingDrafts");
        }
    }
}
