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
    public class ProductVariantController : ApiControllerWithOdataRoot<ProductVariantSM>
    {
        private readonly ProductVariantProcess _productProcess;

        public ProductVariantController(ProductVariantProcess process)
            : base(process)
        {
            _productProcess = process;
        }

        #region ODATA
        [HttpGet("odata")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductVariantSM>>>> GetAsOdata(
            ODataQueryOptions<ProductVariantSM> oDataOptions)
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
        public async Task<ActionResult<ApiResponse<List<ProductVariantSM>>>> GetAll(
            int skip, int top)
        {
            var response = await _productProcess.GetAll(skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("count")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetAllCount()
        {
            var response = await _productProcess.GetAllCount();
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("category")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<ProductVariantSM>>>> GetAllByCategoryId(long categoryId,
            int skip, int top)
        {
            var response = await _productProcess.GetAllByCategoryId(categoryId,skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("category/count")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetAllByCategoryIdCount(long categoryId)
        {
            var response = await _productProcess.GetAllByCategoryIdCount(categoryId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("product/{productId}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<ProductVariantSM>>>> GetAllByProductId(long productId)
        {
            var response = await _productProcess.GetAllVariantsByProductID(productId);
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
            if(role == RoleTypeSM.Seller.ToString())
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
        
        [HttpGet("mine")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<List<ProductVariantSM>>>> GetAllMine(
            int skip, int top)
        {
            #region Check Request

            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }

            #endregion Check Request

            var response = await _productProcess.GetAllMine(userId, skip, top);
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
            var response = await _productProcess.GetAllMineCount(userId);
            return ModelConverter.FormNewSuccessResponse(response);
        } 
        
        [HttpGet("mine/products/{productId}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<List<ProductVariantSM>>>> GetAllMineProductsByProductId(long productId,int skip, int top)
        {
            #region Check Request

            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }

            #endregion Check Request

            var response = await _productProcess.GetAllMineVariantsByProductId(userId, productId, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("mine/products/count/{productId}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetAllMineProductsByProductIdCount(long productId)
        {
            #region Check Request

            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }

            #endregion Check Request
            var response = await _productProcess.GetAllMineVariantsByProductIdCount(userId, productId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("seller/{sellerId}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<ProductVariantSM>>>> GetAllSellerProducts(long sellerId,
            int skip, int top)
        {
            #region Check Request

            

            #endregion Check Request

            var response = await _productProcess.GetAllMine(sellerId, skip, top);
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
            var response = await _productProcess.GetAllMineCount(sellerId);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        #endregion GetAll

        #region Get By Id

        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<ProductVariantSM>>> GetById(long id)
        {
            var response = await _productProcess.GetProductVariantById(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("hotbox/{id}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<HotBoxProductVariantSM>>> GetHotBoxVariant(long id)
        {
            var response = await _productProcess.GetProductVariantByHotBoxId(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        
        [HttpGet("hotbox/associated-variants/{id}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<List<UserHotBoxProductSM>>>> GetHotBoxAssociatedProductsByVariantId(long id)
        {
            var response = await _productProcess.GetHotBoxAssociatedProductsByVariantId(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        

        [HttpGet("speedymart/{id}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<SpeedyMartProductVariantSM>>> GetSpeedyMartVariant(long id)
        {
            var response = await _productProcess.GetProductVariantBySpeedyKartId(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion Get By Id

        #region Filter

        [HttpGet("hotbox/filter/indicator")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<List<UserHotBoxProductSM>>>> GetByFilterIndicator(ProductIndicatorSM indicator, int skip, int top)
        {
            var response = await _productProcess.GetHotBoxProductsByIndicator(indicator, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("hotbox/filter/indicator/count")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetByFilterIndicatorCount(ProductIndicatorSM indicator)
        {
            var response = await _productProcess.GetHotBoxProductsByIndicatorCount(indicator);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("hotbox/filter/nutrition/proteins")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<List<UserHotBoxProductSM>>>> GetByFilterProtiens(int skip, int top)
        {
            var response = await _productProcess.GetHotBoxProductsByHighestProtien( skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("hotbox/filter/nutrition/protiens/count")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetByFilterProtiensCount()
        {
            var response = await _productProcess.GetHotBoxProductsByHighestProtienCount();
            return ModelConverter.FormNewSuccessResponse(response);
        }
        
        [HttpGet("hotbox/most-ordered")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<List<HotBoxProductVariantSM>>>> GetHotBoxMostOrderedProducts(int skip, int top)
        {
            var response = await _productProcess.GetHotBoxMostOrderedProducts( skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("hotbox/most-ordered/count")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetHotBoxMostOrderedProductsCount()
        {
            var response = await _productProcess.GetHotBoxMostOrderedProductsCount();
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("hotbox/latest")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<List<HotBoxProductVariantSM>>>> GetHotBoxLatestProducts(int skip, int top)
        {
            var response = await _productProcess.GetHotBoxLatestProducts(skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("hotbox/latest/count")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetHotBoxLatestProductsCount()
        {
            var response = await _productProcess.GetHotBoxLatestProductsCount();
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("speedyMart/most-ordered")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<List<UserSpeedyMartProductSM>>>> GetSpeedyMartMostOrderedProducts(int skip, int top)
        {
            var response = await _productProcess.GetSpeedyMartMostOrderedProducts(skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("speedymart/most-ordered/count")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetSpeedyMartMostOrderedProductsCount()
        {
            var response = await _productProcess.GetSpeedyMartMostOrderedProductsCount();
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("speedymart/latest")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<List<UserSpeedyMartProductSM>>>> GetSpeedyMartLatestProducts(int skip, int top)
        {
            var response = await _productProcess.GetSpeedyMartLatestProducts(skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("speedymart/latest/count")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetSpeedyMartLatestProductsCount()
        {
            var response = await _productProcess.GetSpeedyMartLatestProductsCount();
            return ModelConverter.FormNewSuccessResponse(response);
        }

        /*[HttpGet("hotbox/search")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<List<UserHotBoxProductSM>>>> GetHotBoxSearch(ProductIndicatorSM indicator, string searchString, int skip, int top)
        {
            var response = await _productProcess.GetHotBoxProductsBySearchString(indicator,searchString, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }*/

        [HttpGet("hotbox/search")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<HotBoxSearchResponseSM>>> GetHotBoxSearchWithCombos(ProductIndicatorSM indicator, string searchString, int skip, int top)
        {
            long userId = 0;
            var role = User.GetUserRoleTypeFromCurrentUserClaims();
            if (role == RoleTypeSM.User.ToString())
            {
                userId = User.GetUserRecordIdFromCurrentUserClaims();
            }
            var response = await _productProcess.GetHotBoxProductsBySearchStringWithCombos(indicator, searchString, skip, top, userId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("hotbox/search/count")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetHotBoxSearchCount(ProductIndicatorSM indicator, string searchString)
        {
            long userId = 0;
            var role = User.GetUserRoleTypeFromCurrentUserClaims();
            if (role == RoleTypeSM.User.ToString())
            {
                userId = User.GetUserRecordIdFromCurrentUserClaims();
            }
            var response = await _productProcess.GetHotBoxProductsBySearchStringCount(indicator, searchString, userId);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        
        [HttpGet("speedymart/search")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<List<UserSpeedyMartProductSM>>>> GetHoGetSpeedyMartProductsBySearchtBoxSearch(string searchString,int skip, int top)
        {
            var response = await _productProcess.GetSpeedyMartProductsBySearch(searchString, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("speedymart/search/count")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetSpeedyMartProductsBySearchCount(string searchString)
        {
            var response = await _productProcess.GetSpeedyMartProductsBySearchCount(searchString);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion Filter

        #endregion Get

        #region Add
        [HttpPost("mine")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<ProductVariantSM>>> Add([FromBody] ApiRequest<ProductVariantSM> apiRequest)
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
        
        [HttpPost("associate")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<ProductVariantSM>>> AssociateProduct([FromBody] ApiRequest<SellerProductAssociationsSM> apiRequest)
        {
            #region Check Request 
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed, ApiErrorTypeSM.InvalidInputData_NoLog));
            }
            
            #endregion Check Request

            var response = await _productProcess.AssignProductToSeller(innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        
        [HttpGet("associate-sellers")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<SearchResponseSM>>>> GetAssignedSellerDetails(long productVariantId)
        {
            #region Check Request 
            
            #endregion Check Request

            var response = await _productProcess.GetAssignedSellers(productVariantId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region Update

        [HttpPut("mine")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<ProductVariantSM>>> Update(long id, [FromBody] ApiRequest<ProductVariantSM> apiRequest)
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
            var response = await _productProcess.UpdateProduct(userId, id, innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        
        [HttpPut("update-stock")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<ProductVariantSM>>> UpdateStock(long variantId, decimal stock)
        {
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_Id_NotFound));

            var response = await _productProcess.UpdateStock(userId, variantId, stock);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpPut("status")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<ProductVariantSM>>> UpdateProductStatus(long id, ProductStatusSM status)
        {
            #region Check Request 
            
            #endregion Check Request
            var response = await _productProcess.UpdateProductStatus(id, status);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpPut("status/bulk")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> BulkUpdateProductStatus([FromBody] ApiRequest<BulkStatusUpdateSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed, ApiErrorTypeSM.InvalidInputData_NoLog));

            var response = await _productProcess.BulkUpdateProductStatus(innerReq.Ids, innerReq.Status);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpPost("associate/bulk")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> BulkAssignToSellers([FromBody] ApiRequest<BulkAssignToSellersSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed, ApiErrorTypeSM.InvalidInputData_NoLog));

            var response = await _productProcess.BulkAssignProductToSellers(innerReq.ProductVariantId, innerReq.SellerIds);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpPut("price/bulk")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> BulkUpdatePrice([FromBody] ApiRequest<BulkPriceUpdateSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed, ApiErrorTypeSM.InvalidInputData_NoLog));

            var response = await _productProcess.BulkUpdatePrice(innerReq.ProductVariantId, innerReq.Price, innerReq.DiscountedPrice, innerReq.SellerId);
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

            var response = await _productProcess.DeleteMineProduct(userId, id);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        
        [HttpPost("admin")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<ProductVariantSM>>> AddByAdmin([FromBody] ApiRequest<ProductVariantSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed, ApiErrorTypeSM.InvalidInputData_NoLog));
            var response = await _productProcess.AddProductByAdmin(innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpPut("admin/{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<ProductVariantSM>>> UpdateByAdmin(long id, [FromBody] ApiRequest<ProductVariantSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed, ApiErrorTypeSM.InvalidInputData_NoLog));
            var response = await _productProcess.UpdateProductByAdmin(id, innerReq);
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

    }
}
