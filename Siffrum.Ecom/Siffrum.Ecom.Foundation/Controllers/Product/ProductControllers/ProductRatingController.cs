using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Siffrum.Ecom.BAL.Foundation.Web;
using Siffrum.Ecom.BAL.Product;
using Siffrum.Ecom.Foundation.Controllers.Base;
using Siffrum.Ecom.Foundation.Security;
using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base.CommonResponseRoot;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Enums;
using Siffrum.Ecom.ServiceModels.v1;

namespace Siffrum.Ecom.Foundation.Controllers.Product.ProductControllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ProductRatingController : ApiControllerWithOdataRoot<ProductRatingSM>
    {
        private readonly ProductRatingProcess _productRatingProcess;

        public ProductRatingController(ProductRatingProcess process)
            : base(process)
        {
            _productRatingProcess = process;
        }

        #region ODATA
        [HttpGet("odata")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductRatingSM>>>> GetAsOdata(
            ODataQueryOptions<ProductRatingSM> oDataOptions)
        {
            var retList = await GetAsEntitiesOdata(oDataOptions);
            return Ok(ModelConverter.FormNewSuccessResponse(retList));
        }
        #endregion

        #region Get

        #region GetAll
        [HttpGet]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<ProductRatingSM>>>> GetAll(
            int skip, int top)
        {
            var response = await _productRatingProcess.GetAll(skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("count")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetAllCount()
        {
            var response = await _productRatingProcess.GetAllCount();
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion GetAll

        #region Get By Id

        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<ProductRatingSM>>> GetById(long id)
        {
            var response = await _productRatingProcess.GetProductRatingById(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion Get By Id

        #endregion Get

        #region Add
        [HttpPost]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "User")]
        public async Task<ActionResult<ApiResponse<ProductRatingSM>>> Add([FromBody] ApiRequest<ProductRatingSM> apiRequest)
        {
            #region Check Request 
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed, ApiErrorTypeSM.InvalidInputData_NoLog));
            }
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_Id_NotFound));
            }
            #endregion Check Request
            var response = await _productRatingProcess.AddProductRating(innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region Update

        [HttpPut]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> Update(long id, StatusSM status)
        {
            #region Check Request 

            #endregion Check Request
            var response = await _productRatingProcess.UpdateProductRatingStatus(id, status);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion Update

        #region Delete
        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> Delete(long id)
        {
            var response = await _productRatingProcess.DeleteProductRating(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        #endregion

    }
}
