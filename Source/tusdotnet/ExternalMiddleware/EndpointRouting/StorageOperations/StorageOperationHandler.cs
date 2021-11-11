using tusdotnet.Stores;

namespace tusdotnet.ExternalMiddleware.EndpointRouting.StorageOperations
{
    internal abstract class StorageOperationHandler
    {
        protected readonly StoreAdapter _storeAdapter;

        internal StorageOperationHandler(StoreAdapter storeAdapter)
        {
            _storeAdapter = storeAdapter;
        }
    }
}
