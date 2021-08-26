#if endpointrouting

using System;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public class WriteResult
    {
        public bool IsComplete { get; set; }
        public long UploadOffset { get; internal set; }
        public bool ClientDisconnectedDuringRead { get; internal set; }
        
        // Checksum Extension
        public bool? ChecksumMatches { get; set; }

        // Expiration Extension
        public DateTimeOffset? FileExpires { get; internal set; }
    }
}
#endif
