using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WF.TransactionService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveWalletNumberProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceiverWalletNumber",
                table: "TransferRequests");

            migrationBuilder.DropColumn(
                name: "SenderWalletNumber",
                table: "TransferRequests");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReceiverWalletNumber",
                table: "TransferRequests",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SenderWalletNumber",
                table: "TransferRequests",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
