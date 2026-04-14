using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Siffrum.Ecom.DomainModels.Foundation
{
    public class ErrorLogRoot
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public string? LoginUserId { get; set; }

        public string? UserRoleType { get; set; }

       // public string? CompanyCode { get; set; }

        public string? CreatedByApp { get; set; }

        public DateTime CreatedOnUTC { get; set; }

        public string LogMessage { get; set; }

        public string? LogStackTrace { get; set; }

        public string? LogExceptionData { get; set; }

        public string? InnerException { get; set; }

        public string? TracingId { get; set; }

        public string Caller { get; set; }

        public string? RequestObject { get; set; }

        public string? ResponseObject { get; set; }

        public string? AdditionalInfo { get; set; }

        public ErrorLogRoot()
        {
            CreatedOnUTC = DateTime.UtcNow;
        }
    }
}
