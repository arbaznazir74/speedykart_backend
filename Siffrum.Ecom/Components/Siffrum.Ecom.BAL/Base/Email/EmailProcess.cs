using AutoMapper;
using Siffrum.Ecom.Config.Configuration;
using Siffrum.Ecom.DAL.Context;
using Siffrum.Ecom.ServiceModels.Foundation.Base.CommonResponseRoot;
using System.Net.Mail;
using System.Net;
using Siffrum.Ecom.BAL.Foundation.Base;

namespace Siffrum.Ecom.BAL.Base.Email
{
    public class EmailProcess : SiffrumBalBase
    {
        private readonly APIConfiguration _apiConfiguration;
        public EmailProcess(IMapper mapper, ApiDbContext context, APIConfiguration apiConfiguration)
            : base(mapper, context)
        {
            _apiConfiguration = apiConfiguration;
        }

        #region Send Email
        /// <summary>
        /// Sends an email with the specified details, including optional attachments. 
        /// The method provides the ability to handle errors by either throwing an exception 
        /// or returning null based on the value of <paramref name="throwOnError"/>.
        /// </summary>
        /// <param name="emailTo">The recipient's email address.</param>
        /// <param name="subject">The subject of the email.</param>
        /// <param name="message">The body of the email, which supports HTML content.</param>
        /// <param name="attachments">A list of attachments to include in the email. This parameter is optional and can be null.</param>
        /// <param name="throwOnError">
        /// A boolean value indicating how errors should be handled:
        /// <list type="bullet">
        ///   <item>
        ///     <description><c>true</c>: Throws an exception if an error occurs.</description>
        ///   </item>
        ///   <item>
        ///     <description><c>false</c>: Returns null if an error occurs.</description>
        ///   </item>
        /// </list>
        /// </param>
        /// <returns>
        /// A <see cref="BoolResponseRoot"/> object indicating the success of the email operation. 
        /// Returns null if an error occurs and <paramref name="throwOnError"/> is set to <c>false</c>.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown if an error occurs during the email sending process and <paramref name="throwOnError"/> is set to <c>true</c>.
        /// </exception>

        public BoolResponseRoot SendEmail(string emailTo, string subject, string message, List<Attachment> attachments = null, bool throwOnError = true)
        {
            var mailMessage = new MailMessage();
            var smtpClient = new SmtpClient();
            try
            {
                var userId = _apiConfiguration.SmtpMailSettings.UserId;
                var password = _apiConfiguration.SmtpMailSettings.Password;
                var port = _apiConfiguration.SmtpMailSettings.Port;
                var host = _apiConfiguration.SmtpMailSettings.Host;
                var enableSSL = _apiConfiguration.SmtpMailSettings.EnableSSL;
                mailMessage = new MailMessage(userId, emailTo)
                {
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true
                };
                smtpClient = new SmtpClient();

                if (attachments == null || attachments.Count < 1)
                {
                    attachments = null;
                }
                if (attachments != null)
                {
                    attachments.ForEach(x => mailMessage.Attachments.Add(x));
                }
                smtpClient.Host = host;
                smtpClient.EnableSsl = enableSSL;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(userId, password);
                smtpClient.Port = 587;
                smtpClient.Send(mailMessage);
                mailMessage.Attachments.Dispose();
                smtpClient.Dispose();

                return new BoolResponseRoot(true, "Success");
            }
            catch (Exception ex)
            {
                mailMessage.Attachments.Dispose();
                smtpClient.Dispose();

                if (throwOnError)
                {
                    throw;
                }

                return null;
            }
        }      


        #endregion Send Email
    }
}
