using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WF.CustomerService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWalletNumberToReadModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WalletNumber",
                table: "WalletReadModels",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WalletNumber",
                table: "WalletReadModels");
        }
    }
}
