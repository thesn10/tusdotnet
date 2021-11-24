#if endpointrouting

using System;
using tusdotnet.Models.Concatenation;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public class WriteResult
    {
        public bool IsComplete { get; set; }
        public long UploadOffset { get; set; }
        public bool ClientDisconnectedDuringRead { get; set; }

        // Expiration Extension
        public DateTimeOffset? FileExpires { get; set; }

        // Concatenation Extension
        public FileConcat? FileConcat { get; set; }
    }
}
#endif
