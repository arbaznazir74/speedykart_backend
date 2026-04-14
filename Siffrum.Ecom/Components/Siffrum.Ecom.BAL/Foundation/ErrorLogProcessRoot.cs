using Siffrum.Ecom.DomainModels.Foundation;
using Siffrum.Ecom.DAL.Foundation;
using Siffrum.Ecom.BAL.Foundation.Config;

namespace Siffrum.Ecom.BAL.Foundation
{
    public class ErrorLogProcessRoot
    {
        private readonly ErrorLogDALRoot _errorLogDALRoot;

        private readonly ApplicationIdentificationRoot _applicationIdentificationRoot;

        public ErrorLogProcessRoot(string connectionStr, ApplicationIdentificationRoot applicationIdentificationRoot, ErrorLogDALRoot errorLogDALRoot = null)
        {
            if (errorLogDALRoot == null)
            {
                _errorLogDALRoot = new ErrorLogDALRoot(connectionStr);
            }
            else
            {
                _errorLogDALRoot = errorLogDALRoot;
            }

            _applicationIdentificationRoot = applicationIdentificationRoot;
        }

        public async Task<bool> SaveErrorObjectInDb(ErrorLogRoot errorLog)
        {
            return await _errorLogDALRoot.SaveErrorObjectInDb(errorLog);
        }
    }
}
