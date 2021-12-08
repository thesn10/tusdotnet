using System;

namespace tusdotnet.Controllers
{
    /// <summary>
    /// The maximum upload size to allow. Exceeding this limit will return a "413 Request Entity Too Large" error to the client.
    /// Set to null to allow any size. The size might still be restricted by the web server or operating system.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class MaxUploadSizeAttribute : Attribute
    {
        /// <summary>
        /// The maximum upload size to allow. Exceeding this limit will return a "413 Request Entity Too Large" error to the client.
        /// Set to null to allow any size. The size might still be restricted by the web server or operating system.
        /// </summary>
        public MaxUploadSizeAttribute(long maxUploadSizeInBytes)
        {
            MaxUploadSizeInBytes = maxUploadSizeInBytes;
        }

        /// <summary>
        /// The maximum upload size
        /// </summary>
        public long MaxUploadSizeInBytes { get; }
    }
}
