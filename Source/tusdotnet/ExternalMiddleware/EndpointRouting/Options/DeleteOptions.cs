using System;
using System.Threading.Tasks;
using tusdotnet.FileLocks;
using tusdotnet.Interfaces;
using tusdotnet.Models;
using tusdotnet.Models.Expiration;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    /// <summary>
    /// Options for a delete operation
    /// </summary>
    public class DeleteOptions
    {
        /// <summary>
        /// Lock provider to use when locking to prevent files from being accessed while the file is still in use.
        /// Defaults to using in-memory locks.
        /// </summary>
        public ITusFileLockProvider FileLockProvider { get; set; } = InMemoryFileLockProvider.Instance;
    }
}
