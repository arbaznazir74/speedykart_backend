using AutoMapper.Internal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Siffrum.Ecom.BAL.Foundation.Web;
using Siffrum.Ecom.BAL.Product;
using Siffrum.Ecom.DomainModels.Enums;
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
    public class UnitController : ApiControllerWithOdataRoot<UnitSM>
    {
        private readonly UnitProcess _unitProcess;

        public UnitController(UnitProcess unitProcess)
            : base(unitProcess)
        {
            _unitProcess = unitProcess;
        }

        #region ODATA
        [HttpGet("odata")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<UnitSM>>>> GetAsOdata(
            ODataQueryOptions<UnitSM> oDataOptions)
        {
            var retList = await GetAsEntitiesOdata(oDataOptions);
            return Ok(ModelConverter.FormNewSuccessResponse(retList));
        }
        #endregion

        #region CREATE
        [HttpPost]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin, Seller")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> CreateBrand(
            [FromBody] ApiRequest<UnitSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var response = await _unitProcess.CreateAsync(innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        #endregion

        #region READ
        [HttpGet]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin,User, Seller")]
        public async Task<ActionResult<ApiResponse<List<UnitSM>>>> GetAllUnits(int skip, int top)
        {
            var response = await _unitProcess.GetAll(skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("count")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin,User, Seller")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetUnitsCount()
        {
            var response = await _unitProcess.GetCount();
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("parent")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin,User, Seller")]
        public async Task<ActionResult<ApiResponse<List<UnitSM>>>> GetAllParentUnits(long parentId, int skip, int top)
        {
            var response = await _unitProcess.GetAllByParentId(parentId, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("parent/count")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin,User, Seller")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetUnitsByParentCount(long parentId)
        {
            var response = await _unitProcess.GetByParentCount(parentId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin,User, Seller")]
        public async Task<ActionResult<ApiResponse<UnitSM>>> GetUnitById(long id)
        {
            var response = await _unitProcess.GetByIdAsync(id);

            return ModelConverter.FormNewSuccessResponse(response);
        }
        #endregion

        [HttpGet("search")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, Seller")]
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
            var response = await _unitProcess.SearchUnits(searchString, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #region UPDATE
        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<UnitSM>>> UpdateUnit(long id, [FromBody] ApiRequest<UnitSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var response = await _unitProcess.UpdateAsync(id, innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        #endregion

        #region DELETE
        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> DeleteUnit(long id)
        {
            var response = await _unitProcess.DeleteAsync(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        #endregion

        #region SELLER
        [HttpPost("mine")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> CreateForSeller(
            [FromBody] ApiRequest<UnitSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var sellerId = User.GetUserRecordIdFromCurrentUserClaims();
            if (sellerId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(
                    DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }

            var response = await _unitProcess.CreateForSellerAsync(innerReq, sellerId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("mine")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<List<UnitSM>>>> GetForSeller(int skip = 0, int top = 200)
        {
            var sellerId = User.GetUserRecordIdFromCurrentUserClaims();
            if (sellerId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(
                    DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }

            var response = await _unitProcess.GetForSeller(sellerId, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpDelete("mine/{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> DeleteForSeller(long id)
        {
            var sellerId = User.GetUserRecordIdFromCurrentUserClaims();
            if (sellerId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(
                    DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }

            var response = await _unitProcess.DeleteForSellerAsync(id, sellerId);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        #endregion
    }
}
