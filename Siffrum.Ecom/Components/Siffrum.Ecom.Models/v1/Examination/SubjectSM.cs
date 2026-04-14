using CoreVisionServiceModels.Foundation.Base;

namespace CoreVisionServiceModels.v1.Examination
{
    public class SubjectSM : CoreVisionServiceModelBase<int>
    {
        public string SubjectName { get; set; }

        public string SubjectDescription { get; set; }
    }
}
