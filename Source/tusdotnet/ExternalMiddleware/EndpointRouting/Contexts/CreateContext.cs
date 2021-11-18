#if endpointrouting

using System;
using System.Collections.Generic;
using tusdotnet.Models;
using tusdotnet.Models.Concatenation;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public class CreateContext
    {
        public string UploadMetadata { get; set; }

        public IDictionary<string, Metadata> Metadata { get; set; }

        public long? UploadOffset { get; set; }

        public long UploadLength { get; set; }

        public FileConcat? FileConcat { get; set; }
    }
}

#endif