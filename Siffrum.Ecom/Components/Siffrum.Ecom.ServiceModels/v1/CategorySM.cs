using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class CategorySM : SiffrumServiceModelBase<long>
    {
        public string Name { get; set; } = null!;

        public string? Slug { get; set; }

        public string? Subtitle { get; set; }

        public string? ImageBase64 { get; set; }

        public PlatformTypeSM? Platform { get; set; }
        public CategoryTimingSM? Timings { get; set; }
        public StatusSM Status { get; set; }

        public long? ParentCategoryId { get; set; }

        public string? MetaTitle { get; set; }

        public string? MetaKeywords { get; set; }

        public string? SchemaMarkup { get; set; }

        public string? MetaDescription { get; set; }

        public string? WebImage { get; set; }

        public bool IsSystem { get; set; }
        public int Level { get; set; } = 1;
        public int SortOrder { get; set; }

        public int ProductsCount { get; set; }
        public long? SuggestedBySellerId { get; set; }
    }
}
