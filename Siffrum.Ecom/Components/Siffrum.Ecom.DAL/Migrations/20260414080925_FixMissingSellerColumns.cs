using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Siffrum.Ecom.DAL.Migrations
{
    public partial class FixMissingSellerColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='sellers' AND column_name='pending_latitude') THEN
                        ALTER TABLE sellers ADD COLUMN pending_latitude numeric;
                    END IF;

                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='sellers' AND column_name='pending_longitude') THEN
                        ALTER TABLE sellers ADD COLUMN pending_longitude numeric;
                    END IF;

                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='sellers' AND column_name='location_status') THEN
                        ALTER TABLE sellers ADD COLUMN location_status smallint NOT NULL DEFAULT 0;
                    END IF;
                END
                $$;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
