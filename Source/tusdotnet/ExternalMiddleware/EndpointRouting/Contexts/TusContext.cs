using Microsoft.AspNetCore.Http;
using tusdotnet.Routing;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    /// <summary>
    /// Context for a tus request
    /// </summary>
    public class TusContext
    {
        /// <summary>
        /// The <see cref="HttpContext"/> of the request
        /// </summary>
        public HttpContext HttpContext { get; set; }

        /// <summary>
        /// Information about supported extensions
        /// </summary>
        public TusExtensionInfo ExtensionInfo { get; set; }

        /// <summary>
        /// Request endpoint options
        /// </summary>
        public ITusEndpointOptions Options { get; set; }

        internal ITusRoutingHelper RoutingHelper { get; set; }
    }
}