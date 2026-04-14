using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Siffrum.Ecom.DAL.Migrations
{
    public partial class FixBrandIdNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                -- Make brand_id nullable (HotBox products don't have brands)
                ALTER TABLE products ALTER COLUMN brand_id DROP NOT NULL;

                -- Drop CASCADE FK and recreate with SET NULL
                ALTER TABLE products DROP CONSTRAINT IF EXISTS ""FK_products_brands_brand_id"";
                ALTER TABLE products ADD CONSTRAINT ""FK_products_brands_brand_id""
                    FOREIGN KEY (brand_id) REFERENCES brands(id) ON DELETE SET NULL;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
