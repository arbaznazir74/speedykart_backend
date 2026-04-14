using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Siffrum.Ecom.BAL.ExceptionHandler;
using Siffrum.Ecom.BAL.Foundation.Web;
using Siffrum.Ecom.BAL.Marketing;
using Siffrum.Ecom.BAL.Product;
using Siffrum.Ecom.Config.Configuration;
using Siffrum.Ecom.Foundation.Controllers.Base;
using Siffrum.Ecom.Foundation.Security;
using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base.CommonResponseRoot;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Enums;
using Siffrum.Ecom.ServiceModels.v1;
using System.Text;

namespace Siffrum.Ecom.Foundation.Controllers.Product.Marketing
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class FaqController : ApiControllerWithOdataRoot<FaqSM>
    {
        private readonly FaqProcess _process;

        public FaqController(FaqProcess process)
            : base(process)
        {
            _process = process;
        }

        #region ODATA

        [HttpGet("odata")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<FaqSM>>>> GetAsOdata(
            ODataQueryOptions<FaqSM> oDataOptions)
        {
            var result = await GetAsEntitiesOdata(oDataOptions);
            return Ok(ModelConverter.FormNewSuccessResponse(result));
        }

        #endregion

        #region CREATE

        [HttpPost]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> Create(
            [FromBody] ApiRequest<FaqSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;

            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var response = await _process.CreateAsync(innerReq);

            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region READ (PUBLIC)

        [HttpGet]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User")]
        public async Task<ActionResult<ApiResponse<List<FaqSM>>>> GetAll(int skip = 0, int top = 10)
        {
            var response = await _process.GetAll(skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("count")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetAllCount()
        {
            var response = await _process.GetAllCount();
            return ModelConverter.FormNewSuccessResponse(response);
        }       
        

        [HttpGet("module")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin,User")]
        public async Task<ActionResult<ApiResponse<List<FaqSM>>>> GetByModule(FaqModuleSM module, int skip = 0, int top = 10)
        {
            var response = await _process.GetByModule(module, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("module/count")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetByModuleCount(FaqModuleSM module)
        {
            var response = await _process.GetByModuleCount(module);
            return ModelConverter.FormNewSuccessResponse(response);
        }


        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User")]
        public async Task<ActionResult<ApiResponse<FaqSM>>> GetById(long id)
        {
            var response = await _process.GetByIdAsync(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region ADMIN READ

        

        #endregion

        #region UPDATE

        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<FaqSM>>> Update(
            long id,
            [FromBody] ApiRequest<FaqSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;

            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var response = await _process.UpdateAsync(id, innerReq);

            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region DELETE

        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> Delete(long id)
        {
            var response = await _process.DeleteAsync(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion
    }
}

    