#if endpointrouting

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public static class EndpointBuilderExtensions
    {
        public static IEndpointRouteBuilder MapTus(this IEndpointRouteBuilder endpoints, Action<ITusEndpointBuilder> builder)
        {
            var tusEndpointBuilder = new TusEndpointBuilder(endpoints);
            builder(tusEndpointBuilder);

            return endpoints;
        }
    }
}

#endif