using System;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public class TusMaxUploadSizeAttribute : Attribute
    {
        public TusMaxUploadSizeAttribute(long maxUploadSizeInBytes)
        {
            MaxUploadSizeInBytes = maxUploadSizeInBytes;
        }

        public long MaxUploadSizeInBytes { get; }
    }
}
