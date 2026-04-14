using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Siffrum.Ecom.BAL.ExceptionHandler;
using Siffrum.Ecom.BAL.Foundation.Base;
using Siffrum.Ecom.DAL.Context;
using Siffrum.Ecom.DomainModels.Enums;
using Siffrum.Ecom.DomainModels.v1;
using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base.CommonResponseRoot;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Interfaces;
using Siffrum.Ecom.ServiceModels.v1;

namespace Siffrum.Ecom.BAL.Marketing
{
    public class FaqProcess : SiffrumBalOdataBase<FaqSM>
    {
        public FaqProcess(
            IMapper mapper,
            ApiDbContext apiDbContext)
            : base(mapper, apiDbContext)
        {
        }

        #region ODATA
        public override async Task<IQueryable<FaqSM>> GetServiceModelEntitiesForOdata()
        {
            var query = _apiDbContext.Faq.AsNoTracking();

            return await base.MapEntityAsToQuerable<FaqDM, FaqSM>(_mapper, query);
        }
        #endregion

        #region CREATE

        public async Task<BoolResponseRoot> CreateAsync(FaqSM objSM)
        {
            if (objSM == null)
                return new BoolResponseRoot(false, "FAQ data is required");

            if (string.IsNullOrWhiteSpace(objSM.Question))
                return new BoolResponseRoot(false, "Question is required");

            if (string.IsNullOrWhiteSpace(objSM.Answer))
                return new BoolResponseRoot(false, "Answer is required");

            var dm = _mapper.Map<FaqDM>(objSM);

            dm.CreatedAt = DateTime.UtcNow;

            await _apiDbContext.Faq.AddAsync(dm);

            var result = await _apiDbContext.SaveChangesAsync();

            if (result > 0)
            {
                return new BoolResponseRoot(true, "FAQ created successfully");
            }

            return new BoolResponseRoot(false, "Failed to create FAQ");
        }

        #endregion

        #region READ

        public async Task<FaqSM?> GetByIdAsync(long id)
        {
            var dm = await _apiDbContext.Faq
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            return dm != null ? _mapper.Map<FaqSM>(dm) : null;
        }

        public async Task<List<FaqSM>> GetAll(int skip, int top)
        {
            var list = await _apiDbContext.Faq
                .AsNoTracking()
                .OrderBy(x => x.Module)
                .Skip(skip)
                .Take(top)
                .ToListAsync();

            return _mapper.Map<List<FaqSM>>(list);
        }
        public async Task<IntResponseRoot> GetAllCount()
        {
            var count = await _apiDbContext.Faq
                .AsNoTracking()
                .CountAsync();

            return new IntResponseRoot(count, "Total FAQs");
        }

        public async Task<List<FaqSM>> GetByModule(FaqModuleSM module, int skip, int top)
        {
            var dmModule = (FaqModuleDM)module;

            var list = await _apiDbContext.Faq
                .AsNoTracking()
                .Where(x => x.Module == dmModule)
                .OrderBy(x => x.Id)
                .Skip(skip).Take(top)
                .ToListAsync();

            return _mapper.Map<List<FaqSM>>(list);
        }
        
        public async Task<IntResponseRoot> GetByModuleCount(FaqModuleSM module)
        {
            var dmModule = (FaqModuleDM)module;

            var count = await _apiDbContext.Faq
                .AsNoTracking()
                .Where(x => x.Module == dmModule)
                .OrderBy(x => x.Id)
                .CountAsync();

            return new IntResponseRoot(count, "Total FAQs");
        }


        #endregion

        #region UPDATE

        public async Task<FaqSM?> UpdateAsync(long id, FaqSM objSM)
        {
            if (objSM == null)
                throw new SiffrumException(ApiErrorTypeSM.ModelError_NoLog, "FAQ data is required");

            var dm = await _apiDbContext.Faq
                .FirstOrDefaultAsync(x => x.Id == id);

            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "FAQ not found");

            dm.Question = objSM.Question;
            dm.Answer = objSM.Answer;
            dm.Module = (FaqModuleDM)objSM.Module;
            dm.UpdatedAt = DateTime.UtcNow;

            await _apiDbContext.SaveChangesAsync();

            return _mapper.Map<FaqSM>(dm);
        }

        #endregion

        #region DELETE

        public async Task<DeleteResponseRoot> DeleteAsync(long id)
        {
            var dm = await _apiDbContext.Faq
                .FirstOrDefaultAsync(x => x.Id == id);

            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, "FAQ not found");

            _apiDbContext.Faq.Remove(dm);

            await _apiDbContext.SaveChangesAsync();

            return new DeleteResponseRoot(true, "FAQ deleted successfully");
        }

        #endregion
    }

}
