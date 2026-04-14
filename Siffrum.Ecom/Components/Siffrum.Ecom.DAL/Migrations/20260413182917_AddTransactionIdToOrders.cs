using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Siffrum.Ecom.DAL.Migrations
{
    public partial class AddTransactionIdToOrders : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "transaction_id",
                table: "orders",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            // Backfill existing rows with unique values so unique index can be created
            migrationBuilder.Sql(
                "UPDATE orders SET transaction_id = (EXTRACT(EPOCH FROM NOW())::bigint * 1000000) + id WHERE transaction_id = 0;");

            migrationBuilder.CreateIndex(
                name: "IX_orders_transaction_id",
                table: "orders",
                column: "transaction_id",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_orders_transaction_id",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "transaction_id",
                table: "orders");
        }
    }
}
