using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ListLift.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProtectedEbayApplicationSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EbayApplicationSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProtectedClientId = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    ProtectedClientSecret = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EbayApplicationSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EbayApplicationSettings");
        }
    }
}
