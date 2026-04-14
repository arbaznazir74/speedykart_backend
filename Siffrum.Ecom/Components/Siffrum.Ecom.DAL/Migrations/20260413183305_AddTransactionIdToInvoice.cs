using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Siffrum.Ecom.DAL.Migrations
{
    public partial class AddTransactionIdToInvoice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "transaction_id",
                table: "invoice",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            // Backfill existing invoices with their order's transaction_id
            migrationBuilder.Sql(
                "UPDATE invoice SET transaction_id = o.transaction_id FROM orders o WHERE invoice.order_id = o.id AND invoice.transaction_id = 0;");

            migrationBuilder.CreateIndex(
                name: "IX_invoice_transaction_id",
                table: "invoice",
                column: "transaction_id",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_invoice_transaction_id",
                table: "invoice");

            migrationBuilder.DropColumn(
                name: "transaction_id",
                table: "invoice");
        }
    }
}
