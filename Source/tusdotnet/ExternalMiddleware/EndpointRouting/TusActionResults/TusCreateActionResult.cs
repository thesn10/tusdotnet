#if endpointrouting
using System;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public interface ITusCreateActionResult
    {
    }

    public class TusCreateOk : ITusCreateActionResult
    {
        public TusCreateOk(CreateResult result)
            : this(result.FileId, result.FileExpires)
        {
        }

        public TusCreateOk(string fileId, DateTimeOffset? expires = null)
        {
            FileId = fileId;
            Expires = expires;
        }

        public string FileId { get; set; }
        public DateTimeOffset? Expires { get; set; }
    }
}

#endif