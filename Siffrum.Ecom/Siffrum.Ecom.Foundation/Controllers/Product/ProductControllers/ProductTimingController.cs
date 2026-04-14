using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Siffrum.Ecom.BAL.Foundation.Web;
using Siffrum.Ecom.BAL.Product;
using Siffrum.Ecom.Foundation.Security;
using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base.CommonResponseRoot;
using Siffrum.Ecom.ServiceModels.v1;

namespace Siffrum.Ecom.Foundation.Controllers.Product.ProductControllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ProductTimingController : ControllerBase
    {
        private readonly ProductTimingProcess _process;

        public ProductTimingController(ProductTimingProcess process)
        {
            _process = process;
        }

        #region Seller CRUD

        [HttpPost]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<ProductTimingSM>>> Create(
            [FromBody] ApiRequest<ProductTimingSM> request)
        {
            var result = await _process.Create(request.ReqData);
            return ModelConverter.FormNewSuccessResponse(result);
        }

        [HttpGet("seller")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<List<ProductTimingSM>>>> GetBySeller(
            int skip = 0, int top = 20)
        {
            var result = await _process.GetBySeller(skip, top);
            return ModelConverter.FormNewSuccessResponse(result);
        }

        [HttpGet("seller/count")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetBySellerCount()
        {
            var result = await _process.GetBySellerCount();
            return ModelConverter.FormNewSuccessResponse(result);
        }

        [HttpPut("{id}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<ProductTimingSM>>> Update(
            long id, [FromBody] ApiRequest<ProductTimingSM> request)
        {
            var result = await _process.Update(id, request.ReqData);
            if (result == null) return NotFound();
            return ModelConverter.FormNewSuccessResponse(result);
        }

        [HttpDelete("{id}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> Delete(long id)
        {
            var result = await _process.Delete(id);
            return ModelConverter.FormNewSuccessResponse(result);
        }

        #endregion Seller CRUD

        #region User Timing Query

        [HttpGet("hotbox/timing")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User")]
        public async Task<ActionResult<ApiResponse<UserHotBoxCategoryProductsSM>>> GetByTiming(
            CategoryTimingSM timing, int skip = 0, int top = 10)
        {
            var result = await _process.GetProductsByTiming(timing, skip, top);
            return ModelConverter.FormNewSuccessResponse(result);
        }

        [HttpGet("hotbox/timing/count")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetByTimingCount(
            CategoryTimingSM timing)
        {
            var result = await _process.GetProductsByTimingCount(timing);
            return ModelConverter.FormNewSuccessResponse(result);
        }

        #endregion User Timing Query
    }
}
