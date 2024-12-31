using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;
using tusdotnet.Interfaces;

namespace tusdotnet.Models.Configuration
{
    /// <summary>
    /// Base context for all events in tusdotnet
    /// </summary>
    /// <typeparam name="TSelf">The type of the derived class inheriting the EventContext</typeparam>
    public abstract class EventContext<TSelf> where TSelf : EventContext<TSelf>, new()
    {
        /// <summary>
        /// The id of the file that was completed
        /// </summary>
        public string FileId { get; set; }

        /// <summary>
        /// The store that was used when completing the upload
        /// </summary>
        public ITusStore Store { get; set; }

        /// <summary>
        /// The request's cancellation token
        /// </summary>
        public CancellationToken CancellationToken { get; set; }

        /// <summary>
        /// The http context for the current request
        /// </summary>
        public HttpContext HttpContext { get; private set; }

        /// <summary>
        /// Get the file with the id specified in the <see cref="FileId"/> property.
		/// Returns null if there is no file id or if the file was not found.
        /// </summary>
        /// <returns>The file or null</returns>
        public Task<ITusFile> GetFileAsync()
        {
            if (string.IsNullOrEmpty(FileId))
                return Task.FromResult<ITusFile>(null);

            return ((ITusReadableStore)Store).GetFileAsync(FileId, CancellationToken);
        }

        internal static TSelf Create(string fileId, HttpContext context, ITusStore store, CancellationToken cancellationToken, Action<TSelf> configure = null)
        {
            if (string.IsNullOrEmpty(fileId))
            {
                fileId = null;
            }

            var eventContext = new TSelf
            {
                Store = store,
                CancellationToken = cancellationToken,
                FileId = fileId,
                HttpContext = context,
            };

            configure?.Invoke(eventContext);

            return eventContext;
        }
    }
}
