using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using tusdotnet.Constants;
using tusdotnet.Models;
using tusdotnet.Routing;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    /// <summary>
    /// Provides helper methods for endpoint routing
    /// </summary>
    internal class TusEndpointRoutingHelper : ITusRoutingHelper
    {
        private readonly LinkGenerator _linkGenerator;
        private readonly LinkParser _linkParser;
        private readonly HttpContext _httpContext;

        public TusEndpointRoutingHelper(LinkGenerator linkGenerator, LinkParser linkParser, HttpContext httpContext)
        {
            _linkGenerator = linkGenerator;
            _linkParser = linkParser;
            _httpContext = httpContext;
        }

        /// <inheritdoc />
        public string? GenerateFilePath(string fileId)
        {
            var endpointName = GetEndpointName();
            var url = _linkGenerator.GetPathByName(_httpContext, endpointName, new RouteValueDictionary(new { fileId = fileId }));

            return url;
        }

        /// <inheritdoc />
        public string? GetFileId()
        {
            return _httpContext.GetRouteValue(RouteConstants.FileId) as string;
        }

        /// <inheritdoc />
        public string? ParseFileId(string url)
        {
            var endpointName = GetEndpointName();

            // TODO: this code is untested
            var routeValues = _linkParser.ParsePathByEndpointName(endpointName, url);

            if (routeValues != null && routeValues.TryGetValue(RouteConstants.FileId, out var fileId))
            {
                return fileId as string;
            }
            return null;
        }

        /// <inheritdoc />
        public bool IsMatchingRoute()
        {
            // we know it is always a matching route because the routing middleware already executed
            return true;
        }

        private string GetEndpointName()
        {
            var endpointNameMetadata = _httpContext.GetEndpoint().Metadata.GetMetadata<IEndpointNameMetadata>();
            if (endpointNameMetadata == null)
            {
                throw new TusConfigurationException($"The tus endpoint must have a name (to be able to parse and generate urls)");
            }
            return endpointNameMetadata.EndpointName;
        }
    }
}