using Siffrum.Ecom.ServiceModels.Enums;

namespace Siffrum.Ecom.ServiceModels.Foundation.Base.Interfaces
{
    public interface ILoginUserDetail
    {
        public long DbRecordId { get; set; }
        public string LoginId { get; set; }
        public RoleTypeSM UserType { get; set; }
    }

    public class LoginUserDetail : ILoginUserDetail
    {
        public long DbRecordId { get; set; }
        public string LoginId { get; set; }
        public RoleTypeSM UserType { get; set; }
    }

    public class LoginUserDetailWithAdmin : ILoginUserDetail
    {
        public long DbRecordId { get; set; }
        public string LoginId { get; set; }
        public long AdminId { get; set; }
        //public string CompanyCode { get; set; }
        public RoleTypeSM UserType { get; set; }
    }
}
