using Siffrum.Ecom.ServiceModels.Foundation.Base.Interfaces;
using Newtonsoft.Json;
using System.Text;

namespace Siffrum.Ecom.Foundation.Foundation.AuthenticationHelper
{
    public abstract class EncryptHelperBase : IEncryptHelper
    {
        protected string _defaultEncryptionKey = "";

        protected string _defaultDecryptionKey = "";

        protected Func<string, byte[]> _convertToBytes;

        protected Func<byte[], string> _convertFromBytes;

        public EncryptHelperBase(string encryptionKey = "", string decryptionKey = "", Func<string, byte[]> convertToBytes = null, Func<byte[], string> convertFromBytes = null)
        {
            _defaultEncryptionKey = encryptionKey;
            _defaultDecryptionKey = decryptionKey;
            if (convertToBytes == null)
            {
                _convertToBytes = Encoding.UTF8.GetBytes;
            }

            if (convertFromBytes == null)
            {
                _convertFromBytes = Encoding.UTF8.GetString;
            }
        }

        public virtual async Task<string> ProtectAsync<T>(T data, string encryptionKey = "")
        {
            if (string.IsNullOrWhiteSpace(encryptionKey))
            {
                encryptionKey = _defaultEncryptionKey;
            }

            string inputData = !(typeof(T) == typeof(string)) ? JsonConvert.SerializeObject(data) : data.ToString();
            return await CipherData(encryptionKey, inputData);
        }

        public virtual async Task<T> UnprotectAsync<T>(string encryptedData, string decryptionKey = "")
        {
            if (string.IsNullOrWhiteSpace(decryptionKey))
            {
                decryptionKey = _defaultDecryptionKey;
            }

            string text = await UnCipherData(decryptionKey, encryptedData);
            if (text == null)
            {
                return default;
            }

            if (typeof(T) == typeof(string))
            {
                return (T)(object)text;
            }

            return JsonConvert.DeserializeObject<T>(text);
        }

        protected abstract Task<string> CipherData(string keyToUse, string inputData);

        protected abstract Task<string> UnCipherData(string keyToUse, string encodedString);
    }
}
