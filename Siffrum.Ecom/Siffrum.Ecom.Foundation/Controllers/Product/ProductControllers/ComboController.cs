using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Siffrum.Ecom.BAL.Product;
using Siffrum.Ecom.Foundation.Controllers.Base;
using Siffrum.Ecom.Foundation.Security;
using Siffrum.Ecom.ServiceModels.Foundation.Base.CommonResponseRoot;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Enums;
using Siffrum.Ecom.ServiceModels.v1;

namespace Siffrum.Ecom.Foundation.Controllers.Product.ProductControllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ComboController : ApiControllerWithOdataRoot<ComboProductSM>
    {
        private readonly ComboProcess _comboProcess;

        public ComboController(ComboProcess process)
            : base(process)
        {
            _comboProcess = process;
        }

        #region ODATA

        [HttpGet("odata")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ComboProductSM>>>> GetAsOdata(
            ODataQueryOptions<ComboProductSM> oDataOptions)
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
            Roles = "SuperAdmin, SystemAdmin, Seller")]
        public async Task<ActionResult<ApiResponse<List<ComboProductSM>>>> GetAll(int skip=0, int top=10)
        {
            var response = await _comboProcess.GetAllAsync(skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("count")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, Seller")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetAllCount()
        {
            var response = await _comboProcess.GetCountAsync();
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region Get By Id

        [HttpGet("{id}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<ComboProductSM>>> GetById(long id)
        {
            var response = await _comboProcess.GetByIdAsync(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #endregion

        #region Add

        [HttpPost("")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, Seller")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> Add(
            [FromBody] ApiRequest<ComboProductSM> apiRequest)
        {
            #region Check Request

            var innerReq = apiRequest?.ReqData;

            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            #endregion

            var response = await _comboProcess.CreateAsync(innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region Update

        [HttpPut()]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin,SystemAdmin, Seller")]
        public async Task<ActionResult<ApiResponse<ComboProductSM>>> Update(
            long id,
            [FromBody] ApiRequest<ComboProductSM> apiRequest)
        {
            #region Check Request

            var innerReq = apiRequest?.ReqData;

            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            #endregion

            var response = await _comboProcess.UpdateAsync(id, innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region Delete       

        [HttpDelete("{id}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, Seller")]
        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> DeleteByAdmin(long id)
        {
            var response = await _comboProcess.DeleteAsync(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region Combo Product Catgeory

        [HttpGet("combo-product/{id}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<ComboCategorySM>>> GetComboProductById(long id)
        {
            var response = await _comboProcess.GetComboByComboProductCategoryId(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpPost("combo-product/assign")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin,Seller")]
        public async Task<ActionResult<ApiResponse<ComboCategorySM>>> AssignComboProductById(long comboId, long categoryId)
        {
            var response = await _comboProcess.AssignComboToCategoryAsync(comboId, categoryId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpDelete("combo-product/{id}")]
        [Authorize(
           AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
           Roles = "SuperAdmin, SystemAdmin, Seller")]
        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> DeleteComboProductCategoryById(long id)
        {
            var response = await _comboProcess.DeleleComboCategoryAsync(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion Combo Product Catgeory
    }
}
