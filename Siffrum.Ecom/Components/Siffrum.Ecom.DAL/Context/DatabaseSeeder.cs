using Microsoft.EntityFrameworkCore;
using Siffrum.Ecom.DAL.Base;

namespace Siffrum.Ecom.DAL.Context
{
    public class DatabaseSeeder<T> where T : EfCoreContextRoot
    {
        #region Setup Database Seed Data
        public void SetupDatabaseWithSeedData(ModelBuilder modelBuilder)
        {
            var defaultCreatedBy = "SeedAdmin";
            //SeedDummyCompanyData(modelBuilder, defaultCreatedBy);
        }

        #endregion Setup Database Seed Data

        #region Setup Database With Test Data
        public async Task<bool> SetupDatabaseWithTestData(T context, Func<string, string> encryptorFunc)
        {
            var defaultCreatedBy = "SeedAdmin";
            var defaultUpdatedBy = "UpdateAdmin";
            var apiDb = context as ApiDbContext;
            if (apiDb != null && apiDb.Admin.Count() == 0)
            {               

                return true;
            }
            return false;
        }

        #endregion Setup Database With Test Data


    }
}
