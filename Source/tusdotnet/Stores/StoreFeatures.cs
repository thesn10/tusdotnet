using tusdotnet.Interfaces;

namespace tusdotnet.Stores
{
    /// <summary>
    /// Supported features of a <see cref="ITusStore"/>
    /// </summary>
    public class StoreFeatures
    {
        /// <summary>
        /// Supports reading
        /// </summary>
        public bool Readable { get; set; }

        /// <summary>
        /// Supports System.IO.Pipelines
        /// </summary>
        public bool Pipelines { get; set; }
    }
}
