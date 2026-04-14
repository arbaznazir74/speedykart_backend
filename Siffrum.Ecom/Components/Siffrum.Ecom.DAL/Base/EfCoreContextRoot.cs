using Microsoft.EntityFrameworkCore;

namespace Siffrum.Ecom.DAL.Base
{
    public abstract class EfCoreContextRoot : DbContext, IEfCoreContextRoot
    {
        public EfCoreContextRoot(DbContextOptions options)
            : base(options)
        {
        }
    }
}
