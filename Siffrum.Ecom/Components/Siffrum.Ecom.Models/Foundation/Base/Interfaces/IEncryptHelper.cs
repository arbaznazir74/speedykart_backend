namespace CoreVisionServiceModels.Foundation.Base.Interfaces
{
    public interface IEncryptHelper
    {
        Task<string> ProtectAsync<T>(T data, string encryptionKey = "");

        Task<T> UnprotectAsync<T>(string encryptedData, string decryptionKey = "");
    }
}
