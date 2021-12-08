using tusdotnet.Models.Concatenation;

namespace tusdotnet.Storage
{
    /// <summary>
    /// Result of an GetFileInfo operation
    /// </summary>
    public class GetFileInfoResult
    {
        /// <summary>
        /// The current Metadata of the file
        /// </summary>
        public string UploadMetadata { get; set; }

        /// <summary>
        /// The size of the file
        /// </summary>
        public long? UploadLength { get; set; }

        /// <summary>
        /// Current offset of the file
        /// </summary>
        public long UploadOffset { get; set; }

        /// <summary>
        /// File concatenation information otherwise null
        /// </summary>
        public FileConcat? FileConcatenation { get; set; }
    }
}