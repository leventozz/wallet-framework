using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WF.CustomerService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReadModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WalletReadModels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Balance = table.Column<decimal>(type: "numeric", nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletReadModels", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WalletReadModels_CustomerId",
                table: "WalletReadModels",
                column: "CustomerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WalletReadModels");
        }
    }
}
