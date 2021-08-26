#if endpointrouting

using System;
using System.IO;
using tusdotnet.Models;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public class WriteContext
    {
        public string FileId { get; internal set; }

        public long? UploadOffset { get; internal set; }
        public long? UploadLength { get; internal set; }

        public Func<Checksum> GetChecksumProvidedByClient { get; internal set; }

        public Stream RequestStream { get; internal set; }

        public bool IsPartialFile { get; set; }
    }
}

#endif