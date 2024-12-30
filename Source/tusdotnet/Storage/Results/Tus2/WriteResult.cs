namespace tusdotnet.Storage.Results.Tus2
{
    public class WriteResult
    {
        public long? UploadOffset { get; set; }
        public bool UploadIncomplete { get; set; }
        public bool DisconnectClient { get; set; }
    }
}
