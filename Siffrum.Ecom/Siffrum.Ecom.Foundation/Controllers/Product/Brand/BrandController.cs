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

namespace Siffrum.Ecom.Foundation.Controllers.Product.Brand
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class BrandController : ApiControllerWithOdataRoot<BrandSM>
    {
        private readonly BrandProcess _brandProcess;

        public BrandController(BrandProcess brandProcess)
            : base(brandProcess)
        {
            _brandProcess = brandProcess;
        }

        #region ODATA
        [HttpGet("odata")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<BrandSM>>>> GetAsOdata(
            ODataQueryOptions<BrandSM> oDataOptions)
        {
            var retList = await GetAsEntitiesOdata(oDataOptions);
            return Ok(ModelConverter.FormNewSuccessResponse(retList));
        }
        #endregion

        #region CREATE
        [HttpPost]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> CreateBrand(
            [FromBody] ApiRequest<BrandSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstants.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var response = await _brandProcess.CreateAsync(innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        #endregion

        #region READ
        [HttpGet]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin,User, Seller")]
        public async Task<ActionResult<ApiResponse<List<BrandSM>>>> GetAllBrands(int skip,int top)
        {
            #region Check Request
            
            var role = User.GetUserRoleTypeFromCurrentUserClaims();
            if (string.IsNullOrEmpty(role))
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Role_NotFound));
            }
            #endregion Check Request           

            var response = await _brandProcess.GetAll(skip, top, role);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("count")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin,User, Seller")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetBrandsCount()
        {
            #region Check Request

            var role = User.GetUserRoleTypeFromCurrentUserClaims();
            if (string.IsNullOrEmpty(role))
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Role_NotFound));
            }
            #endregion Check Request

            var response = await _brandProcess.GetCount(role);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin,User, Seller")]
        public async Task<ActionResult<ApiResponse<BrandSM>>> GetBrandById(long id)
        {
            #region Check Request

            var role = User.GetUserRoleTypeFromCurrentUserClaims();
            if (string.IsNullOrEmpty(role))
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Role_NotFound));
            }
            #endregion Check Request    
            var response = await _brandProcess.GetByIdAsync(id, role);           

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

            var response = await _brandProcess.SearchBrands( searchString, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        #endregion

        #region UPDATE
        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<BrandSM>>> UpdateBrand(long id,[FromBody] ApiRequest<BrandSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstants.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var response = await _brandProcess.UpdateAsync(id, innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpPut("status/{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> UpdateStatus(long id, StatusSM status)
        {
            var response = await _brandProcess.UpdateStatusAsync(id, status);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        #endregion

        #region DELETE
        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> DeleteBrand(long id)
        {
            var response = await _brandProcess.DeleteAsync(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        #endregion
    }
}
