using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WF.TransactionService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTransactionDbContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TransferRequests",
                table: "TransferRequests");

            migrationBuilder.RenameTable(
                name: "TransferRequests",
                newName: "Transactions");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Transactions",
                table: "Transactions",
                column: "CorrelationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Transactions",
                table: "Transactions");

            migrationBuilder.RenameTable(
                name: "Transactions",
                newName: "TransferRequests");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TransferRequests",
                table: "TransferRequests",
                column: "CorrelationId");
        }
    }
}
