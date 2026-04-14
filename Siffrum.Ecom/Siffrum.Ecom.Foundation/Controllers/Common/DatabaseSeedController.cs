using Microsoft.AspNetCore.Mvc;
using Siffrum.Ecom.BAL.Base.Seeder;
using Siffrum.Ecom.DAL.Context;
using Siffrum.Ecom.Foundation.Controllers.Base;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Interfaces;

namespace Siffrum.Ecom.Foundation.Controllers.Common
{
    [ApiController]
    [Route("[controller]")]
    public partial class DatabaseSeedController : ApiControllerRoot
    {
        private readonly ApiDbContext _apiDbContext;
        private readonly SeederProcess _seederProcess;
        private readonly IPasswordEncryptHelper _passwordEncryptHelper;

        public DatabaseSeedController(ApiDbContext context, IPasswordEncryptHelper passwordEncryptHelper, SeederProcess seederProcess)
        {
            _apiDbContext = context;
            _passwordEncryptHelper = passwordEncryptHelper;
            _seederProcess = seederProcess;
        }

        [HttpGet]
        [Route("status")]
        public IActionResult GetStatus()
        {
            var adminsCount = _apiDbContext.Admin.Count();
            return Ok(new { initialized = adminsCount > 0, adminsCount });
        }

    }
}
