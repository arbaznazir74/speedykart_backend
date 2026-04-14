using Siffrum.Ecom.ServiceModels.Foundation.Base.Interfaces;
using Siffrum.Ecom.Foundation.Foundation.AuthenticationHelper;

namespace Siffrum.Ecom.Foundation.Foundation.Web.Security
{
    public class PasswordEncryptHelper : Rfc2898Helper, IPasswordEncryptHelper, IEncryptHelper
    {
        public PasswordEncryptHelper(string encryptionKey, string decryptionKey)
            : base(encryptionKey, decryptionKey)
        {
        }
    }
}
