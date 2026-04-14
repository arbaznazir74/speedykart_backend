using CoreVisionServiceModels.v1.Examination;
using CoreVisionServiceModels.v1.General.License;
using System.Globalization;

namespace CoreVisionServiceModels.AppUser
{
    public class UserDashboardSM
    {
        public ClientUserSM UserDetails { get; set; }
        public List<ExamSM> Exams { get; set; }
        public UserTestLicenseDetailsSM LicenseDetails { get; set; }

        public string  TopExamPerformance { get; set; }
        public string TopSubjectPerformance { get; set; }
        public string TopTopicPerformance { get; set; }
        public double TopExamPercentage { get; set; }
        public double TopTopicPercentage { get; set; }
        public double TopSubjectPercentage { get; set; }
    }
}
