using tusdotnet.Stores;

namespace tusdotnet.Routing
{
    /// <summary>
    /// Context for supported extensions and features
    /// </summary>
    public class FeatureSupportContext
    {
        /// <summary>
        /// The supported extensions
        /// </summary>
        public StoreExtensions SupportedExtensions { get; set; }

        /// <summary>
        /// The supported features
        /// </summary>
        public StoreFeatures SupportedFeatures { get; set; }

        /// <summary>
        /// The supported checksum algorithms
        /// </summary>
        public StoreChecksumAlgorithms SupportedChecksumAlgorithms { get; set; }

        /// <summary>
        /// Create a new <see cref="FeatureSupportContext"/> from a <see cref="StoreAdapter"/>
        /// </summary>
        public static FeatureSupportContext FromStoreAdapter(StoreAdapter storeAdapter)
        {
            return new FeatureSupportContext()
            {
                SupportedExtensions = storeAdapter.Extensions,
                SupportedFeatures = storeAdapter.Features,
                SupportedChecksumAlgorithms = storeAdapter.ChecksumAlgorithms,
            };
        }
    }
}
