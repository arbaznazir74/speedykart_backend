using Microsoft.AspNetCore.Authorization;
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
    public class ProductController : ApiControllerWithOdataRoot<ProductSM>
    {
        private readonly ProductProcess _productProcess;

        public ProductController(ProductProcess process)
            : base(process)
        {
            _productProcess = process;
        }

        #region ODATA
        [HttpGet("odata")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductSM>>>> GetAsOdata(
            ODataQueryOptions<ProductSM> oDataOptions)
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
        public async Task<ActionResult<ApiResponse<List<ProductSM>>>> GetAll(
            int skip, int top, PlatformTypeSM? platformType = null)
        {
            var response = await _productProcess.GetAll(skip, top, platformType);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("count")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetAllCount(PlatformTypeSM? platformType = null)
        {
            var response = await _productProcess.GetAllCount(platformType);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("mine")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<List<ProductSM>>>> GetAllMine(
            int skip, int top)
        {
            #region Check Request

            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }

            #endregion Check Request

            var response = await _productProcess.GetAllSellerProducts(userId, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("mine/count")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetAllMineCount()
        {
            #region Check Request

            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }

            #endregion Check Request
            var response = await _productProcess.GetAllSellerProductsCount(userId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("seller/{sellerId}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<ProductSM>>>> GetAllSellerProducts(long sellerId,
            int skip, int top)
        {
            #region Check Request



            #endregion Check Request

            var response = await _productProcess.GetAllSellerProducts(sellerId, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("seller/count/{sellerId}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetAllSellerProductsCount(long sellerId)
        {
            #region Check Request
            #endregion Check Request
            var response = await _productProcess.GetAllSellerProductsCount(sellerId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("search")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, Seller, User")]
        public async Task<ActionResult<ApiResponse<List<SearchResponseSM>>>> GetAllBySearch(string searchString,
            int skip = 0, int top = 50)
        {
            long sellerId = 0;
            var role = User.GetUserRoleTypeFromCurrentUserClaims();
            if (role == RoleTypeSM.Seller.ToString())
            {
                sellerId = User.GetUserRecordIdFromCurrentUserClaims();
                if (sellerId <= 0)
                {
                    return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
                }
            }
            var response = await _productProcess.SearchProducts(sellerId, searchString, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion GetAll

        #region Get By Id

        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<ProductSM>>> GetById(long id)
        {
            var response = await _productProcess.GetProductById(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion Get By Id

        #endregion Get

        #region Add
        [HttpPost("mine")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<ProductSM>>> Add([FromBody] ApiRequest<ProductSM> apiRequest)
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
            var response = await _productProcess.AddProduct(userId, innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region Update

        [HttpPut("mine")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> Update(long id, [FromBody] ApiRequest<ProductSM> apiRequest)
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
            var response = await _productProcess.UpdateProduct(userId,id, innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion Update

        #region Delete
        [HttpDelete("mine/{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> Delete(long id)
        {
            #region Check Request

            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_Id_NotFound));
            }

            #endregion Check Request
            var response = await _productProcess.DeleteProduct(userId, id);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        
        [HttpDelete("admin/{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> DeleteByAdmin(long id)
        {
            #region Check Request


            #endregion Check Request
            var response = await _productProcess.DeleteProductByAdmin(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        #endregion

        #region Admin Create / Update

        [HttpPost("admin")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<ProductSM>>>> AddByAdmin(
            [FromBody] ApiRequest<AdminProductCreateSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }
            var response = await _productProcess.AddProductByAdmin(innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpPut("admin/{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> UpdateByAdmin(
            long id, [FromBody] ApiRequest<ProductSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }
            var response = await _productProcess.UpdateProductByAdmin(id, innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion Admin Create / Update

    }
}
