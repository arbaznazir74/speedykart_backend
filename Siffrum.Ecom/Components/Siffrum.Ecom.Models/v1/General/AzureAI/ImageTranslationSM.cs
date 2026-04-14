using CoreVisionServiceModels.Enums;

namespace CoreVisionServiceModels.v1.General.AzureAI
{
    public class ImageTranslationSM
    {
        public string Base64Image { get; set; }
        public TextTranslationLanguageSupportSM Language { get; set; }
    }
}
