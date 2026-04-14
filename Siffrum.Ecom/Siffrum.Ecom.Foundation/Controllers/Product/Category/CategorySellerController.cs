using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Siffrum.Ecom.DAL.Context;
using Siffrum.Ecom.DomainModels.v1;
using Siffrum.Ecom.Foundation.Controllers.Base;
using Siffrum.Ecom.Foundation.Security;
using Siffrum.Ecom.ServiceModels.Foundation.Base.CommonResponseRoot;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Enums;
using Siffrum.Ecom.ServiceModels.v1;

namespace Siffrum.Ecom.Foundation.Controllers.Product.Category
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CategorySellerController : ApiControllerRoot
    {
        private readonly ApiDbContext _db;

        public CategorySellerController(ApiDbContext db)
        {
            _db = db;
        }

        [HttpGet("{categoryId}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<CategorySellerSM>>>> GetByCategoryId(long categoryId)
        {
            var list = await _db.CategorySellers
                .AsNoTracking()
                .Where(cs => cs.CategoryId == categoryId)
                .Include(cs => cs.Seller)
                .Select(cs => new CategorySellerSM
                {
                    Id = cs.Id,
                    CategoryId = cs.CategoryId,
                    SellerId = cs.SellerId,
                    SellerName = cs.Seller.StoreName ?? cs.Seller.Name
                })
                .ToListAsync();
            return Ok(ModelConverter.FormNewSuccessResponse(list));
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<CategorySellerSM>>>> AssignSellers(
            [FromBody] ApiRequest<CategorySellerAssignRequestSM> apiRequest)
        {
            var req = apiRequest?.ReqData;
            if (req == null || req.CategoryId <= 0 || req.SellerIds == null || !req.SellerIds.Any())
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    "CategoryId and at least one SellerId are required",
                    ApiErrorTypeSM.InvalidInputData_NoLog));

            var existing = await _db.CategorySellers
                .Where(cs => cs.CategoryId == req.CategoryId)
                .Select(cs => cs.SellerId)
                .ToListAsync();

            var toAdd = req.SellerIds.Where(sid => !existing.Contains(sid)).ToList();
            foreach (var sid in toAdd)
            {
                _db.CategorySellers.Add(new CategorySellerDM
                {
                    CategoryId = req.CategoryId,
                    SellerId = sid,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            await _db.SaveChangesAsync();

            var result = await _db.CategorySellers
                .AsNoTracking()
                .Where(cs => cs.CategoryId == req.CategoryId)
                .Include(cs => cs.Seller)
                .Select(cs => new CategorySellerSM
                {
                    Id = cs.Id,
                    CategoryId = cs.CategoryId,
                    SellerId = cs.SellerId,
                    SellerName = cs.Seller.StoreName ?? cs.Seller.Name
                })
                .ToListAsync();
            return Ok(ModelConverter.FormNewSuccessResponse(result));
        }

        [HttpDelete("{categoryId}/{sellerId}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> RemoveSeller(long categoryId, long sellerId)
        {
            var entity = await _db.CategorySellers
                .FirstOrDefaultAsync(cs => cs.CategoryId == categoryId && cs.SellerId == sellerId);
            if (entity == null)
                return NotFound(ModelConverter.FormNewErrorResponse("Assignment not found", ApiErrorTypeSM.InvalidInputData_NoLog));

            _db.CategorySellers.Remove(entity);
            await _db.SaveChangesAsync();
            return Ok(ModelConverter.FormNewSuccessResponse(new DeleteResponseRoot(true, "Seller removed from category")));
        }
    }

    public class CategorySellerAssignRequestSM
    {
        public long CategoryId { get; set; }
        public List<long> SellerIds { get; set; }
    }
}
