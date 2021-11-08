#if endpointrouting

using System.Threading.Tasks;
using tusdotnet.Interfaces;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    /// <summary>
    /// Provides <see cref="TusStorageClient"/>s
    /// </summary>
    public interface ITusStorageClientProvider
    {
        /// <summary>
        /// Gets the default TusStorageClient which has been configured
        /// </summary>
        /// <returns></returns>
        Task<TusStorageClient> Default();

        /// <summary>
        /// Gets the tus storage client with the specifed profile
        /// </summary>
        /// <param name="name">The name of the profile</param>
        Task<TusStorageClient> Get(string name);

        /// <summary>
        /// Gets the tus storage client with the specifed profile or returns null
        /// </summary>
        /// <param name="name">The name of the profile</param>
        Task<TusStorageClient> GetOrNull(string name);

        /// <summary>
        /// Creates a new TusStorageClient using an <see cref="ITusStore"/>. 
        /// If you want to reuse configurated <see cref="ITusStore"/>s, 
        /// use <see cref="Get(string)"/>
        /// </summary>
        TusStorageClient Create(ITusStore store);
    }
}
#endif