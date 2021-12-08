using System;

namespace tusdotnet.Storage
{
    /// <summary>
    /// Result of an create file operation
    /// </summary>
    public class CreateResult
    {
        /// <summary>
        /// The generated file id
        /// </summary>
        public string FileId { get; set; }

        /// <summary>
        /// The expiration of the file (if supported)
        /// </summary>
        public DateTimeOffset? FileExpires { get; internal set; }
    }
}
