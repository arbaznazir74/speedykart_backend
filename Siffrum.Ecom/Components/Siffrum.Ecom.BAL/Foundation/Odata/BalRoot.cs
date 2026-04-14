using AutoMapper;
using System.Linq.Expressions;
using AutoMapper.QueryableExtensions;
namespace Siffrum.Ecom.BAL.Foundation.Odata
{
    public abstract class BalRoot
    {
        protected async Task<IQueryable<SM>> MapEntityAsToQuerable<DM, SM>(IMapper mapperToUse, IQueryable<DM> srcEntitySet)
        {
            return srcEntitySet.ProjectTo(mapperToUse.ConfigurationProvider, Array.Empty<Expression<Func<SM, object>>>());
        }

        protected async Task<bool> SavePostedFileAtPath(IFormFile postedFile, string targetPath)
        {
            if (!Directory.Exists(Path.GetDirectoryName(targetPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
            }

            if (postedFile == null)
            {
                return false;
            }

            using (FileStream filestream = File.Create(targetPath))
            {
                await postedFile.CopyToAsync(filestream);
                filestream.Flush();
            }

            return true;
        }

        protected async Task<byte[]> GetPostedFileAsMemoryStream(IFormFile? postedFile)
        {
            if (postedFile == null)
            {
                return null;
            }

            using MemoryStream memStream = new MemoryStream();
            await postedFile.CopyToAsync(memStream);
            return memStream.ToArray();
        }
    }
}
