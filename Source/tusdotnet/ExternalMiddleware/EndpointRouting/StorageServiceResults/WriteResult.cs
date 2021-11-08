#if endpointrouting

using System;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public class WriteResult
    {
        public bool IsComplete { get; set; }
        public long UploadOffset { get; set; }
        public bool ClientDisconnectedDuringRead { get; set; }
        
        // Checksum Extension
        public bool? ChecksumMatches { get; set; }

        // Expiration Extension
        public DateTimeOffset? FileExpires { get; set; }
    }
}
#endif
