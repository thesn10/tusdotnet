#if endpointrouting

using System;
using System.IO;
using System.Threading.Tasks;
using tusdotnet.Models;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public class WriteContext
    {
        public string FileId { get; set; }

        public long UploadOffset { get; set; }

        public long? UploadLength { get; set; }

        public Func<Task<Checksum>> GetChecksumProvidedByClient { get; set; }

        public Stream RequestStream { get; set; }
#if pipelines
        public System.IO.Pipelines.PipeReader RequestReader { get; set; }
#endif
    }
}

#endif