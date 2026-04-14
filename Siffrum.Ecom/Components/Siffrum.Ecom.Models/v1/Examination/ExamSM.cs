using CoreVisionServiceModels.Foundation.Base;

namespace CoreVisionServiceModels.v1.Examination
{
    public class ExamSM : CoreVisionServiceModelBase<int>
    {
        public string ExamName { get; set; }

        public string ExamDescription { get; set; }
        public string ConductedBy { get; set; } 

        public bool IsActive { get; set; }
    }
}
