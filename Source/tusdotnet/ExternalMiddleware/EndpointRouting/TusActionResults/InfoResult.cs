#if endpointrouting

using tusdotnet.Models.Concatenation;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public interface IInfoResult
    {

    }

    public class TusInfoOk : IInfoResult
    {
        public TusInfoOk(GetFileInfoResult result)
            : this(result.UploadMetadata, result.UploadLength, result.UploadOffset, result.UploadConcat)
        {
        }

        public TusInfoOk(string uploadMetadata, long? uploadLength, long uploadOffset, FileConcat? uploadConcat = null)
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
