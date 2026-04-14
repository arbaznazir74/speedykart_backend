using Siffrum.Ecom.ServiceModels.Foundation.Base.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace Siffrum.Ecom.BAL.Foundation.Interface
{
    public class PasswordEncryptHelper : IPasswordEncryptHelper
    {
        private readonly byte[] _key;
        private readonly byte[] _fixedIV = Encoding.UTF8.GetBytes("0123456789ABCDEF"); // 16 bytes IV

        public PasswordEncryptHelper()
        {
            string keyString = "#$%wellandgoodsiffrum#%$#GSVDAhgde";
            _key = Encoding.UTF8.GetBytes(keyString);
            if (_key.Length != 32)
            {
                Array.Resize(ref _key, 32); // Ensure the key is exactly 32 bytes
            }
        }

        public async Task<string> ProtectAsync<T>(T data, string encryptionKey = "")
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var jsonData = System.Text.Json.JsonSerializer.Serialize(data);
            var encryptedData = EncryptString(jsonData, _key);
            return await Task.FromResult(encryptedData);
        }

        public async Task<T> UnprotectAsync<T>(string encryptedData, string decryptionKey = "")
        {
            if (string.IsNullOrEmpty(encryptedData))
            {
                throw new ArgumentNullException(nameof(encryptedData));
            }

            var jsonData = DecryptString(encryptedData, _key);
            var data = System.Text.Json.JsonSerializer.Deserialize<T>(jsonData);
            return await Task.FromResult(data);
        }

        private string EncryptString(string plainText, byte[] key)
        {
            using (var aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = _fixedIV; // Use fixed IV

                using (var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (var swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }

                    var encrypted = msEncrypt.ToArray();
                    return Convert.ToBase64String(encrypted);
                }
            }
        }

        /*private string EncryptString(string plainText, byte[] key)
        {
            using var aesAlg = Aes.Create();
            aesAlg.Key = key;
            aesAlg.IV = _fixedIV;
            aesAlg.Padding = PaddingMode.PKCS7;
            aesAlg.Mode = CipherMode.CBC;

            using var encryptor = aesAlg.CreateEncryptor();
            using var msEncrypt = new MemoryStream();
            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            {
                var plainBytes = Encoding.UTF8.GetBytes(plainText);
                csEncrypt.Write(plainBytes, 0, plainBytes.Length);
                csEncrypt.FlushFinalBlock(); // 🔥 REQUIRED
            }

            return Convert.ToBase64String(msEncrypt.ToArray());
        }*/

        private string DecryptString(string cipherText, byte[] key)
        {
            var fullCipher = Convert.FromBase64String(cipherText);

            using (var aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = _fixedIV; // Use fixed IV

                using (var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                using (var msDecrypt = new MemoryStream(fullCipher))
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (var srDecrypt = new StreamReader(csDecrypt))
                {
                    return srDecrypt.ReadToEnd();
                }
            }
        }

        /*private string DecryptString(string cipherText, byte[] key)
        {
            var fullCipher = Convert.FromBase64String(cipherText);

            using var aesAlg = Aes.Create();
            aesAlg.Key = key;
            aesAlg.IV = _fixedIV;
            aesAlg.Padding = PaddingMode.PKCS7;
            aesAlg.Mode = CipherMode.CBC;

            using var decryptor = aesAlg.CreateDecryptor();
            using var msDecrypt = new MemoryStream(fullCipher);
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt, Encoding.UTF8);

            return srDecrypt.ReadToEnd();
        }*/
    }
}
