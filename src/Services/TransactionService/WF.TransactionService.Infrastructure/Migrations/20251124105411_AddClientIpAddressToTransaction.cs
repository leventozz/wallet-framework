using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WF.TransactionService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClientIpAddressToTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClientIpAddress",
                table: "Transactions",
                type: "character varying(45)",
                maxLength: 45,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientIpAddress",
                table: "Transactions");
        }
    }
}
