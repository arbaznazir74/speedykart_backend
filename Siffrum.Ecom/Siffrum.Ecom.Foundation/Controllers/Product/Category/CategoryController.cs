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

namespace Siffrum.Ecom.Foundation.Controllers.Product.Category
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CategoryController : ApiControllerWithOdataRoot<CategorySM>
    {
        private readonly CategoryProcess _categoryProcess;

        public CategoryController(CategoryProcess categoryprocess)
            : base(categoryprocess)
        {
            _categoryProcess = categoryprocess;
        }

        #region ODATA
        [HttpGet("odata")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<CategorySM>>>> GetAsOdata(
            ODataQueryOptions<CategorySM> oDataOptions)
        {
            var retList = await GetAsEntitiesOdata(oDataOptions);
            return Ok(ModelConverter.FormNewSuccessResponse(retList));
        }
        #endregion

        #region CREATE
        [HttpPost]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> CreateCategory(
            [FromBody] ApiRequest<CategorySM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstants.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var response = await _categoryProcess.CreateAsync(innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        #endregion

        #region Admin/User/Seller Get All

        #region Get All

        [HttpGet("parent/admin")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<CategorySM>>>> GetParentCategoriesForAdmin(
            int skip, int top, PlatformTypeSM? platform = null)
        {
            var response = await _categoryProcess.GetParentCategoriesForAdmin(skip, top, platform);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("parent/admin/count")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetParentCategoriesForAdminCount(PlatformTypeSM? platform = null)
        {
            var response = await _categoryProcess.GetParentCategoriesForAdminCount(platform);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("search")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, Seller, User")]
        public async Task<ActionResult<ApiResponse<List<SearchResponseSM>>>> GetAllBySearch(PlatformTypeSM platform, string searchString,
            int skip = 0, int top = 50)
        {
            var role = User.GetUserRoleTypeFromCurrentUserClaims();
            var isToShowAllCategories = false;
            if (role == RoleTypeSM.SuperAdmin.ToString() || role == RoleTypeSM.SystemAdmin.ToString())
            {
                isToShowAllCategories = true;
            }
            var response = await _categoryProcess.SearchCategories(platform, searchString, skip, top, isToShowAllCategories);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("sub/admin/{parentCategoryId}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<CategorySM>>>> GetSubCategoriesForAdmin(
            long parentCategoryId, int skip, int top)
        {
            var response = await _categoryProcess.GetSubCategoriesForAdmin(parentCategoryId, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("sub/admin/count/{parentCategoryId}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetSubCategoriesForAdminCount(
            long parentCategoryId)
        {
            var response = await _categoryProcess.GetSubCategoriesForAdminCount(parentCategoryId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion Get All

        #region User

        [HttpGet("parent/user")]
        [Authorize(
           AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
           Roles = "User")]
        public async Task<ActionResult<ApiResponse<List<CategorySM>>>> GetParentCategoriesForEndUser(PlatformTypeSM platform,
           int skip, int top)
        {
            var response = await _categoryProcess.GetParentCategoriesForEndUser(platform, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("parent/user/count")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetParentCategoriesForEndUserCount(PlatformTypeSM platform)
        {
            var response = await _categoryProcess.GetParentCategoriesForEndUserCount(platform);
            return ModelConverter.FormNewSuccessResponse(response);
        }        

        [HttpGet("sub/user/{parentCategoryId}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User")]
        public async Task<ActionResult<ApiResponse<List<CategorySM>>>> GetSubCategoriesForEndUser(
            long parentCategoryId, PlatformTypeSM platform, int skip, int top)
        {
            var response = await _categoryProcess.GetSubCategoriesForEndUserByParentCategory(parentCategoryId, platform, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("sub/user/count/{parentCategoryId}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetSubCategoriesForEndUserCount(
            long parentCategoryId, PlatformTypeSM platform)
        {
            var response = await _categoryProcess.GetSubCategoriesForEndUserCount(parentCategoryId, platform);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("sub/user")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User")]
        public async Task<ActionResult<ApiResponse<List<CategorySM>>>> GetAllSubCategoriesForUser(
            PlatformTypeSM platform, int skip, int top)
        {
            var response = await _categoryProcess.GetSubCategoriesForUser(platform, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("summary/user")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User")]
        public async Task<ActionResult<ApiResponse<List<UserCategorySummarySM>>>> GetCategorySummaryForUser(
            PlatformTypeSM platform, int skip = 0, int top = 50)
        {
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            var response = await _categoryProcess.GetCategorySummaryForUser(platform, skip, top, userId);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        [HttpGet("sub/user/more-on-hot-box")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User")]
        public async Task<ActionResult<ApiResponse<List<CategorySM>>>> GetRandomSubCategoriesForUser(
            PlatformTypeSM platform, int top)
        {
            var response = await _categoryProcess.GetRandomSubCategoriesForUser(platform, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("sub/user/count")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetAllSubCategoriesForUserCountAsync(
            PlatformTypeSM platform)
        {
            var response = await _categoryProcess.GetSubCategoriesForUserCount(platform);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("menu/user")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User")]
        public async Task<ActionResult<ApiResponse<List<SearchResponseSM>>>> GetAllSubCategoriesForUserAsync(
            PlatformTypeSM platform, int skip, int top)
        {
            var response = await _categoryProcess.GetSubCategoriesForUserAsync(platform, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("menu/user/count")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetAllSubCategoriesForUserCount(
            PlatformTypeSM platform)
        {
            var response = await _categoryProcess.GetSubCategoriesForUserCount(platform);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        

        [HttpGet("hot-box/products/user/timing")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User")]
        public async Task<ActionResult<ApiResponse<UserHotBoxCategoryProductsSM>>> GetProductsInHotBoxUsingCategoryTiming(
            CategoryTimingSM timing, int skip, int top)
        {
            var response = await _categoryProcess.GetTopHotBoxCategoryByTiming(timing, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("hot-box/products/user/count/timing/count")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetProductsInHotBoxUsingCategoryTimingCount(
            CategoryTimingSM timing)
        {
            var response = await _categoryProcess.GetProductsInHotBoxUsingTimingsCount(timing);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion User

        #region Seller

        [HttpGet("parent/seller")]
        [Authorize(
           AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
           Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<List<CategorySM>>>> GetParentCategoriesForSeller(
           int skip, int top, PlatformTypeSM? platform = null, long sellerId = 0)
        {
            var response = await _categoryProcess.GetParentCategoriesForSeller(skip, top, platform, sellerId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("parent/seller/count")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetParentCategoriesForSellerCount(PlatformTypeSM? platform = null, long sellerId = 0)
        {
            var response = await _categoryProcess.GetParentCategoriesForSellerCount(platform, sellerId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("platform/sub/seller/{parentCategoryId}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<List<CategorySM>>>> GetSubCategoriesForSeller(
            long parentCategoryId, PlatformTypeSM platform, int skip, int top)
        {
            var response = await _categoryProcess.GetSubCategoriesForSellerByParentCategory(parentCategoryId, platform, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("platform/sub/seller/count/{parentCategoryId}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetSubCategoriesForSellerCount(
            long parentCategoryId, PlatformTypeSM platform)
        {
            var response = await _categoryProcess.GetSubCategoriesForSellerCount(parentCategoryId, platform);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("sub/seller/{parentCategoryId}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<List<CategorySM>>>> GetSubCategoriesForSellerWithoutPlatform(
            long parentCategoryId, int skip, int top)
        {
            var response = await _categoryProcess.GetSubCategoriesForSellerByParentCategory(parentCategoryId, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("sub/seller/count/{parentCategoryId}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetSubCategoriesForSellerWithoutPlatformCount(
            long parentCategoryId)
        {
            var response = await _categoryProcess.GetSubCategoriesForSellerCount(parentCategoryId);
            return ModelConverter.FormNewSuccessResponse(response);
        }



        [HttpGet("sub/seller")]
        [Authorize(
           AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
           Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<List<CategorySM>>>> GetAllSubCategoriesForSeller(
           PlatformTypeSM platform, int skip, int top)
        {
            var response = await _categoryProcess.GetSubCategoriesForSeller(platform, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("sub/seller/count")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetAllSubCategoriesForSellerCount(
            PlatformTypeSM platform)
        {
            var response = await _categoryProcess.GetSubCategoriesForSellerCount(platform);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion Seller

        #endregion Admin/User/Seller Get All

        #region Products

        [HttpGet("speedy-mart/products/user/{categoryId}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User")]
        public async Task<ActionResult<ApiResponse<UserSpeedyMartCategoryProductsSM>>> GetProductsInSpeedyMartUsingCategory(
            long categoryId,int skip, int top, int comboProductCount)
        {
            var response = await _categoryProcess.GetProductsInSpeedyMartUsingCategory(categoryId, skip, top, comboProductCount);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("speedy-mart/products/user/count/{categoryId}")]
        [Authorize(
           AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
           Roles = "User")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetProductsInSpeedyMartUsingCategoryCount(
           long categoryId)
        {
            var response = await _categoryProcess.GetProductsInSpeedyMartUsingCategoryCount(categoryId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("hot-box/products/user/{categoryId}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User")]
        public async Task<ActionResult<ApiResponse<UserHotBoxCategoryProductsSM>>> GetProductsInHotBoxUsingCategory(
            long categoryId, int skip, int top, int comboProductCount)
        {
            var response = await _categoryProcess.GetProductsInHotBoxUsingCategory(categoryId, skip, top, comboProductCount);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("hot-box/products/full/user/{categoryId}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User")]
        public async Task<ActionResult<ApiResponse<UserHotBoxCategoryFullProductsSM>>> GetFullProductsInHotBoxUsingCategory(
            long categoryId, int skip, int top, int comboProductCount = 5)
        {
            var response = await _categoryProcess.GetFullProductsInHotBoxUsingCategory(categoryId, skip, top, comboProductCount);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("hot-box/products/user/count/{categoryId}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetProductsInHotBoxUsingCategoryCount(
            long categoryId)
        {
            var response = await _categoryProcess.GetProductsInHotBoxUsingCategoryCount(categoryId);
            return ModelConverter.FormNewSuccessResponse(response);
        }



        #endregion Products

        #region READ BY ID

        [HttpGet("{id}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User, Seller, SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<CategorySM>>> GetCategoryById(long id)
        {

            var response = await _categoryProcess.GetByIdAsync(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region SELLER SUGGEST

        [HttpPost("suggest")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<CategorySM>>> SellerSuggestCategory(
            [FromBody] ApiRequest<CategorySM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null || string.IsNullOrWhiteSpace(innerReq.Name))
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    "Category name is required",
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var sellerId = innerReq.SuggestedBySellerId ?? 0;
            if (sellerId <= 0)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    "Seller ID is required",
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var platform = innerReq.Platform ?? PlatformTypeSM.HotBox;
            var response = await _categoryProcess.SellerSuggestCategoryAsync(innerReq, sellerId, platform);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region ADMIN APPROVAL

        [HttpGet("pending")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<CategorySM>>>> GetPendingCategories()
        {
            var response = await _categoryProcess.GetPendingCategories();
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpPut("approve/{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> ApproveCategory(long id)
        {
            var response = await _categoryProcess.ApproveCategoryAsync(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpPut("reject/{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> RejectCategory(long id)
        {
            var response = await _categoryProcess.RejectCategoryAsync(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region UPDATE

        [HttpPut("{id}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<CategorySM>>> UpdateCategory(
            long id, [FromBody] ApiRequest<CategorySM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstants.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var response = await _categoryProcess.UpdateAsync(id, innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region DELETE

        [HttpPut("status/{id}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> UpdateCategoryStatus(long id, StatusSM status)
        {
            var response = await _categoryProcess.UpdateCategoryStatus(id, status);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        
        [HttpDelete("{id}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> DeleteCategory(long id)
        {
            var response = await _categoryProcess.DeleteAsync(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion
    }
}
