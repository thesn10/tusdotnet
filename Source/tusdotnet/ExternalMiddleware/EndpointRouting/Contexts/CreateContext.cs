using System.Collections.Generic;
using tusdotnet.Models;
using tusdotnet.Models.Concatenation;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    /// <summary>
    /// Context for the Create action
    /// </summary>
    public class CreateContext
    {
        /// <summary>
        /// Raw Upload-Metadata Header. To use parsed Metadata values, use <see cref="Metadata"/>
        /// </summary>
        public string UploadMetadata { get; set; }

        /// <summary>
        /// The metadata for the file.
        /// </summary>
        public IDictionary<string, Metadata> Metadata { get; set; }

        /// <summary>
        /// The length (in bytes) of the file to be created. Will be -1 if Upload-Defer-Length is used.
        /// </summary>
        public long UploadLength { get; set; }

        /// <summary>
        /// True if Upload-Defer-Length is used in the request, otherwise false.
        /// </summary>
        public bool UploadLengthIsDeferred => UploadLength == -1;

        /// <summary>
        /// File concatenation information if the concatenation extension is used in the request,
        /// otherwise null.
        /// </summary>
        public FileConcat? FileConcatenation { get; set; }
    }
}