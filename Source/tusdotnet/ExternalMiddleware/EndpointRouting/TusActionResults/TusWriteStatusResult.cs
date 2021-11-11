#if endpointrouting

using System;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public class TusWriteStatusResult : IWriteResult
    {
        public TusWriteStatusResult(WriteResult writeResult)
            : this(writeResult.IsComplete, writeResult.UploadOffset, writeResult.ClientDisconnectedDuringRead, 
                  writeResult.ChecksumMatches, writeResult.FileExpires)
        {
        }

        public TusWriteStatusResult(bool isComplete, long uploadOffset, bool clientDisconnectedDuringRead, bool? checksumMatches = null, DateTimeOffset? fileExpires = null)
        {
            IsComplete = isComplete;
            UploadOffset = uploadOffset;
            ClientDisconnectedDuringRead = clientDisconnectedDuringRead;
            ChecksumMatches = checksumMatches;
            FileExpires = fileExpires;
        }

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
