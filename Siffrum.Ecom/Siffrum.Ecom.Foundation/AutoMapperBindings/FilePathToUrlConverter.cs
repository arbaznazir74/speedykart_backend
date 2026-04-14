using AutoMapper;
using Siffrum.Ecom.BAL.Foundation.CommonUtils;

namespace Siffrum.Ecom.Foundation.AutoMapperBindings
{
    public class FilePathToUrlConverter : IValueConverter<string, string>
    {
        public string Convert(string sourceMember, ResolutionContext context)
        {
            return sourceMember.ConvertFromFilePathToUrl();
        }
    }
}
