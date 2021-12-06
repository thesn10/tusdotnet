using Microsoft.AspNetCore.Http;
namespace tusdotnet.Routing
{
    internal class TusUrlPathRoutingHelperFactory : ITusRoutingHelperFactory
    {
        private readonly string _urlPath;

        public TusUrlPathRoutingHelperFactory(string urlPath)
        {
            _urlPath = urlPath;
        }

        public ITusRoutingHelper Get(HttpContext context)
        {
            return new TusUrlPathRoutingHelper(_urlPath, context);
        }
    }
}
