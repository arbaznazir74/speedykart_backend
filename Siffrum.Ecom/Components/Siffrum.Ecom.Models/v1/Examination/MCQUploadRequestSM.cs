using CoreVisionServiceModels.Foundation.Base;

namespace CoreVisionServiceModels.v1.Examination
{
    public class MCQUploadRequestSM
    {
        public string QuestionText { get; set; }
        public string OptionA { get; set; }
        public string OptionB { get; set; }
        public string OptionC { get; set; }
        public string OptionD { get; set; }
        public string CorrectOption { get; set; } 
        public string CorrectOptionByUser { get; set; } 
        public string? Explanation { get; set; }
        public int Id { get; set; }
    }
}
