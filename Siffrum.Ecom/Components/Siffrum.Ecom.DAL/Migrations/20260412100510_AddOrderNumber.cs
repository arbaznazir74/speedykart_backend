using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Siffrum.Ecom.DAL.Migrations
{
    public partial class AddOrderNumber : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "assigned_seller_id",
                table: "users",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "assigned_store_id",
                table: "users",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "seller_assigned_at",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "order_number",
                table: "orders",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "selected_addons",
                table: "order_items",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "selected_toppings",
                table: "order_items",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "addons_total",
                table: "cart_items",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "selected_addons_json",
                table: "cart_items",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "selected_toppings_json",
                table: "cart_items",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "toppings_total",
                table: "cart_items",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "category_sellers",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    category_id = table.Column<long>(type: "bigint", nullable: false),
                    seller_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_category_sellers", x => x.id);
                    table.ForeignKey(
                        name: "FK_category_sellers_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_category_sellers_sellers_seller_id",
                        column: x => x.seller_id,
                        principalTable: "sellers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_orders_order_number",
                table: "orders",
                column: "order_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_category_sellers_category_id_seller_id",
                table: "category_sellers",
                columns: new[] { "category_id", "seller_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_category_sellers_seller_id",
                table: "category_sellers",
                column: "seller_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "category_sellers");

            migrationBuilder.DropIndex(
                name: "IX_orders_order_number",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "assigned_seller_id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "assigned_store_id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "seller_assigned_at",
                table: "users");

            migrationBuilder.DropColumn(
                name: "order_number",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "selected_addons",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "selected_toppings",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "addons_total",
                table: "cart_items");

            migrationBuilder.DropColumn(
                name: "selected_addons_json",
                table: "cart_items");

            migrationBuilder.DropColumn(
                name: "selected_toppings_json",
                table: "cart_items");

            migrationBuilder.DropColumn(
                name: "toppings_total",
                table: "cart_items");
        }
    }
}
