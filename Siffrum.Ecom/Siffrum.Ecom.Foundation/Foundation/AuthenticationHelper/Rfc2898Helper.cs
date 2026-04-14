using System.Security.Cryptography;
using System.Text;

namespace Siffrum.Ecom.Foundation.Foundation.AuthenticationHelper
{
    public class Rfc2898Helper : EncryptHelperBase
    {
        public Rfc2898Helper(string encryptionKey = "", string decryptionKey = "", Func<string, byte[]> convertToBytes = null, Func<byte[], string> convertFromBytes = null)
            : base(encryptionKey, decryptionKey, convertToBytes, convertFromBytes)
        {
            if (convertToBytes == null)
            {
                _convertToBytes = Encoding.Unicode.GetBytes;
            }

            if (convertFromBytes == null)
            {
                _convertFromBytes = Encoding.Unicode.GetString;
            }
        }

        protected override async Task<string> CipherData(string keyToUse, string inputData)
        {
            _ = string.Empty;
            byte[] array = _convertToBytes(inputData);
            using Aes aes = Aes.Create();
            Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(keyToUse, new byte[13]
            {
            73, 118, 97, 110, 32, 77, 101, 100, 118, 101,
            100, 101, 118
            });
            aes.Key = rfc2898DeriveBytes.GetBytes(32);
            aes.IV = rfc2898DeriveBytes.GetBytes(16);
            using MemoryStream memoryStream = new MemoryStream();
            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cryptoStream.Write(array, 0, array.Length);
                cryptoStream.Close();
            }

            return Convert.ToBase64String(memoryStream.ToArray());
        }

        protected override async Task<string> UnCipherData(string keyToUse, string encodedString)
        {
            byte[] array = Convert.FromBase64String(encodedString);
            using Aes aes = Aes.Create();
            Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(keyToUse, new byte[13]
            {
            73, 118, 97, 110, 32, 77, 101, 100, 118, 101,
            100, 101, 118
            });
            aes.Key = rfc2898DeriveBytes.GetBytes(32);
            aes.IV = rfc2898DeriveBytes.GetBytes(16);
            using MemoryStream memoryStream = new MemoryStream();
            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Write))
            {
                cryptoStream.Write(array, 0, array.Length);
                cryptoStream.Close();
            }

            return _convertFromBytes(memoryStream.ToArray());
        }
    }
}
