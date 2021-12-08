using tusdotnet.Stores;

namespace tusdotnet.Storage.Handlers
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
