using System;
using System.IO;
using System.Threading.Tasks;
using tusdotnet.Models;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    /// <summary>
    /// Context for the Write action
    /// </summary>
    public class WriteContext
    {
        /// <summary>
        /// The file id of the request
        /// </summary>
        public string FileId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public long UploadOffset { get; set; }

        /// <summary>
        /// The length (in bytes) of the file. 
        /// Will only have a value, if Upload-Defer-Length is used
        /// else the Upload length is supplied at the create request.
        /// </summary>
        public long? UploadLength { get; set; }

        /// <summary>
        /// Callback to support checksum headers and trailers
        /// </summary>
        public Func<Task<Checksum>> GetChecksumProvidedByClient { get; set; }

        /// <summary>
        /// Indicates if a write operation is initiated as an addition to the create request
        /// </summary>
        public bool IsCreationWithUpload { get; set; }

        /// <summary>
        /// The request body stream that contains the file data
        /// </summary>
        public Stream RequestStream { get; set; }

#if pipelines
        /// <summary>
        /// The request body pipe reader that contains the file data
        /// </summary>
        public System.IO.Pipelines.PipeReader RequestReader { get; set; }
#endif
    }
}