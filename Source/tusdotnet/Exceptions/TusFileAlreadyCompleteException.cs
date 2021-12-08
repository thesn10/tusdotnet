namespace tusdotnet.Exceptions
{
    /// <summary>
    /// Exception thrown if there is an attempt to write to a file but the file is already complete
    /// </summary>
    public class TusFileAlreadyCompleteException : TusException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TusFileAlreadyCompleteException"/> class.
        /// </summary>
        public TusFileAlreadyCompleteException() : base("Upload is already complete.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TusFileAlreadyCompleteException"/> class.
        /// </summary>
        public TusFileAlreadyCompleteException(string message) : base(message)
        {
        }
    }
}