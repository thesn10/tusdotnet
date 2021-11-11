#if endpointrouting

using System;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public class TusCreateStatusResult : ICreateResult
    {
        public TusCreateStatusResult(CreateResult result)
            : this(result.FileId, result.FileExpires)
        {
        }

        public TusCreateStatusResult(string fileId, DateTimeOffset? expires = null)
        {
            FileId = fileId;
            Expires = expires;
        }

        public string FileId { get; set; }
        public DateTimeOffset? Expires { get; set; }
    }
}

#endif