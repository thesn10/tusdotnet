using System;
using System.Threading.Tasks;
using tusdotnet.Interfaces;

namespace tusdotnet.Storage
{
    internal class DefaultStoreConfigurator : ITusStoreConfigurator
    {
        private readonly Func<ITusStore> _configure;

        public DefaultStoreConfigurator(ITusStore store)
        {
            _configure = () => store;
        }

        public DefaultStoreConfigurator(Func<ITusStore> configure)
        {
            _configure = configure;
        }

        public Task<ITusStore> GetStoreAsync()
        {
            return Task.FromResult(_configure());
        }
    }
}
