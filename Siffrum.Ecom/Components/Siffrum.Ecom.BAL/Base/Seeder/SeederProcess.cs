using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Siffrum.Ecom.BAL.Foundation.Base;
using Siffrum.Ecom.DAL.Context;
using Siffrum.Ecom.DomainModels.Enums;
using Siffrum.Ecom.DomainModels.v1;
using Siffrum.Ecom.Config.Configuration;
using Siffrum.Ecom.ServiceModels.Foundation.Base.CommonResponseRoot;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Interfaces;

namespace Siffrum.Ecom.BAL.Base.Seeder
{
    public class SeederProcess : SiffrumBalBase
    {
        private readonly IPasswordEncryptHelper _passwordEncryptHelper;
        private readonly APIConfiguration _apiConfiguration;
        private readonly string defaultCreatedBy;
        private readonly string defaultUpdatedBy;

        public SeederProcess(IMapper mapper, ApiDbContext context, IPasswordEncryptHelper passwordEncryptHelper, APIConfiguration apiConfiguration)
            : base(mapper, context)
        {
            _passwordEncryptHelper = passwordEncryptHelper;
            _apiConfiguration = apiConfiguration;
            defaultCreatedBy = "SeedAdmin";
            defaultUpdatedBy = "UpdateAdmin";
        }
        public async Task<bool> SetupDatabaseWithTestData()
        {
            var adminsCount = _apiDbContext.Admin.Count();

            if (adminsCount == 0)
            {
                SeedData();

                return true;
            }
            return false;
        }
        public async Task<BoolResponseRoot> SeedData()
        {
            await SeedAdminUsers();
            return new BoolResponseRoot(true, "Seed check completed");
        }

        public async Task<BoolResponseRoot> SeedAllTestData()
        {
            await SeedAdminUsers();
            await SeedDeliverBoys();
            await SeedSellers();
            await SeedUsers();
            await SeedCategories();
            await SeedUnits();
            await SeedBrands();
            await SeedProducts();
            return new BoolResponseRoot(true, "Test data seeded successfully");
        }
        #region Admin/SystemAdmin

        private async Task SeedAdminUsers()
        {
            var saSettings = _apiConfiguration.SuperAdminSettings;
            var saUsername = saSettings?.Username ?? "Super1";
            var saEmail = saSettings?.Email ?? "superone@email.com";
            var saPassword = saSettings?.Password ?? "siffrumsuper1";
            var passwordHash = await _passwordEncryptHelper.ProtectAsync(saPassword);

            var existing = await _apiDbContext.Admin
                .FirstOrDefaultAsync(x => x.RoleType == RoleTypeDM.SuperAdmin && x.CreatedBy == defaultCreatedBy);

            if (existing == null)
            {
                var admin = new AdminDM()
                {
                    RoleType = RoleTypeDM.SuperAdmin,
                    Username = saUsername,
                    Email = saEmail,
                    Password = passwordHash,
                    FcmId = "dummyfcm2",
                    LoginStatus = LoginStatusDM.Enabled,
                    CreatedBy = defaultCreatedBy,
                    CreatedAt = DateTime.UtcNow
                };
                await _apiDbContext.Admin.AddAsync(admin);
            }
            else
            {
                existing.Username = saUsername;
                existing.Email = saEmail;
                existing.Password = passwordHash;
            }

            await _apiDbContext.SaveChangesAsync();
            


        }
        #endregion Admin/SystemAdmin

        #region Seller

        private async Task SeedSellers()
        {
            var count  = _apiDbContext.Seller.Count();
            if (count > 0) return;
            var passwordhash1 = await _passwordEncryptHelper.ProtectAsync("seller1");
            var passwordhash2 = await _passwordEncryptHelper.ProtectAsync("seller2");
            var admin1Id = await _apiDbContext.Admin.Where(x => x.Email == "sadminone@email.com").Select(x => x.Id).FirstOrDefaultAsync();
            var admin2Id = await _apiDbContext.Admin.Where(x => x.Email == "sadmintwo@email.com").Select(x => x.Id).FirstOrDefaultAsync();
            var seller1 = new SellerDM()
            {
                RoleType = RoleTypeDM.Seller,
                Email = "seller1@email.com",
                Username = "seller1",
                Mobile = "+911234567890",
                IsEmailConfirmed = true,
                IsMobileConfirmed = true,
                PincodeId = null,
                Status = SellerStatusDM.Active,
                LoginStatus = LoginStatusDM.Enabled,
                Password = passwordhash1,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = defaultCreatedBy
            };

            var seller2 = new SellerDM()
            {
                RoleType = RoleTypeDM.Seller,
                Username = "seller2",
                Email = "seller2@email.com",
                Mobile = "+911234567890",
                IsEmailConfirmed = true,
                IsMobileConfirmed = true,
                PincodeId = null,
                Status = SellerStatusDM.Active,
                LoginStatus = LoginStatusDM.Enabled,
                Password = passwordhash2,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = defaultCreatedBy
            };

            await _apiDbContext.Seller.AddRangeAsync(seller1, seller2);
            await _apiDbContext.SaveChangesAsync();

        }

        #endregion Seller

        #region Delivery Boys

        private async Task SeedDeliverBoys()
        {
            var count = _apiDbContext.DeliveryBoy.Count();
            if (count > 0) return;
            var passwordhash1 = await _passwordEncryptHelper.ProtectAsync("deliveryboy1");
            var passwordhash2 = await _passwordEncryptHelper.ProtectAsync("deliveryboy2");
            var admin1Id = await _apiDbContext.Admin.Where(x => x.Email == "sadminone@email.com").Select(x => x.Id).FirstOrDefaultAsync();
            var admin2Id = await _apiDbContext.Admin.Where(x => x.Email == "sadmintwo@email.com").Select(x => x.Id).FirstOrDefaultAsync();
            var deliverboy1 = new DeliveryBoyDM()
            {
                RoleType = RoleTypeDM.DeliveryBoy,
                AdminId = admin1Id,
                Name = "Delivery Boy 1",
                Username = "deliveryboy1",
                Email = "deliverboy1@email.com",
                IsMobileConfirmed = true,
                IsEmailConfirmed = true,
                Mobile = "+911234567891",
                Address = "dummyAddress",
                Status = DeliveryBoyStatusDM.Active,
                LoginStatus = LoginStatusDM.Enabled,
                Password = passwordhash1,
                CreatedBy = defaultCreatedBy,
                CreatedAt = DateTime.UtcNow
            };
            var deliverboy2 = new DeliveryBoyDM()
            {
                AdminId = admin2Id,
                RoleType = RoleTypeDM.DeliveryBoy,
                Name = "Delivery Boy 2",
                Username = "deliveryboy2",
                Email = "deliverboy2@email.com",
                IsMobileConfirmed = true,
                IsEmailConfirmed = true,
                Mobile = "+911234567892",
                Address = "dummyAddress",
                Status = DeliveryBoyStatusDM.Active,
                LoginStatus = LoginStatusDM.Enabled,
                Password = passwordhash2,
                CreatedBy = defaultCreatedBy,
                CreatedAt = DateTime.UtcNow
            };


            await _apiDbContext.DeliveryBoy.AddRangeAsync(deliverboy1, deliverboy2);
            await _apiDbContext.SaveChangesAsync();

        }
        #endregion Delivery Boys

        #region Users

        private async Task SeedUsers()
        {
            var count = _apiDbContext.User.Count();
            if (count > 0) return;
            var passwordhash1 = await _passwordEncryptHelper.ProtectAsync("user1");
            var passwordhash2 = await _passwordEncryptHelper.ProtectAsync("user2");
            var user1 = new UserDM()
            {
                RoleType = RoleTypeDM.User,
                Username = "user1",
                Email = "userone@email.com",
                Password = passwordhash1,
                Image = "wwwroot/content/loginusers/profile/user1.jpg",
                CountryCode = "+91",
                Mobile = "7006636038",
                PaymentId = "",
                IsEmailConfirmed = true,
                IsMobileConfirmed = true,
                LoginStatus = LoginStatusDM.Enabled,
                Status = StatusDM.Active,
                OTP = 123456,
                OfferJsonDetails = "{}",
                CreatedBy = defaultCreatedBy,
                CreatedAt = DateTime.UtcNow

            };            
            await _apiDbContext.User.AddRangeAsync(user1);
            await _apiDbContext.SaveChangesAsync();

        }

        #endregion Users

        #region Seed Categories

        private async Task SeedCategories()
        {
            var count = _apiDbContext.Category.Count();
            if (count > 0) return;
            var parentCategory = new CategoryDM()
            {
                Name = "Grocery",
                Slug = "grocery",
                Status = StatusDM.Active,
                Image = "wwwroot/content/categories/grocery.jpg",
                SortOrder = 1,
                Level= 1,
                Platform = PlatformTypeDM.None,
                ParentCategoryId = null,
                CreatedAt = DateTime.UtcNow

            };
            var parentCategory2 = new CategoryDM()
            {
                Name = "Electronics",
                Slug = "electronics",
                Image = "wwwroot/content/categories/electronics.jpg",
                Status = StatusDM.Active,
                SortOrder = 1,
                Platform = PlatformTypeDM.None,
                ParentCategoryId = null,
                Level = 1,
                CreatedAt = DateTime.UtcNow

            };
            
            await _apiDbContext.Category.AddRangeAsync(parentCategory, parentCategory2);
            await _apiDbContext.SaveChangesAsync();
            var pCategoryId = await _apiDbContext.Category.Where(x => x.Name == "Grocery").Select(x => x.Id).FirstOrDefaultAsync();
            var electronicCategoryId = await _apiDbContext.Category.Where(x => x.Name == "Electronics").Select(x => x.Id).FirstOrDefaultAsync();
            var subCategory = new CategoryDM()
            {
                Name = "Fruits",
                Slug = "fruits",
                Status = StatusDM.Active,
                Image = "wwwroot/content/categories/fruits.jpg",
                WebImage = "wwwroot/content/categories/fruits.jpg",
                SortOrder = 1,
                Level = 2,
                Platform = PlatformTypeDM.HotBox,
                ParentCategoryId = pCategoryId,
                CreatedAt = DateTime.UtcNow

            };
            
            var subCategory2 = new CategoryDM()
            {
                Name = "Cheese",
                Slug = "cheese",
                Status = StatusDM.Active,
                Image = "wwwroot/content/categories/cheese.jpg",
                WebImage = "wwwroot/content/categories/cheese2.jpg",
                SortOrder = 1,
                Level = 2,
                Platform = PlatformTypeDM.HotBox,
                ParentCategoryId = pCategoryId,
                CreatedAt = DateTime.UtcNow

            };

            var electronicSubCategory = new CategoryDM()
            {
                Name = "Mobiles",
                Slug = "mobiles",
                Status = StatusDM.Active,
                Image = "wwwroot/content/categories/mobile.jpg",
                WebImage = "wwwroot/content/categories/mobile.jpg",
                Platform = PlatformTypeDM.SpeedyMart,
                SortOrder = 1,
                Level = 2,
                ParentCategoryId = pCategoryId,
                CreatedAt = DateTime.UtcNow

            };

            var earbudsSubCategory = new CategoryDM()
            {
                Name = "Earbuds",
                Slug = "earbuds",
                Status = StatusDM.Active,
                Image = "wwwroot/content/categories/earbuds.jpg",
                WebImage = "wwwroot/content/categories/earbuds2.jpg",
                Platform = PlatformTypeDM.SpeedyMart,
                SortOrder = 1,
                Level = 2,
                ParentCategoryId = pCategoryId,
                CreatedAt = DateTime.UtcNow

            };
            await _apiDbContext.Category.AddRangeAsync(subCategory, electronicSubCategory, subCategory2, earbudsSubCategory);
            await _apiDbContext.SaveChangesAsync();

        }

        #endregion Seed Categories

        #region Seed Units

        private async Task SeedUnits()
        {
            var count = _apiDbContext.Unit.Count();
            if (count > 0) return;
            var parentUnit = new UnitDM()
            {
                Name = "Kilogram",
                ShortCode = "kg",
                ParentId = null,
                CreatedBy = defaultCreatedBy,
                CreatedAt = DateTime.UtcNow

            };
            

            await _apiDbContext.Unit.AddRangeAsync(parentUnit);
            await _apiDbContext.SaveChangesAsync();
            var pUnitId = await _apiDbContext.Unit.Where(x => x.Name == "Kilogram").Select(x => x.Id).FirstOrDefaultAsync();
            var subUnit = new UnitDM()
            {
                Name = "Milligram",
                ShortCode = "mg",
                ParentId = pUnitId,
                CreatedBy = defaultCreatedBy,
                CreatedAt = DateTime.UtcNow

            };
            await _apiDbContext.Unit.AddRangeAsync(subUnit);
            await _apiDbContext.SaveChangesAsync();

        }

        #endregion Seed Units

        #region Brands

        private async Task SeedBrands()
        {
            var count = _apiDbContext.Brand.Count();
            if (count > 0) return;
            var puma = new BrandDM()
            {
                Name = "Apple",
                Image = "wwwroot/content/brands/apple-brand.jpg",
                Status = StatusDM.Active,
                CreatedBy = defaultCreatedBy,
                CreatedAt = DateTime.UtcNow
            };
            var cheese = new BrandDM()
            {
                Name = "Cheese",
                Image = "wwwroot/content/brands/cheese-brand.jpg",
                Status = StatusDM.Active,
                CreatedBy = defaultCreatedBy,
                CreatedAt = DateTime.UtcNow
            };
            await _apiDbContext.Brand.AddRangeAsync(puma, cheese);
            await _apiDbContext.SaveChangesAsync();
        }

        #endregion Brands

        #region Product

        private async Task SeedProducts()
        {
            var count = _apiDbContext.Product.Count();
            if (count > 0) return;
            var sellerId = await _apiDbContext.Seller.Where(x => x.Username == "seller1").Select(x => x.Id).FirstOrDefaultAsync();
            var cheeseCategory = await _apiDbContext.Category.Where(x => x.Name == "Cheese").Select(x => x.Id).FirstOrDefaultAsync();
            var earbudCategory = await _apiDbContext.Category.Where(x => x.Name == "Earbuds").Select(x => x.Id).FirstOrDefaultAsync();
            var appleBrand = await _apiDbContext.Brand.Where(x => x.Name == "Apple").Select(x => x.Id).FirstOrDefaultAsync();
            var cheeseBrand = await _apiDbContext.Brand.Where(x => x.Name == "Cheese").Select(x => x.Id).FirstOrDefaultAsync();
            var product1 = new ProductDM()
            {
                Name = "Cheese",
                Slug = "cheese",
                BrandId = cheeseBrand,
                CategoryId = cheeseCategory,
                SellerId = sellerId,
                ProductVariants = new List<ProductVariantDM>()
                {
                    new ProductVariantDM()
                    {
                        Name = "Yummy Cheese Rings",
                        Price = 99,
                        Description = "",
                        Indicator = ProductIndicatorDM.NonVeg,
                        PlatformType = PlatformTypeDM.HotBox,
                        Status = ProductStatusDM.Active,
                        Image = "wwwroot/content/products/cheese-rings.jpg",
                        IsCancelable = true,
                        CreatedBy = defaultCreatedBy,
                        CreatedAt = DateTime.UtcNow
                    },
                    new ProductVariantDM()
                    {
                        Name = "Yummy Cheese Nuggets",
                        Description = "",
                        Price = 199,
                        Indicator = ProductIndicatorDM.NonVeg,
                        Status = ProductStatusDM.Active,
                        PlatformType = PlatformTypeDM.HotBox,
                        Image = "wwwroot/content/products/cheese-nuggets.jpg",
                        IsCancelable = true,
                        CreatedBy = defaultCreatedBy,
                        CreatedAt = DateTime.UtcNow
                    },
                },
                CreatedBy = defaultCreatedBy,
                CreatedAt = DateTime.UtcNow
            };
            var product2 = new ProductDM()
            {
                Name = "Earbud",
                Slug = "earbud",
                BrandId = appleBrand,
                CategoryId = earbudCategory,
                SellerId = sellerId,
                ProductVariants = new List<ProductVariantDM>()
                {
                    new ProductVariantDM()
                    {
                        Name = "Apple earbud",
                        Price = 599,
                        Description = "",
                        Indicator = ProductIndicatorDM.None,
                        Status = ProductStatusDM.Active,
                        Image = "wwwroot/content/products/earbud.jpg",
                        PlatformType = PlatformTypeDM.SpeedyMart,
                        IsCancelable = true,
                        CreatedBy = defaultCreatedBy,
                        CreatedAt = DateTime.UtcNow
                    }
                },
                CreatedBy = defaultCreatedBy,
                CreatedAt = DateTime.UtcNow
            };
            await _apiDbContext.Product.AddRangeAsync(product1, product2);
            await _apiDbContext.SaveChangesAsync();
        }

        #endregion Product
    }
}
