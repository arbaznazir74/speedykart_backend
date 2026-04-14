using Microsoft.EntityFrameworkCore;
using Siffrum.Ecom.DAL.Base;
using Siffrum.Ecom.DomainModels.AppUser;
using Siffrum.Ecom.DomainModels.Foundation;
using Siffrum.Ecom.DomainModels.v1;

namespace Siffrum.Ecom.DAL.Context
{
    public class ApiDbContext : EfCoreContextRoot
    {
        #region Constructor
        public ApiDbContext(DbContextOptions<ApiDbContext> options)
            : base(options)
        {
            Database.Migrate();
        }
        #endregion Constructor

        #region Log Tables
        public DbSet<ErrorLogRoot> ErrorLogRoots { get; set; }

        #endregion Log Tables

        #region App Users
        
        public DbSet<ExternalUserDM> ExternalUsers { get; set; }

        #endregion App Users

        #region Ecom Tables
        public DbSet<AdminDM> Admin { get; set; }        
        public DbSet<BrandDM> Brand { get; set; }
        public DbSet<CartDM> Carts { get; set; }
        public DbSet<CartItemDM> CartItems { get; set; }
        public DbSet<CategoryDM> Category { get; set; }
        public DbSet<CategorySellerDM> CategorySellers { get; set; }
        public DbSet<DeliveryBoyDM> DeliveryBoy { get; set; }
        public DbSet<DeliveryDM> Deliveries { get; set; }
        public DbSet<DeliveryTrackingDM> DeliveryTracking { get; set; }
        public DbSet<DeliveryStatusHistoryDM> DeliveryStatusHistory { get; set; }
        public DbSet<FaqDM> Faq { get; set; }
        public DbSet<ProductFaqDM> ProductFaq { get; set; }
        public DbSet<InvoiceDM> Invoice { get; set; }
        public DbSet<OrderDM> Order { get; set; }
        public DbSet<OrderItemDM> OrderItem { get; set; }
        public DbSet<DeliveryPlacesDM> DeliveryPlaces { get; set; }
        public DbSet<ProductDM> Product { get; set; }
        public DbSet<ProductImagesDM> ProductImages { get; set; }
        public DbSet<ProductRatingDM> ProductRating { get; set; }
        public DbSet<ProductTagDM> ProductTag { get; set; }
        public DbSet<ProductVariantDM> ProductVariant { get; set; }
        public DbSet<PromoCodeDM> PromoCodes { get; set; }
        public DbSet<RatingImagesDM> RatingImages { get; set; }
        public DbSet<SellerDM> Seller { get; set; }
        public DbSet<BannerDM> Banners { get; set; }
        public DbSet<ProductBannerDM> ProductBanners {get; set;}
        public DbSet<TagDM> Tag { get; set; }
        public DbSet<UnitDM> Unit { get; set; }
        public DbSet<UserDM> User { get; set; }
        public DbSet<UserPromocodesDM> UserPromocodes { get; set; }
        public DbSet<DeliveryInstructionsDM> DeliveryInstructions { get; set; }
        public DbSet<ProductUnitDM> ProductUnit { get; set; }
        public DbSet<UserAddressDM> UserAddress { get; set; }
        public DbSet<OffersAndCouponsDM> OffersAndCoupons { get; set; }
        public DbSet<ProductSpecificationDM> ProductSpecifications { get; set; }
        public DbSet<ProductSpecificationFilterDM> ProductSpecificationFilters { get; set; }
        public DbSet<ProductSpecificationValueDM> ProductSpecificationValues { get; set; }
        public DbSet<ProductSpecificationFilterValueDM> ProductSpecificationFilterValues { get; set; }
        public DbSet<CategorySpecificationDM> CategorySpecifications { get; set; }
        public DbSet<ProductNutritionDataDM> ProductNutritionData { get; set; }
        public DbSet<NutritionCategoryDM> NutritionCategory { get; set; }
        public DbSet<ComboProductDM> ProductCombos { get; set; }
        public DbSet<ComboCategoryDM> ComboCategory { get; set; }
        public DbSet<AddOnProductsDM> AddonProducts { get; set; }
        public DbSet<PromotionalContentDM> PromotionalContents { get; set; }
        public DbSet<DeliveryBoyPincodesDM> DeliveryBoyPincodes { get; set; }
        public DbSet<DeliveryRequestDM> DeliveryRequest { get; set; }
        public DbSet<UserSupportRequestDM> UserSupportRequest { get; set; }
        public DbSet<SettingsDM> Settings { get; set; }
        public DbSet<SellerSettingsDM> SellerSettings { get; set; }
        public DbSet<ToppingDM> Topping { get; set; }
        public DbSet<ProductToppingDM> ProductTopping { get; set; }
        public DbSet<DeliveryBoyTransactionsDM> DeliveryBoyTransactions { get; set; }
        public DbSet<DeliveryBoyOrderTransactionsDM> DeliveryBoyOrderTransactions { get; set; }
        public DbSet<OrderComplaintDM> OrderComplaint { get; set; }
        public DbSet<CashCollectionDM> CashCollection { get; set; }
        public DbSet<StoreHoursDM> StoreHours { get; set; }
        public DbSet<ProductTimingDM> ProductTimings { get; set; }

        #endregion Ecom Tables

        #region On Model Creating
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AddOnProductsDM>()
                .HasOne(x => x.MainProduct)
                .WithMany(p => p.MainProductAddons)
                .HasForeignKey(x => x.MainProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AddOnProductsDM>()
                .HasOne(x => x.AddOnProduct)
                .WithMany(p => p.AddonProducts)
                .HasForeignKey(x => x.AddonProductId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CartDM>()
                .HasIndex(x => new { x.UserId, x.PlatformType })
                .IsUnique();
            modelBuilder.Entity<CategorySellerDM>()
                .HasIndex(x => new { x.CategoryId, x.SellerId })
                .IsUnique();
            modelBuilder.Entity<StoreHoursDM>()
                .HasIndex(x => new { x.SellerId, x.DayOfWeek })
                .IsUnique();
            DatabaseSeeder<ApiDbContext> seeder = new DatabaseSeeder<ApiDbContext>();
            seeder.SetupDatabaseWithSeedData(modelBuilder);
        }

        #endregion On Model Creating

    }
}
