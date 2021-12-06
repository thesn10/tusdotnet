using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using tusdotnet.Interfaces;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    /// <summary>
    /// Profile to configure a <see cref="ITusStore"/> for each request
    /// </summary>
    public interface ITusStoreConfigurator
    {
        /// <summary>
        /// Configure and return a <see cref="ITusStore"/>
        /// </summary>
        Task<ITusStore> GetStoreAsync();
    }
}
