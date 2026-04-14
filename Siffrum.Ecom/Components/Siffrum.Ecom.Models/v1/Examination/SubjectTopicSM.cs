using CoreVisionServiceModels.Foundation.Base;

namespace CoreVisionServiceModels.v1.Examination
{
    public class SubjectTopicSM : CoreVisionServiceModelBase<int>
    {
        public string TopicName { get; set; }
        public string TopicDescription { get; set; }
        public int SubjectId { get; set; }
    }
}
