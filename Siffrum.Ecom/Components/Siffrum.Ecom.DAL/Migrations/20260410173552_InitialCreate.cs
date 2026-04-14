using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Siffrum.Ecom.DAL.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "admins",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    username = table.Column<string>(type: "character varying(191)", maxLength: 191, nullable: true),
                    email = table.Column<string>(type: "character varying(191)", maxLength: 191, nullable: false),
                    password = table.Column<string>(type: "text", nullable: true),
                    forgot_password_code = table.Column<string>(type: "text", nullable: true),
                    fcm_id = table.Column<string>(type: "character varying(191)", maxLength: 191, nullable: true),
                    remember_token = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    login_status = table.Column<int>(type: "integer", nullable: false),
                    login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_active_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    role_type = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admins", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "banners",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    type = table.Column<int>(type: "integer", nullable: false),
                    banner_type = table.Column<int>(type: "integer", nullable: false),
                    platform_type = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    sub_title = table.Column<string>(type: "text", nullable: false),
                    image = table.Column<string>(type: "text", nullable: false),
                    slider_url = table.Column<string>(type: "text", nullable: true),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    priority = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_banners", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "brands",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(191)", maxLength: 191, nullable: false),
                    image = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<int>(type: "integer", maxLength: 191, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_brands", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(191)", maxLength: 191, nullable: false),
                    slug = table.Column<string>(type: "character varying(191)", maxLength: 191, nullable: true),
                    subtitle = table.Column<string>(type: "text", nullable: true),
                    image = table.Column<string>(type: "text", nullable: false),
                    web_image = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    is_system = table.Column<bool>(type: "boolean", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    level = table.Column<int>(type: "integer", nullable: false),
                    platform = table.Column<int>(type: "integer", nullable: true),
                    timings = table.Column<int>(type: "integer", nullable: true),
                    parent_category_id = table.Column<long>(type: "bigint", nullable: true),
                    meta_title = table.Column<string>(type: "character varying(191)", maxLength: 191, nullable: true),
                    meta_keywords = table.Column<string>(type: "text", nullable: true),
                    schema_markup = table.Column<string>(type: "text", nullable: true),
                    meta_description = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.id);
                    table.ForeignKey(
                        name: "FK_categories_categories_parent_category_id",
                        column: x => x.parent_category_id,
                        principalTable: "categories",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "combo_products",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    is_in_hot_box = table.Column<bool>(type: "boolean", nullable: false),
                    product_ids = table.Column<string>(type: "text", nullable: false),
                    items = table.Column<int>(type: "integer", nullable: false),
                    best_for = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    json_details = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_combo_products", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ErrorLogRoots",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LoginUserId = table.Column<string>(type: "text", nullable: true),
                    UserRoleType = table.Column<string>(type: "text", nullable: true),
                    CreatedByApp = table.Column<string>(type: "text", nullable: true),
                    CreatedOnUTC = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LogMessage = table.Column<string>(type: "text", nullable: false),
                    LogStackTrace = table.Column<string>(type: "text", nullable: true),
                    LogExceptionData = table.Column<string>(type: "text", nullable: true),
                    InnerException = table.Column<string>(type: "text", nullable: true),
                    TracingId = table.Column<string>(type: "text", nullable: true),
                    Caller = table.Column<string>(type: "text", nullable: false),
                    RequestObject = table.Column<string>(type: "text", nullable: true),
                    ResponseObject = table.Column<string>(type: "text", nullable: true),
                    AdditionalInfo = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErrorLogRoots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "external_user",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_token = table.Column<string>(type: "text", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    external_user_type = table.Column<int>(type: "integer", nullable: false),
                    role_type = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_external_user", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "faqs",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    module = table.Column<int>(type: "integer", nullable: false),
                    question = table.Column<string>(type: "text", nullable: false),
                    answer = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_faqs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "offers_and_coupons",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    base64path = table.Column<string>(type: "text", nullable: false),
                    extension_type = table.Column<int>(type: "integer", nullable: false),
                    percentage = table.Column<decimal>(type: "numeric", nullable: true),
                    platform_type = table.Column<int>(type: "integer", nullable: false),
                    offer_value = table.Column<decimal>(type: "numeric", nullable: true),
                    min_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    max_discount_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_offers_and_coupons", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "product_specification_filter",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_specification_filter", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "promo_codes",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    discount_value = table.Column<decimal>(type: "numeric", nullable: false),
                    max_discount_amount = table.Column<decimal>(type: "numeric", nullable: true),
                    minimum_cart_amount = table.Column<decimal>(type: "numeric", nullable: true),
                    usage_limit = table.Column<int>(type: "integer", nullable: false),
                    used_count = table.Column<int>(type: "integer", nullable: true),
                    usage_per_user_limit = table.Column<int>(type: "integer", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_first_order_only = table.Column<bool>(type: "boolean", nullable: false),
                    platform_type = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promo_codes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "promotional_content",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "text", nullable: false),
                    icon = table.Column<string>(type: "text", nullable: true),
                    extension = table.Column<int>(type: "integer", nullable: true),
                    platform_type = table.Column<int>(type: "integer", nullable: false),
                    display_location = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promotional_content", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "settings",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    jsondata = table.Column<string>(name: "json-data", type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_settings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tags",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(191)", maxLength: 191, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tags", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "toppings",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(191)", maxLength: 191, nullable: false),
                    image = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_toppings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "units",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(191)", maxLength: 191, nullable: false),
                    short_code = table.Column<string>(type: "character varying(191)", maxLength: 191, nullable: false),
                    parent_id = table.Column<long>(type: "bigint", nullable: true),
                    conversion = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_units", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(191)", maxLength: 191, nullable: true),
                    username = table.Column<string>(type: "character varying(191)", maxLength: 191, nullable: true),
                    email = table.Column<string>(type: "character varying(191)", maxLength: 191, nullable: true),
                    password = table.Column<string>(type: "character varying(191)", maxLength: 191, nullable: true),
                    email_verification_code = table.Column<string>(type: "character varying(191)", maxLength: 191, nullable: true),
                    profile = table.Column<string>(type: "character varying(191)", maxLength: 191, nullable: true),
                    country_code = table.Column<string>(type: "character varying(191)", maxLength: 191, nullable: true),
                    mobile = table.Column<string>(type: "character varying(191)", maxLength: 191, nullable: false),
                    balance = table.Column<double>(type: "double precision", nullable: false),
                    referral_code = table.Column<string>(type: "character varying(191)", maxLength: 191, nullable: true),
                    friends_code = table.Column<string>(type: "character varying(191)", maxLength: 191, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    login_status = table.Column<int>(type: "integer", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    payment_id = table.Column<string>(type: "character varying(191)", maxLength: 191, nullable: true),
                    pm_type = table.Column<string>(type: "character varying(191)", maxLength: 191, nullable: true),
                    pm_last_four = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    trial_ends_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    role_type = table.Column<int>(type: "integer", nullable: false),
                    device_type = table.Column<int>(type: "integer", nullable: true),
                    fcm_id = table.Column<string>(type: "text", nullable: true),
                    is_email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    is_mobile_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    otp = table.Column<int>(type: "integer", nullable: false),
                    offer_json_details = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "delivery_boys",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(191)", maxLength: 191, nullable: false),
                    username = table.Column<string>(type: "character varying(191)", maxLength: 191, nullable: true),
                    email = table.Column<string>(type: "text", nullable: false),
                    mobile = table.Column<string>(type: "character varying(191)", maxLength: 191, nullable: true),
                    order_note = table.Column<string>(type: "text", nullable: true),
                    address = table.Column<string>(type: "text", nullable: true),
                    bonus_type = table.Column<int>(type: "integer", nullable: false),
                    bonus_percentage = table.Column<double>(type: "double precision", nullable: false),
                    bonus_min_amount = table.Column<double>(type: "double precision", nullable: false),
                    bonus_max_amount = table.Column<double>(type: "double precision", nullable: false),
                    balance = table.Column<double>(type: "double precision", nullable: false),
                    driving_license = table.Column<string>(type: "text", nullable: true),
                    national_identity_card = table.Column<string>(type: "text", nullable: true),
                    dob = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    bank_account_number = table.Column<string>(type: "text", nullable: true),
                    bank_name = table.Column<string>(type: "text", nullable: true),
                    account_name = table.Column<string>(type: "text", nullable: true),
                    ifsc_code = table.Column<string>(type: "text", nullable: true),
                    other_payment_information = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    login_status = table.Column<int>(type: "integer", nullable: false),
                    is_available = table.Column<int>(type: "integer", nullable: false),
                    fcm_id = table.Column<string>(type: "character varying(191)", maxLength: 191, nullable: true),
                    pincode_id = table.Column<int>(type: "integer", nullable: true),
                    cash_received = table.Column<double>(type: "double precision", nullable: false),
                    password = table.Column<string>(type: "text", nullable: true),
                    remark = table.Column<string>(type: "text", nullable: true),
                    is_email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    is_mobile_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    payment_type = table.Column<int>(type: "integer", nullable: false),
                    admin_id = table.Column<long>(type: "bigint", nullable: true),
                    role_type = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_delivery_boys", x => x.id);
                    table.ForeignKey(
                        name: "FK_delivery_boys_admins_admin_id",
                        column: x => x.admin_id,
                        principalTable: "admins",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "sellers",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: true),
                    username = table.Column<string>(type: "text", nullable: true),
                    store_name = table.Column<string>(type: "text", nullable: true),
                    slug = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    mobile = table.Column<string>(type: "text", nullable: true),
                    balance = table.Column<double>(type: "double precision", nullable: false),
                    store_url = table.Column<string>(type: "text", nullable: true),
                    logo = table.Column<string>(type: "text", nullable: true),
                    store_description = table.Column<string>(type: "text", nullable: true),
                    street = table.Column<string>(type: "text", nullable: true),
                    pincode_id = table.Column<long>(type: "bigint", nullable: true),
                    state = table.Column<string>(type: "text", nullable: true),
                    city = table.Column<string>(type: "text", nullable: true),
                    country = table.Column<string>(type: "text", nullable: true),
                    categories = table.Column<string>(type: "text", nullable: true),
                    account_number = table.Column<string>(type: "text", nullable: true),
                    bank_ifsc_code = table.Column<string>(type: "text", nullable: true),
                    account_name = table.Column<string>(type: "text", nullable: true),
                    bank_name = table.Column<string>(type: "text", nullable: true),
                    commission = table.Column<decimal>(type: "numeric", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    login_status = table.Column<int>(type: "integer", nullable: false),
                    require_products_approval = table.Column<short>(type: "smallint", nullable: false),
                    fcm_id = table.Column<string>(type: "text", nullable: true),
                    national_identity_card = table.Column<string>(type: "text", nullable: true),
                    address_proof = table.Column<string>(type: "text", nullable: true),
                    pan_number = table.Column<string>(type: "text", nullable: true),
                    tax_name = table.Column<string>(type: "text", nullable: true),
                    tax_number = table.Column<string>(type: "text", nullable: true),
                    customer_privacy = table.Column<short>(type: "smallint", nullable: true),
                    latitude = table.Column<decimal>(type: "numeric", nullable: true),
                    longitude = table.Column<decimal>(type: "numeric", nullable: true),
                    place_name = table.Column<string>(type: "character varying(191)", maxLength: 191, nullable: true),
                    formatted_address = table.Column<string>(type: "character varying(191)", maxLength: 191, nullable: true),
                    forgot_password_code = table.Column<string>(type: "character varying(191)", maxLength: 191, nullable: true),
                    password = table.Column<string>(type: "text", nullable: true),
                    view_order_otp = table.Column<short>(type: "smallint", nullable: false),
                    assign_delivery_boy = table.Column<short>(type: "smallint", nullable: false),
                    fssai_lic_no = table.Column<string>(type: "character varying(191)", maxLength: 191, nullable: true),
                    self_pickup_mode = table.Column<bool>(type: "boolean", nullable: false),
                    is_pickup_mode_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    door_step_mode = table.Column<bool>(type: "boolean", nullable: false),
                    pickup_store_address = table.Column<string>(type: "text", nullable: true),
                    pickup_latitude = table.Column<decimal>(type: "numeric", nullable: true),
                    pickup_longitude = table.Column<decimal>(type: "numeric", nullable: true),
                    pickup_store_timings = table.Column<string>(type: "text", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    remark = table.Column<string>(type: "text", nullable: true),
                    RoleType = table.Column<int>(type: "integer", nullable: false),
                    change_order_status_delivered = table.Column<string>(type: "text", nullable: true),
                    is_email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    is_mobile_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    admin_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sellers", x => x.id);
                    table.ForeignKey(
                        name: "FK_sellers_admins_admin_id",
                        column: x => x.admin_id,
                        principalTable: "admins",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "nutrition_category",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    category_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nutrition_category", x => x.id);
                    table.ForeignKey(
                        name: "FK_nutrition_category_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "combo_categories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    category_id = table.Column<long>(type: "bigint", nullable: false),
                    combo_product_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_combo_categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_combo_categories_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_combo_categories_combo_products_combo_product_id",
                        column: x => x.combo_product_id,
                        principalTable: "combo_products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "category_specifications",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    category_id = table.Column<long>(type: "bigint", nullable: false),
                    specificationId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_category_specifications", x => x.id);
                    table.ForeignKey(
                        name: "FK_category_specifications_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_category_specifications_product_specification_filter_specif~",
                        column: x => x.specificationId,
                        principalTable: "product_specification_filter",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_specification_value",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    value = table.Column<string>(type: "text", nullable: false),
                    specification_filter_id = table.Column<long>(type: "bigint", nullable: false),
                    ProductSpecificationFilterId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_specification_value", x => x.id);
                    table.ForeignKey(
                        name: "FK_product_specification_value_product_specification_filter_Pr~",
                        column: x => x.ProductSpecificationFilterId,
                        principalTable: "product_specification_filter",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "carts",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    platform_type = table.Column<int>(type: "integer", nullable: false),
                    subtotal = table.Column<decimal>(type: "numeric", nullable: false),
                    tax_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    discount_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    grand_total = table.Column<decimal>(type: "numeric", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_carts", x => x.id);
                    table.ForeignKey(
                        name: "FK_carts_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "delivery-instructions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    audiopath = table.Column<string>(name: "audio-path", type: "text", nullable: true),
                    avoid_calling = table.Column<bool>(type: "boolean", nullable: false),
                    dont_ring_bell = table.Column<bool>(type: "boolean", nullable: false),
                    leave_with_guard = table.Column<bool>(type: "boolean", nullable: false),
                    leave_at_door = table.Column<bool>(type: "boolean", nullable: false),
                    beware_of_dogs = table.Column<bool>(type: "boolean", nullable: false),
                    additional_notes = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_delivery-instructions", x => x.id);
                    table.ForeignKey(
                        name: "FK_delivery-instructions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryRequest",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    pincode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    latitude = table.Column<double>(type: "double precision", maxLength: 191, nullable: false),
                    longitude = table.Column<double>(type: "double precision", maxLength: 191, nullable: false),
                    address = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    platform = table.Column<int>(type: "integer", nullable: false),
                    admin_remarks = table.Column<string>(type: "text", nullable: false),
                    is_resolved = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryRequest", x => x.id);
                    table.ForeignKey(
                        name: "FK_DeliveryRequest_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    razorpay_order_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    razorpay_payment_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    receipt = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    paid_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    due_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    refund_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    tip_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    failure_reason = table.Column<string>(type: "text", nullable: true),
                    expected_delivery_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    payment_status = table.Column<int>(type: "integer", nullable: false),
                    order_status = table.Column<int>(type: "integer", nullable: false),
                    payment_mode = table.Column<int>(type: "integer", nullable: false),
                    address_id = table.Column<long>(type: "bigint", nullable: true),
                    is_cutlary_allowed = table.Column<bool>(type: "boolean", nullable: false),
                    delivery_charge = table.Column<decimal>(type: "numeric", nullable: false),
                    platorm_charge = table.Column<decimal>(type: "numeric", nullable: false),
                    cutlary_charge = table.Column<decimal>(type: "numeric", nullable: false),
                    low_cart_fee_charge = table.Column<decimal>(type: "numeric", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orders", x => x.id);
                    table.ForeignKey(
                        name: "FK_orders_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_addresses",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    type = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(191)", maxLength: 191, nullable: false),
                    mobile = table.Column<string>(type: "character varying(191)", maxLength: 191, nullable: false),
                    alternate_mobile = table.Column<string>(type: "character varying(191)", maxLength: 191, nullable: true),
                    address = table.Column<string>(type: "text", nullable: false),
                    landmark = table.Column<string>(type: "text", nullable: false),
                    area = table.Column<string>(type: "character varying(191)", maxLength: 191, nullable: false),
                    pincode = table.Column<string>(type: "character varying(191)", maxLength: 191, nullable: false),
                    city = table.Column<string>(type: "character varying(191)", maxLength: 191, nullable: false),
                    state = table.Column<string>(type: "character varying(191)", maxLength: 191, nullable: false),
                    country = table.Column<string>(type: "character varying(191)", maxLength: 191, nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    latitude = table.Column<double>(type: "double precision", maxLength: 191, nullable: false),
                    longitude = table.Column<double>(type: "double precision", maxLength: 191, nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_addresses", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_addresses_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_promo_codes",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    promocode_id = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    usage_count = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_promo_codes", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_promo_codes_promo_codes_promocode_id",
                        column: x => x.promocode_id,
                        principalTable: "promo_codes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_promo_codes_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSupportRequest",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    mobile = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    admin_response = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_resolved = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSupportRequest", x => x.id);
                    table.ForeignKey(
                        name: "FK_UserSupportRequest_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "delibery_boy_pincodes",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    pincode = table.Column<string>(type: "text", nullable: false),
                    delivery_boy_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_delibery_boy_pincodes", x => x.id);
                    table.ForeignKey(
                        name: "FK_delibery_boy_pincodes_delivery_boys_delivery_boy_id",
                        column: x => x.delivery_boy_id,
                        principalTable: "delivery_boys",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "delivery_boy_order_transactions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    transaction_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    transaction_id = table.Column<string>(type: "text", nullable: true),
                    payment_type = table.Column<int>(type: "integer", nullable: false),
                    order_id = table.Column<long>(type: "bigint", nullable: true),
                    delivery_boy_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_delivery_boy_order_transactions", x => x.id);
                    table.ForeignKey(
                        name: "FK_delivery_boy_order_transactions_delivery_boys_delivery_boy_~",
                        column: x => x.delivery_boy_id,
                        principalTable: "delivery_boys",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "delivery_boy_transactions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    transaction_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    transaction_id = table.Column<string>(type: "text", nullable: true),
                    delivery_boy_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_delivery_boy_transactions", x => x.id);
                    table.ForeignKey(
                        name: "FK_delivery_boy_transactions_delivery_boys_delivery_boy_id",
                        column: x => x.delivery_boy_id,
                        principalTable: "delivery_boys",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "delivery_places",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    pincode = table.Column<string>(type: "character varying(191)", maxLength: 191, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    latitude = table.Column<double>(type: "double precision", maxLength: 191, nullable: true),
                    longitude = table.Column<double>(type: "double precision", maxLength: 191, nullable: true),
                    delivery_charges = table.Column<decimal>(type: "numeric", nullable: false),
                    sellerId = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_delivery_places", x => x.id);
                    table.ForeignKey(
                        name: "FK_delivery_places_sellers_sellerId",
                        column: x => x.sellerId,
                        principalTable: "sellers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(191)", maxLength: 191, nullable: false),
                    slug = table.Column<string>(type: "character varying(191)", maxLength: 191, nullable: false),
                    seller_id = table.Column<long>(type: "bigint", nullable: false),
                    category_id = table.Column<long>(type: "bigint", nullable: false),
                    brand_id = table.Column<long>(type: "bigint", nullable: false),
                    tax_percentage = table.Column<decimal>(type: "numeric", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_products", x => x.id);
                    table.ForeignKey(
                        name: "FK_products_brands_brand_id",
                        column: x => x.brand_id,
                        principalTable: "brands",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_products_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_products_sellers_seller_id",
                        column: x => x.seller_id,
                        principalTable: "sellers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "seller_settings",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    seller_id = table.Column<long>(type: "bigint", nullable: false),
                    jsondata = table.Column<string>(name: "json-data", type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_seller_settings", x => x.id);
                    table.ForeignKey(
                        name: "FK_seller_settings_sellers_seller_id",
                        column: x => x.seller_id,
                        principalTable: "sellers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "deliveries",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_id = table.Column<long>(type: "bigint", nullable: false),
                    delivery_boy_id = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    payment_mode = table.Column<int>(type: "integer", nullable: false),
                    start_lat = table.Column<double>(type: "double precision", nullable: true),
                    start_long = table.Column<double>(type: "double precision", nullable: true),
                    end_lat = table.Column<double>(type: "double precision", nullable: true),
                    end_long = table.Column<double>(type: "double precision", nullable: true),
                    expected_delivery_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    assigned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    delivered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_deliveries", x => x.id);
                    table.ForeignKey(
                        name: "FK_deliveries_delivery_boys_delivery_boy_id",
                        column: x => x.delivery_boy_id,
                        principalTable: "delivery_boys",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_deliveries_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "invoice",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    invoice_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    order_id = table.Column<long>(type: "bigint", nullable: false),
                    razprpay_invoice_id = table.Column<string>(type: "text", nullable: true),
                    currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    payment_status = table.Column<int>(type: "integer", nullable: false),
                    order_status = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoice", x => x.id);
                    table.ForeignKey(
                        name: "FK_invoice_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_toppings",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    topping_id = table.Column<long>(type: "bigint", nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_toppings", x => x.id);
                    table.ForeignKey(
                        name: "FK_product_toppings_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_product_toppings_toppings_topping_id",
                        column: x => x.topping_id,
                        principalTable: "toppings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_variants",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    indicator = table.Column<int>(type: "integer", nullable: false),
                    manufacturer = table.Column<string>(type: "character varying(191)", maxLength: 191, nullable: true),
                    made_in = table.Column<string>(type: "character varying(191)", maxLength: 191, nullable: true),
                    is_cancelable = table.Column<bool>(type: "boolean", nullable: false),
                    image = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    platform_type = table.Column<int>(type: "integer", nullable: false),
                    return_days = table.Column<int>(type: "integer", nullable: false),
                    is_unlimited_stock = table.Column<bool>(type: "boolean", nullable: false),
                    is_cod_allowed = table.Column<bool>(type: "boolean", nullable: false),
                    fssai_lic_no = table.Column<string>(type: "character varying(191)", maxLength: 191, nullable: true),
                    barcode = table.Column<string>(type: "text", nullable: true),
                    meta_title = table.Column<string>(type: "character varying(191)", maxLength: 191, nullable: true),
                    meta_keywords = table.Column<string>(type: "text", nullable: true),
                    schema_markup = table.Column<string>(type: "text", nullable: true),
                    meta_description = table.Column<string>(type: "text", nullable: true),
                    total_allowed_quantity = table.Column<int>(type: "integer", nullable: true),
                    is_tax_included_in_price = table.Column<bool>(type: "boolean", nullable: false),
                    return_policy = table.Column<int>(type: "integer", nullable: false),
                    measurement = table.Column<string>(type: "text", nullable: true),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    discounted_price = table.Column<decimal>(type: "numeric", nullable: true),
                    stock = table.Column<decimal>(type: "numeric", nullable: true),
                    view_count = table.Column<int>(type: "integer", nullable: true),
                    sku = table.Column<string>(type: "text", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    UnitDMId = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_variants", x => x.id);
                    table.ForeignKey(
                        name: "FK_product_variants_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_product_variants_units_UnitDMId",
                        column: x => x.UnitDMId,
                        principalTable: "units",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "delivery_status_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    delivery_id = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_delivery_status_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_delivery_status_history_deliveries_delivery_id",
                        column: x => x.delivery_id,
                        principalTable: "deliveries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "delivery_tracking",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    delivery_id = table.Column<long>(type: "bigint", nullable: false),
                    current_lat = table.Column<double>(type: "double precision", nullable: false),
                    current_long = table.Column<double>(type: "double precision", nullable: false),
                    address = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_delivery_tracking", x => x.id);
                    table.ForeignKey(
                        name: "FK_delivery_tracking_deliveries_delivery_id",
                        column: x => x.delivery_id,
                        principalTable: "deliveries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "addon_products",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    main_product_id = table.Column<long>(type: "bigint", nullable: false),
                    addon_product_id = table.Column<long>(type: "bigint", nullable: false),
                    category_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_addon_products", x => x.id);
                    table.ForeignKey(
                        name: "FK_addon_products_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_addon_products_product_variants_addon_product_id",
                        column: x => x.addon_product_id,
                        principalTable: "product_variants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_addon_products_product_variants_main_product_id",
                        column: x => x.main_product_id,
                        principalTable: "product_variants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cart_items",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cart_id = table.Column<long>(type: "bigint", nullable: false),
                    product_variant_id = table.Column<long>(type: "bigint", nullable: false),
                    qty = table.Column<int>(type: "integer", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric", nullable: false),
                    total_price = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cart_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_cart_items_carts_cart_id",
                        column: x => x.cart_id,
                        principalTable: "carts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_cart_items_product_variants_product_variant_id",
                        column: x => x.product_variant_id,
                        principalTable: "product_variants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_items",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_id = table.Column<long>(type: "bigint", nullable: false),
                    product_variant_id = table.Column<long>(type: "bigint", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    total_price = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_order_items_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_order_items_product_variants_product_variant_id",
                        column: x => x.product_variant_id,
                        principalTable: "product_variants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_banners",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    banner_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_banners", x => x.id);
                    table.ForeignKey(
                        name: "FK_product_banners_banners_banner_id",
                        column: x => x.banner_id,
                        principalTable: "banners",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_product_banners_product_variants_product_id",
                        column: x => x.product_id,
                        principalTable: "product_variants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_faqs",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    question = table.Column<string>(type: "text", nullable: false),
                    answer = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<bool>(type: "boolean", maxLength: 191, nullable: false),
                    product_variant_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_faqs", x => x.id);
                    table.ForeignKey(
                        name: "FK_product_faqs_product_variants_product_variant_id",
                        column: x => x.product_variant_id,
                        principalTable: "product_variants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_images",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    image = table.Column<string>(type: "character varying(191)", maxLength: 191, nullable: false),
                    product_variant_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_images", x => x.id);
                    table.ForeignKey(
                        name: "FK_product_images_product_variants_product_variant_id",
                        column: x => x.product_variant_id,
                        principalTable: "product_variants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_nutrition_data",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    product_variant_id = table.Column<long>(type: "bigint", nullable: false),
                    serve_size = table.Column<string>(type: "text", nullable: true),
                    calories = table.Column<decimal>(type: "numeric", nullable: true),
                    proteins = table.Column<decimal>(type: "numeric", nullable: true),
                    carbohydrates = table.Column<decimal>(type: "numeric", nullable: true),
                    fats = table.Column<decimal>(type: "numeric", nullable: true),
                    sugars = table.Column<decimal>(type: "numeric", nullable: true),
                    fiber = table.Column<decimal>(type: "numeric", nullable: true),
                    sodium = table.Column<decimal>(type: "numeric", nullable: true),
                    ingredients_json = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_nutrition_data", x => x.id);
                    table.ForeignKey(
                        name: "FK_product_nutrition_data_product_variants_product_variant_id",
                        column: x => x.product_variant_id,
                        principalTable: "product_variants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_ratings",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    rate = table.Column<short>(type: "smallint", nullable: false),
                    review = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    product_variant_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_ratings", x => x.id);
                    table.ForeignKey(
                        name: "FK_product_ratings_product_variants_product_variant_id",
                        column: x => x.product_variant_id,
                        principalTable: "product_variants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_product_ratings_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_specification_filter_value",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    product_filter_id = table.Column<long>(type: "bigint", nullable: false),
                    product_filter_value_id = table.Column<long>(type: "bigint", nullable: false),
                    product_variant_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_specification_filter_value", x => x.id);
                    table.ForeignKey(
                        name: "FK_product_specification_filter_value_product_specification_fi~",
                        column: x => x.product_filter_id,
                        principalTable: "product_specification_filter",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_product_specification_filter_value_product_specification_va~",
                        column: x => x.product_filter_value_id,
                        principalTable: "product_specification_value",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_product_specification_filter_value_product_variants_product~",
                        column: x => x.product_variant_id,
                        principalTable: "product_variants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_specifications",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    key = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: false),
                    product_variant_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_specifications", x => x.id);
                    table.ForeignKey(
                        name: "FK_product_specifications_product_variants_product_variant_id",
                        column: x => x.product_variant_id,
                        principalTable: "product_variants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_tag",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    product_variant_id = table.Column<long>(type: "bigint", nullable: false),
                    tag_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_tag", x => x.id);
                    table.ForeignKey(
                        name: "FK_product_tag_product_variants_product_variant_id",
                        column: x => x.product_variant_id,
                        principalTable: "product_variants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_product_tag_tags_tag_id",
                        column: x => x.tag_id,
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_units",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    product_variant_id = table.Column<long>(type: "bigint", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    unit_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_units", x => x.id);
                    table.ForeignKey(
                        name: "FK_product_units_product_variants_product_variant_id",
                        column: x => x.product_variant_id,
                        principalTable: "product_variants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_product_units_units_unit_id",
                        column: x => x.unit_id,
                        principalTable: "units",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rating_images",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    image = table.Column<string>(type: "character varying(191)", maxLength: 191, nullable: false),
                    product_rating_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rating_images", x => x.id);
                    table.ForeignKey(
                        name: "FK_rating_images_product_ratings_product_rating_id",
                        column: x => x.product_rating_id,
                        principalTable: "product_ratings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_addon_products_addon_product_id",
                table: "addon_products",
                column: "addon_product_id");

            migrationBuilder.CreateIndex(
                name: "IX_addon_products_category_id",
                table: "addon_products",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_addon_products_main_product_id",
                table: "addon_products",
                column: "main_product_id");

            migrationBuilder.CreateIndex(
                name: "IX_cart_items_cart_id",
                table: "cart_items",
                column: "cart_id");

            migrationBuilder.CreateIndex(
                name: "IX_cart_items_product_variant_id",
                table: "cart_items",
                column: "product_variant_id");

            migrationBuilder.CreateIndex(
                name: "IX_carts_user_id_platform_type",
                table: "carts",
                columns: new[] { "user_id", "platform_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_categories_parent_category_id",
                table: "categories",
                column: "parent_category_id");

            migrationBuilder.CreateIndex(
                name: "IX_category_specifications_category_id",
                table: "category_specifications",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_category_specifications_specificationId",
                table: "category_specifications",
                column: "specificationId");

            migrationBuilder.CreateIndex(
                name: "IX_combo_categories_category_id",
                table: "combo_categories",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_combo_categories_combo_product_id",
                table: "combo_categories",
                column: "combo_product_id");

            migrationBuilder.CreateIndex(
                name: "IX_delibery_boy_pincodes_delivery_boy_id",
                table: "delibery_boy_pincodes",
                column: "delivery_boy_id");

            migrationBuilder.CreateIndex(
                name: "IX_deliveries_delivery_boy_id",
                table: "deliveries",
                column: "delivery_boy_id");

            migrationBuilder.CreateIndex(
                name: "IX_deliveries_order_id",
                table: "deliveries",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_delivery_boy_order_transactions_delivery_boy_id",
                table: "delivery_boy_order_transactions",
                column: "delivery_boy_id");

            migrationBuilder.CreateIndex(
                name: "IX_delivery_boy_transactions_delivery_boy_id",
                table: "delivery_boy_transactions",
                column: "delivery_boy_id");

            migrationBuilder.CreateIndex(
                name: "IX_delivery_boys_admin_id",
                table: "delivery_boys",
                column: "admin_id");

            migrationBuilder.CreateIndex(
                name: "IX_delivery_boys_email",
                table: "delivery_boys",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_delivery_boys_username",
                table: "delivery_boys",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_delivery_places_sellerId",
                table: "delivery_places",
                column: "sellerId");

            migrationBuilder.CreateIndex(
                name: "IX_delivery_status_history_delivery_id",
                table: "delivery_status_history",
                column: "delivery_id");

            migrationBuilder.CreateIndex(
                name: "IX_delivery_tracking_delivery_id",
                table: "delivery_tracking",
                column: "delivery_id");

            migrationBuilder.CreateIndex(
                name: "IX_delivery-instructions_user_id",
                table: "delivery-instructions",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryRequest_user_id",
                table: "DeliveryRequest",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_invoice_order_id",
                table: "invoice",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_nutrition_category_category_id",
                table: "nutrition_category",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_items_order_id",
                table: "order_items",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_items_product_variant_id",
                table: "order_items",
                column: "product_variant_id");

            migrationBuilder.CreateIndex(
                name: "IX_orders_user_id",
                table: "orders",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_banners_banner_id",
                table: "product_banners",
                column: "banner_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_banners_product_id",
                table: "product_banners",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_faqs_product_variant_id",
                table: "product_faqs",
                column: "product_variant_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_images_product_variant_id",
                table: "product_images",
                column: "product_variant_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_nutrition_data_product_variant_id",
                table: "product_nutrition_data",
                column: "product_variant_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_ratings_product_variant_id",
                table: "product_ratings",
                column: "product_variant_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_ratings_user_id",
                table: "product_ratings",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_specification_filter_value_product_filter_id",
                table: "product_specification_filter_value",
                column: "product_filter_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_specification_filter_value_product_filter_value_id",
                table: "product_specification_filter_value",
                column: "product_filter_value_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_specification_filter_value_product_variant_id",
                table: "product_specification_filter_value",
                column: "product_variant_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_specification_value_ProductSpecificationFilterId",
                table: "product_specification_value",
                column: "ProductSpecificationFilterId");

            migrationBuilder.CreateIndex(
                name: "IX_product_specifications_product_variant_id",
                table: "product_specifications",
                column: "product_variant_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_tag_product_variant_id",
                table: "product_tag",
                column: "product_variant_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_tag_tag_id",
                table: "product_tag",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_toppings_product_id",
                table: "product_toppings",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_toppings_topping_id",
                table: "product_toppings",
                column: "topping_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_units_product_variant_id",
                table: "product_units",
                column: "product_variant_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_units_unit_id",
                table: "product_units",
                column: "unit_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_variants_product_id",
                table: "product_variants",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_variants_UnitDMId",
                table: "product_variants",
                column: "UnitDMId");

            migrationBuilder.CreateIndex(
                name: "IX_products_brand_id",
                table: "products",
                column: "brand_id");

            migrationBuilder.CreateIndex(
                name: "IX_products_category_id",
                table: "products",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_products_seller_id_slug",
                table: "products",
                columns: new[] { "seller_id", "slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rating_images_product_rating_id",
                table: "rating_images",
                column: "product_rating_id");

            migrationBuilder.CreateIndex(
                name: "IX_seller_settings_seller_id",
                table: "seller_settings",
                column: "seller_id");

            migrationBuilder.CreateIndex(
                name: "IX_sellers_admin_id",
                table: "sellers",
                column: "admin_id");

            migrationBuilder.CreateIndex(
                name: "IX_sellers_email",
                table: "sellers",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sellers_username",
                table: "sellers",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tags_name",
                table: "tags",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_addresses_user_id",
                table: "user_addresses",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_promo_codes_promocode_id",
                table: "user_promo_codes",
                column: "promocode_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_promo_codes_user_id",
                table: "user_promo_codes",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_UserSupportRequest_user_id",
                table: "UserSupportRequest",
                column: "user_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "addon_products");

            migrationBuilder.DropTable(
                name: "cart_items");

            migrationBuilder.DropTable(
                name: "category_specifications");

            migrationBuilder.DropTable(
                name: "combo_categories");

            migrationBuilder.DropTable(
                name: "delibery_boy_pincodes");

            migrationBuilder.DropTable(
                name: "delivery_boy_order_transactions");

            migrationBuilder.DropTable(
                name: "delivery_boy_transactions");

            migrationBuilder.DropTable(
                name: "delivery_places");

            migrationBuilder.DropTable(
                name: "delivery_status_history");

            migrationBuilder.DropTable(
                name: "delivery_tracking");

            migrationBuilder.DropTable(
                name: "delivery-instructions");

            migrationBuilder.DropTable(
                name: "DeliveryRequest");

            migrationBuilder.DropTable(
                name: "ErrorLogRoots");

            migrationBuilder.DropTable(
                name: "external_user");

            migrationBuilder.DropTable(
                name: "faqs");

            migrationBuilder.DropTable(
                name: "invoice");

            migrationBuilder.DropTable(
                name: "nutrition_category");

            migrationBuilder.DropTable(
                name: "offers_and_coupons");

            migrationBuilder.DropTable(
                name: "order_items");

            migrationBuilder.DropTable(
                name: "product_banners");

            migrationBuilder.DropTable(
                name: "product_faqs");

            migrationBuilder.DropTable(
                name: "product_images");

            migrationBuilder.DropTable(
                name: "product_nutrition_data");

            migrationBuilder.DropTable(
                name: "product_specification_filter_value");

            migrationBuilder.DropTable(
                name: "product_specifications");

            migrationBuilder.DropTable(
                name: "product_tag");

            migrationBuilder.DropTable(
                name: "product_toppings");

            migrationBuilder.DropTable(
                name: "product_units");

            migrationBuilder.DropTable(
                name: "promotional_content");

            migrationBuilder.DropTable(
                name: "rating_images");

            migrationBuilder.DropTable(
                name: "seller_settings");

            migrationBuilder.DropTable(
                name: "settings");

            migrationBuilder.DropTable(
                name: "user_addresses");

            migrationBuilder.DropTable(
                name: "user_promo_codes");

            migrationBuilder.DropTable(
                name: "UserSupportRequest");

            migrationBuilder.DropTable(
                name: "carts");

            migrationBuilder.DropTable(
                name: "combo_products");

            migrationBuilder.DropTable(
                name: "deliveries");

            migrationBuilder.DropTable(
                name: "banners");

            migrationBuilder.DropTable(
                name: "product_specification_value");

            migrationBuilder.DropTable(
                name: "tags");

            migrationBuilder.DropTable(
                name: "toppings");

            migrationBuilder.DropTable(
                name: "product_ratings");

            migrationBuilder.DropTable(
                name: "promo_codes");

            migrationBuilder.DropTable(
                name: "delivery_boys");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropTable(
                name: "product_specification_filter");

            migrationBuilder.DropTable(
                name: "product_variants");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "products");

            migrationBuilder.DropTable(
                name: "units");

            migrationBuilder.DropTable(
                name: "brands");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "sellers");

            migrationBuilder.DropTable(
                name: "admins");
        }
    }
}
