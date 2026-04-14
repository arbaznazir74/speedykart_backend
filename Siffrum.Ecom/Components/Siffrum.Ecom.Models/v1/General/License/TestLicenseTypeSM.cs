using CoreVisionServiceModels.Foundation.Base;

namespace CoreVisionServiceModels.v1.General.License
{
    public class TestLicenseTypeSM : CoreVisionServiceModelBase<int>
    {
        public string Title { get; set; }
        public string? Description { get; set; }

        public int TestCountValidity { get; set; }

        public string LicenseTypeCode { get; set; }

        public double Amount { get; set; }
    }
}
