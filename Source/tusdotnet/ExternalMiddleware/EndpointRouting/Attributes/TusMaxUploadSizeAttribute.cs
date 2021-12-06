using System;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    /// <summary>
    /// The maximum upload size to allow. Exceeding this limit will return a "413 Request Entity Too Large" error to the client.
    /// Set to null to allow any size. The size might still be restricted by the web server or operating system.
    /// </summary>
    public class TusMaxUploadSizeAttribute : Attribute
    {
        /// <summary>
        /// The maximum upload size to allow. Exceeding this limit will return a "413 Request Entity Too Large" error to the client.
        /// Set to null to allow any size. The size might still be restricted by the web server or operating system.
        /// </summary>
        public TusMaxUploadSizeAttribute(long maxUploadSizeInBytes)
        {
            MaxUploadSizeInBytes = maxUploadSizeInBytes;
        }

        /// <summary>
        /// The maximum upload size
        /// </summary>
        public long MaxUploadSizeInBytes { get; }
    }
}
