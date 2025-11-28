using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WF.TransactionService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExpirationTokenId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ExpirationTokenId",
                table: "Transactions",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpirationTokenId",
                table: "Transactions");
        }
    }
}
