using SFManagement.Data;
using SFManagement.Models;

namespace SFManagement.Services
{
    public class OfxService : BaseService<Ofx>
    {
        public OfxService(DataContext context) : base(context)
        {
        }

        public async Task<Ofx> Add(IFormFile formFile)
        {
            await Task.Yield();
            //TODO: FAZER CONVERSÂO DO IFORMFILE EM ENTIDADE DO BANCO.

            throw new NotImplementedException();
        }
    }
}
