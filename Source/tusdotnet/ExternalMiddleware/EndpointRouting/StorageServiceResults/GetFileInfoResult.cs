#if endpointrouting

using tusdotnet.Models.Concatenation;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public class GetFileInfoResult
    {
        public string UploadMetadata { get; set; }
        public long? UploadLength { get; set; }
        public long UploadOffset { get; set; }
        public FileConcat UploadConcat { get; set; }
    }
}
#endif