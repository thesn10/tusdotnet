#if endpointrouting

using tusdotnet.Models.Concatenation;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public class TusFileInfoResult : IFileInfoResult
    {
        public TusFileInfoResult(GetFileInfoResult result)
            : this(result.UploadMetadata, result.UploadLength, result.UploadOffset, result.UploadConcat)
        {
        }

        public TusFileInfoResult(string uploadMetadata, long? uploadLength, long uploadOffset, FileConcat? uploadConcat = null)
        {
            UploadMetadata = uploadMetadata;
            UploadLength = uploadLength;
            UploadOffset = uploadOffset;
            UploadConcat = uploadConcat;
        }

        public string UploadMetadata { get; set; }
        public long? UploadLength { get; set; }
        public long? UploadOffset { get; set; }
        public FileConcat? UploadConcat { get; set; }
    }
}
#endif
