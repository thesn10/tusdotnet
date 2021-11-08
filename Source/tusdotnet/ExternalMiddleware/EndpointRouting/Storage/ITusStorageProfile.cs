using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using tusdotnet.Interfaces;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public interface ITusStorageProfile
    {
        Task<ITusStore> GetStore(HttpContext context);
    }
}
