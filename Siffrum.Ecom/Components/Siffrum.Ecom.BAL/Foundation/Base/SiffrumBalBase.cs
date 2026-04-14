using AutoMapper;
using Siffrum.Ecom.DAL.Context;
using Siffrum.Ecom.BAL.Foundation.Odata;

namespace Siffrum.Ecom.BAL.Foundation.Base
{
    public class SiffrumBalBase : BalRoot
    {
        protected readonly IMapper _mapper;
        protected readonly ApiDbContext _apiDbContext;

        public SiffrumBalBase(IMapper mapper, ApiDbContext apiDbContext)
        {
            _mapper = mapper;
            _apiDbContext = apiDbContext;
        }
    }
    public abstract class SiffrumBalOdataBase<T> : BalOdataRoot<T>
    {
        protected readonly IMapper _mapper;
        protected readonly ApiDbContext _apiDbContext;

        protected SiffrumBalOdataBase(IMapper mapper, ApiDbContext apiDbContext)
        {
            _mapper = mapper;
            _apiDbContext = apiDbContext;
        }
    }
}
