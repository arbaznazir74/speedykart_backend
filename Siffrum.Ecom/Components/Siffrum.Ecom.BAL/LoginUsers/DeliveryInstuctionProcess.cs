using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Siffrum.Ecom.BAL.Base.Email;
using Siffrum.Ecom.BAL.Base.ImageProcess;
using Siffrum.Ecom.BAL.Base.Location;
using Siffrum.Ecom.BAL.ExceptionHandler;
using Siffrum.Ecom.BAL.Foundation.Base;
using Siffrum.Ecom.DAL.Context;
using Siffrum.Ecom.DomainModels.Enums;
using Siffrum.Ecom.DomainModels.v1;
using Siffrum.Ecom.ServiceModels.AppUser;
using Siffrum.Ecom.ServiceModels.AppUser.Login;
using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base.CommonResponseRoot;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Interfaces;
using Siffrum.Ecom.ServiceModels.Foundation.Token;
using Siffrum.Ecom.ServiceModels.v1;
using Siffrum.Ecom.ServiceModels.v1.General;
using System.Text;

namespace Siffrum.Ecom.BAL.LoginUsers
{
    public class DeliveryInstuctionProcess : SiffrumBalOdataBase<DeliveryInstructionsSM>
    {
        private readonly ILoginUserDetail _loginUserDetail;
        private readonly ImageProcess _imageProcess;
        private readonly GeoLocationService _geoService;

        public DeliveryInstuctionProcess(IMapper mapper,ApiDbContext apiDbContext,ImageProcess imageProcess, 
            ILoginUserDetail loginUserDetail)
            : base(mapper, apiDbContext)
        {
            _loginUserDetail = loginUserDetail;
            _imageProcess = imageProcess;
        }

        #region OData
        public override async Task<IQueryable<DeliveryInstructionsSM>> GetServiceModelEntitiesForOdata()
        {
            IQueryable<DeliveryInstructionsDM> entitySet = _apiDbContext.DeliveryInstructions.AsNoTracking();
            return await base.MapEntityAsToQuerable<DeliveryInstructionsDM, DeliveryInstructionsSM>(_mapper, entitySet);
        }
        #endregion       

        #region CREATE

        public async Task<DeliveryInstructionsSM> CreateAsync(long userId, DeliveryInstructionsSM objSM)
        {
            if (objSM == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Please provide delivery instruction details.");
            }
            var existingDeliveryInstruction = await _apiDbContext.DeliveryInstructions.Where(x=>x.UserId == userId).FirstOrDefaultAsync();
            if (existingDeliveryInstruction != null)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog,"Delivery instruction already exists, update the instructions instead");
            }
            var userExists = await _apiDbContext.User
        .AnyAsync(x => x.Id == userId);

            if (!userExists)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog,
                    "User does not exist.");
            }


            var dm = _mapper.Map<DeliveryInstructionsDM>(objSM);
            if (!string.IsNullOrEmpty(objSM.AudioBase64))
            {
                var audioPath = await _imageProcess.SaveFromBase64(objSM.AudioBase64, "mp3", @"wwwroot/content/audio");
                if (!string.IsNullOrEmpty(audioPath))
                {
                    dm.AudioPath = audioPath;
                }
                else
                {
                    dm.AudioPath = null;
                }
            }
            dm.CreatedAt = DateTime.UtcNow;
            dm.UserId = userId;
            dm.CreatedBy = _loginUserDetail.LoginId;

            await _apiDbContext.DeliveryInstructions.AddAsync(dm);

            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                return await GetByIdAsync(dm.Id);
            }

            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log,
                "Error while saving delivery instructions.");
        }

        #endregion


        #region READ

        public async Task<List<DeliveryAvailabiltySM>> GetAllDeliveryInstructions(int skip, int top)
        {
            var dms = await _apiDbContext.DeliveryInstructions
                .AsNoTracking()
                .OrderBy(x=>x.UserId)
                .Skip(skip)
                .Take(top)
                .ToListAsync();
            return _mapper.Map<List<DeliveryAvailabiltySM>>(dms);
        }

        public async Task<IntResponseRoot> GetAllCount()
        {
            
            var count = await _apiDbContext.DeliveryInstructions.AsNoTracking().CountAsync();

            return new IntResponseRoot(count, "Total Delivery instuctions");
        }

        public async Task<DeliveryInstructionsSM?> GetByIdAsync(long id)
        {
            var dm = await _apiDbContext.DeliveryInstructions
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (dm == null)
                return null;
            var sm = _mapper.Map<DeliveryInstructionsSM>(dm);
            if (!string.IsNullOrEmpty(dm.AudioPath))
            {
                var aImg = await _imageProcess.ResolveImage(dm.AudioPath);
                sm.AudioBase64 = aImg.Base64;
                sm.NetworkAudio = aImg.NetworkUrl;
            }

            return sm;
        }


        public async Task<DeliveryInstructionsSM> GetByUserId(long userId)
        {
            var dm = await _apiDbContext.DeliveryInstructions
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .FirstOrDefaultAsync();
            if(dm == null)
            {
                return null;
            }
            return await GetByIdAsync(dm.Id);
        }

        #endregion


        #region UPDATE

        public async Task<DeliveryInstructionsSM?> UpdateAsync(long id, DeliveryInstructionsSM objSM)
        {
            if (objSM == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Please provide details to update.");
            }

            var dm = await _apiDbContext.DeliveryInstructions
                .FirstOrDefaultAsync(x => x.Id == id);

            if (dm == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log,
                    "Delivery instruction not found.");
            }
            string previousAudioFile = null;
            
            objSM.UserId = dm.UserId;
            objSM.Id = dm.Id;
            _mapper.Map(objSM, dm);
            if (!string.IsNullOrEmpty(objSM.AudioBase64))
            {
                previousAudioFile = dm.AudioPath;
                var audioFile = await _imageProcess.SaveFromBase64(objSM.AudioBase64, "mp3", @"wwwroot/content/audio");
                if (!string.IsNullOrEmpty(audioFile))
                {
                    dm.AudioPath = audioFile;
                }
                else
                {
                    dm.AudioPath = null;
                }
            }
            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;

            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                if (File.Exists(previousAudioFile)) File.Delete(previousAudioFile);
                return await GetByIdAsync(id);
            }

            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log,
                "Error while updating delivery instructions.");
        }

        public async Task<DeliveryInstructionsSM?> UpdateMine(long userId, DeliveryInstructionsSM objSM)
        {
            if (objSM == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Please provide details to update.");
            }

            var dm = await _apiDbContext.DeliveryInstructions
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (dm == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log,
                    "Delivery instruction not found.");
            }
            string previousAudioFile = null;
            objSM.Id = dm.Id;
            objSM.UserId = dm.UserId;
            _mapper.Map(objSM, dm);
            if (!string.IsNullOrEmpty(objSM.AudioBase64))
            {
                previousAudioFile = dm.AudioPath;
                var audioFile = await _imageProcess.SaveFromBase64(objSM.AudioBase64, "mp3", @"wwwroot/content/audio");
                if (!string.IsNullOrEmpty(audioFile))
                {
                    dm.AudioPath = audioFile;
                }
                else
                {
                    dm.AudioPath = null;
                }
            }
            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;

            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                if (File.Exists(previousAudioFile)) File.Delete(previousAudioFile);
                return await GetByIdAsync(dm.Id);
            }

            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log,
                "Error while updating delivery instructions.");
        }

        #endregion


        #region DELETE (SOFT DELETE)

        public async Task<DeleteResponseRoot> DeleteAsync(long id)
        {
            var dm = await _apiDbContext.DeliveryInstructions
                .FirstOrDefaultAsync(x => x.Id == id);

            if (dm == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log,
                    "Delivery instruction not found.");
            }
            string previousAudioFile = null;
            if (!string.IsNullOrEmpty(dm.AudioPath))
            {
                previousAudioFile = dm.AudioPath;
            }
            _apiDbContext.DeliveryInstructions.Remove(dm);

            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                if (File.Exists(previousAudioFile)) File.Delete(previousAudioFile);
                return new DeleteResponseRoot(true, "Delivery instruction deleted successfully.");
            }

            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log,
                "Error while deleting delivery instruction.");
        }

        public async Task<DeleteResponseRoot> DeleteMine(long userId)
        {
            var dm = await _apiDbContext.DeliveryInstructions
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (dm == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log,
                    "Delivery instruction not found.");
            }
            string previousAudioFile = null;
            if (!string.IsNullOrEmpty(dm.AudioPath))
            {
                previousAudioFile = dm.AudioPath;
            }
            _apiDbContext.DeliveryInstructions.Remove(dm);

            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                if (File.Exists(previousAudioFile)) File.Delete(previousAudioFile);
                return new DeleteResponseRoot(true, "Delivery instruction deleted successfully.");
            }

            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log,
                "Error while deleting delivery instruction.");
        }

        #endregion

    }
}

