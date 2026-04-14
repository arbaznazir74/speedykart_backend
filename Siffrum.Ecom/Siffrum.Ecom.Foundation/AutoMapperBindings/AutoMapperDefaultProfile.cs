using AutoMapper;
using Siffrum.Ecom.DomainModels.Enums;
using Siffrum.Ecom.DomainModels.Foundation.Base;
using Siffrum.Ecom.DomainModels.v1;
using Siffrum.Ecom.ServiceModels.AppUser;
using Siffrum.Ecom.ServiceModels.AppUser.Login;
using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base;
using Siffrum.Ecom.ServiceModels.v1;
namespace Siffrum.Ecom.Foundation.AutoMapperBindings
{
    public class AutoMapperDefaultProfile : Profile
    {
        public AutoMapperDefaultProfile(IServiceProvider serviceProvider)
        {
            ApplicationSpecificMappings();


            //this.CreateMap<DummySubjectDM, DummySubjectSM>()
            //.ForMember(dst => dst.CreatedOnLTZ, opts => opts.MapFrom(src => DateExtensions.ConvertFromUTCToSystemTimezone(src.CreatedOnUTC)))
            //.ReverseMap();

            //this.CreateMap(typeof(DummySubjectDM), typeof(DummySubjectSM))
            //    .ForMember(nameof(DummySubjectSM.CreatedOnLTZ), opt =>
            //    {
            //        opt.MapFrom("CreatedOnUTC");
            //    });            

            // create auto mapping from DM to SM with same names
            var mapResp = this.RegisterAutoMapperFromDmToSm<SiffrumDomainModelBase<object>, SiffrumServiceModelBase<object>>();

            Console.WriteLine("AutoMappings DmToSm Success: " + mapResp.SuccessDmToSmMaps.Count);
            Console.WriteLine("AutoMappings SmToDm Success: " + mapResp.SuccessSmToDmMaps.Count);
            Console.WriteLine("AutoMappings Error: " + mapResp.UnsuccessfullPaths.Count);

            // serviceProviderUsage here
            //.ForMember(
            //    dest => dest.PropertyName,
            //    opt => opt.MapFrom(
            //        s => serviceProvider.GetService<ILanguage>().Language == "en-US"
            //            ? s.PropertyEnglishName
            //            : s.PropertyArabicName));
        }


        private void ApplicationSpecificMappings()
        {
            CreateMap<TokenUserSM, AdminDM>().ReverseMap();
            CreateMap<TokenUserSM, AdminSM>().ReverseMap();
            CreateMap<TokenUserSM, SellerDM>().ReverseMap();
            CreateMap<TokenUserSM, SellerSM>().ReverseMap();
            CreateMap<TokenUserSM, DeliveryBoyDM>().ReverseMap();
            CreateMap<TokenUserSM, DeliveryBoySM>().ReverseMap();
            CreateMap<TokenUserSM, UserDM>().ReverseMap();
            CreateMap<TokenUserSM, UserSM>().ReverseMap();
            CreateMap<SocialLoginSM, ExternalUserSM>().ReverseMap();
            CreateMap<ComboProductDM, ComboProductSM>()
                .ForMember(dest => dest.ProductIds, opt => opt.Ignore())
                .ForMember(dest => dest.JsonDetails, opt => opt.Ignore())
                .ForMember(dest => dest.ProductData, opt => opt.Ignore());
            CreateMap<ProductSpecificationFilterSM, ProductSpecificationFilterDM>().ReverseMap();
            CreateMap<ProductBannerDM, ProductBannerSM>().ReverseMap();
            CreateMap<ProductSpecificationValueDM, ProductSpecificationValueSM>().ReverseMap();
            CreateMap<ProductTagSM, ProductTagDM>().ReverseMap();
            CreateMap<OrderItemDM, OrderItemSM>()
                .ForMember(dest => dest.SelectedToppings, opt => opt.Ignore())
                .ForMember(dest => dest.SelectedAddons, opt => opt.Ignore());
            CreateMap<OrderItemSM, OrderItemDM>()
                .ForMember(dest => dest.SelectedToppings, opt => opt.Ignore())
                .ForMember(dest => dest.SelectedAddons, opt => opt.Ignore());
            CreateMap<DeliveryBoyPincodesDM, DeliveryBoyPincodesSM>().ReverseMap();
            CreateMap<PlatformTypeSM, PlatformTypeDM>().ReverseMap();
            CreateMap<ProductUnitDM, ProductUnitSM>();
            CreateMap<ProductUnitSM, ProductUnitDM>()
                .ForMember(dest => dest.Unit, opt => opt.Ignore())
                .ForMember(dest => dest.ProductVariants, opt => opt.Ignore());
            CreateMap<ToppingDM, ToppingSM>().ReverseMap();
            CreateMap<ProductToppingDM, ProductToppingSM>()
                .ForMember(dest => dest.ToppingName, opt => opt.Ignore());
            CreateMap<ProductToppingSM, ProductToppingDM>()
                .ForMember(dest => dest.Product, opt => opt.Ignore())
                .ForMember(dest => dest.Topping, opt => opt.Ignore());
            CreateMap<OrderComplaintDM, OrderComplaintSM>().ReverseMap();
            CreateMap<CashCollectionDM, CashCollectionSM>().ReverseMap();
            CreateMap<UserSupportReplyDM, UserSupportReplySM>().ReverseMap();

        }
    }
}
