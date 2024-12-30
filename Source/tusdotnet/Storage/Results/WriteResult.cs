using System;
using tusdotnet.Models.Concatenation;

namespace tusdotnet.Storage
{
    /// <summary>
    /// Result of an write file operation
    /// </summary>
    public class WriteResult
    {
        /// <summary>
        /// True if the file upload is complete
        /// </summary>
        public bool IsComplete { get; set; }

        /// <summary>
        /// Current upload offset of the file
        /// </summary>
        public long UploadOffset { get; set; }

        /// <summary>
        /// The expiration of the file (if supported)
        /// </summary>
        public DateTimeOffset? FileExpires { get; set; }

        /// <summary>
        /// File concatenation information otherwise null
        /// </summary>
        public FileConcat? FileConcatenation { get; set; }
    }
}
