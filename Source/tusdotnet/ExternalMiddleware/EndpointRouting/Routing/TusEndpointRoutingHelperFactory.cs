#if endpointrouting

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using tusdotnet.Routing;

namespace tusdotnet.ExternalMiddleware.EndpointRouting.Routing
{
    /// <inheritdoc />
    internal class TusEndpointRoutingHelperFactory : ITusRoutingHelperFactory
    {
        private readonly LinkGenerator _linkGenerator;
        private readonly LinkParser _linkParser;

        public TusEndpointRoutingHelperFactory(LinkGenerator linkGenerator, LinkParser linkParser)
        {
            _linkGenerator = linkGenerator;
            _linkParser = linkParser;
        }

        /// <inheritdoc />
        public ITusRoutingHelper Get(HttpContext context)
        {
            return new TusEndpointRoutingHelper(_linkGenerator, _linkParser, context);
        }
    }
}

#endif