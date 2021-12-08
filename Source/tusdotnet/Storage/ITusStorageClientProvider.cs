using System.Threading.Tasks;

namespace tusdotnet.Storage
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
        Task<TusStorageClient?> GetOrNull(string name);
    }
}