using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using tusdotnet.Interfaces;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    internal class TusStorageProfile : ITusStorageProfile
    {
        private readonly Func<HttpContext, Task<ITusStore>> _storeFunc;

        public TusStorageProfile(ITusStore store) 
            : this((_) => store)
        {
        }

        public TusStorageProfile(Func<HttpContext, ITusStore> storeFunc) 
            : this((context) => Task.FromResult(storeFunc(context)))
        {
        }

        public TusStorageProfile(Func<HttpContext, Task<ITusStore>> storeFunc)
        {
            _storeFunc = storeFunc;
        }

        public Task<ITusStore> GetStore(HttpContext context)
        {
            return _storeFunc(context);
        }
    }
}
