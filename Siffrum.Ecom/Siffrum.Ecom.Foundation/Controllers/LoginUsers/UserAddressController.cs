using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Siffrum.Ecom.BAL.Base.Location;
using Siffrum.Ecom.BAL.Foundation.Web;
using Siffrum.Ecom.BAL.LoginUsers;
using Siffrum.Ecom.Foundation.Controllers.Base;
using Siffrum.Ecom.Foundation.Security;
using Siffrum.Ecom.ServiceModels.Foundation.Base.CommonResponseRoot;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Enums;
using Siffrum.Ecom.ServiceModels.v1;

namespace Siffrum.Ecom.Foundation.Controllers.LoginUsers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UserAddressController : ApiControllerWithOdataRoot<UserAddressSM>
    {
        private readonly UserAddressProcess _process;
        private readonly GeoLocationService _geoservice;
        public UserAddressController(UserAddressProcess process, GeoLocationService geoservice)
            : base(process)
        {
            _process = process;
            _geoservice = geoservice;
        }

        #region ODATA
        [HttpGet("odata")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin,SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<UserAddressSM>>>> GetAsOdata(
            ODataQueryOptions<UserAddressSM> options)
        {
            var data = await GetAsEntitiesOdata(options);
            return Ok(ModelConverter.FormNewSuccessResponse(data));
        }
        #endregion

        #region Admin Get 

        [HttpGet()]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<UserAddressSM>>>> GetAll(int skip, int top)
        {
            
            var data = await _process.GetAllAddresses( skip, top);
            return ModelConverter.FormNewSuccessResponse(data);
        }

        [HttpGet("count")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]

        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetAllCount()
        {

            var data = await _process.GetAllAddressesCount();
            return ModelConverter.FormNewSuccessResponse(data);
        }

        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<UserAddressSM>>> GetByIdAsync(long id)
        {
            var data = await _process.GetById(id);
            return ModelConverter.FormNewSuccessResponse(data);
        }

        #endregion Admin Get 

        #region GET MINE

        [HttpGet("mine")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "User")]
        public async Task<ActionResult<ApiResponse<List<UserAddressSM>>>> GetMine(int skip, int top)
        {
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }

            var data = await _process.GetAllMine(userId, skip, top);
            return ModelConverter.FormNewSuccessResponse(data);
        }

        [HttpGet("mine/count")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "User")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetMineCount()
        {
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }
            var data = await _process.GetAllMineCount(userId);
            return ModelConverter.FormNewSuccessResponse(data);
        }

        [HttpGet("mine/{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "User")]
        public async Task<ActionResult<ApiResponse<UserAddressSM>>> GetById(long id)
        {
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }

            var data = await _process.GetByIdAsync(userId, id);
            return ModelConverter.FormNewSuccessResponse(data);
        }
        [HttpGet("default")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "User")]
        public async Task<ActionResult<ApiResponse<UserAddressSM>>> GetDefaultAddress()
        {
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }

            var data = await _process.GetDefaultAddress(userId);
            return ModelConverter.FormNewSuccessResponse(data);
        }



        #endregion

        #region Address By Api

        [HttpPost("latlong")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "User, DeliveryBoy")]
        public async Task<ActionResult<ApiResponse<UserAddressSM>>> GetByLatlong([FromBody] ApiRequest<LocationRequestSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_ReqDataNotFormed, ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var data = await _geoservice.GetAddressFromLatLongAsync((double)innerReq.Latitude, (double)innerReq.Longitude);
            return ModelConverter.FormNewSuccessResponse(data);
        }

        [HttpGet("address/search")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "User, DeliveryBoy")]
        public async Task<ActionResult<ApiResponse<List<UserAddressSM>>>> GetBySearch(string searchString)
        {           

            var data = await _geoservice.GetAddressFromSearchAsync(searchString);
            return ModelConverter.FormNewSuccessResponse(data);
        }

        #endregion Address By Api

        #region CREATE
        [HttpPost("mine")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "User")]
        public async Task<ActionResult<ApiResponse<UserAddressSM>>> Create(
            [FromBody] ApiRequest<UserAddressSM> request)
        {
            var innerReq = request?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_ReqDataNotFormed, ApiErrorTypeSM.InvalidInputData_NoLog));
            }
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }
            var data = await _process.CreateAsync(userId, innerReq);
            return ModelConverter.FormNewSuccessResponse(data);
        }
        #endregion

        #region UPDATE
        [HttpPut("mine/{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "User")]
        public async Task<ActionResult<ApiResponse<UserAddressSM>>> Update(
            long id,
            [FromBody] ApiRequest<UserAddressSM> request)
        {
            var innerReq = request?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_ReqDataNotFormed, ApiErrorTypeSM.InvalidInputData_NoLog));
            }
            var userId = User.GetUserRecordIdFromCurrentUserClaims();

            var data = await _process.UpdateAsync(userId, id, innerReq);
            return ModelConverter.FormNewSuccessResponse(data);
        }
        #endregion

        #region DELETE
        [HttpDelete("mine/{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "User")]
        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> Delete(long id)
        {
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }

            var data = await _process.DeleteMineAsync(userId, id);
            return ModelConverter.FormNewSuccessResponse(data);
        }

        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> DeleteAsync(long id)
        {

            var data = await _process.DeleteAsync(id);
            return ModelConverter.FormNewSuccessResponse(data);
        }
        #endregion
    }
}
