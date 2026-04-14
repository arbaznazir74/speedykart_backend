using CoreVisionServiceModels.Enums;

namespace CoreVisionServiceModels.v1.General.StoryAI
{
    public class FictionalCharacterSM
    {
        public string Name { get; set; }
        public string? Role { get; set; }
        public string? ImageBase64 { get; set; }
    }
}