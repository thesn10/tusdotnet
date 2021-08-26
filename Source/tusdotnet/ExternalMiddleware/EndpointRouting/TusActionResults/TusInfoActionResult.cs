#if endpointrouting

using tusdotnet.Models.Concatenation;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public interface ITusInfoActionResult
    {

    }

    public class TusInfoOk : ITusInfoActionResult
    {
        public TusInfoOk(GetFileInfoResult result)
            : this(result.UploadMetadata, result.UploadLength, result.UploadOffset, result.UploadConcat)
        {
        }

        public TusInfoOk(string uploadMetadata, long? uploadLength, long uploadOffset, bool uploadDeferLength = false)
            : this(uploadMetadata, uploadLength, uploadOffset, null)
        {
        }

        public TusInfoOk(string uploadMetadata, long? uploadLength, long uploadOffset, FileConcat? uploadConcat, bool uploadDeferLength = false)
        {
            UploadMetadata = uploadMetadata;
            UploadLength = uploadLength;
            UploadOffset = uploadOffset;
            UploadConcat = UploadConcat;
        }

        public string UploadMetadata { get; set; }
        public long? UploadLength { get; set; }
        public long? UploadOffset { get; set; }
        public FileConcat? UploadConcat { get; set; }
        public bool UploadDeferLength { get; set; }
    }
}
#endif
