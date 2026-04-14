using CoreVisionServiceModels.Enums;

namespace CoreVisionServiceModels.v1.General.StoryAI
{
    public class ContentGenerationRequestSM
    {
        public List<FictionalCharacterSM> FictionalCharacters { get; set; }
        public ContentTypeSM ContentType { get; set; }
        public GenreTypeSM Genre { get; set; }
        public string Theme { get; set; }
        public AgeGroupSM AgeGroup { get; set; }
    }
}
