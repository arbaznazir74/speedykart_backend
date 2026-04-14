namespace Siffrum.Ecom.BAL.Foundation.CommonUtils
{
    public static class WebExtensions
    {
        public static string ConvertFromFilePathToUrl(this string filePath)
        {
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                return filePath.Replace("\\", "/");
            }

            return "";
        }
    }
}
