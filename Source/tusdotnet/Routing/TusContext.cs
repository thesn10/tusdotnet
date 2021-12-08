using Microsoft.AspNetCore.Http;

namespace tusdotnet.Routing
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
        /// The supported features/extensions of the request
        /// </summary>
        public FeatureSupportContext FeatureSupportContext { get; set; }

        /// <summary>
        /// The request endpoint options
        /// </summary>
        public ITusEndpointOptions EndpointOptions { get; set; }

        /// <summary>
        /// The routing helper of the request
        /// </summary>
        public ITusRoutingHelper RoutingHelper { get; set; }
    }
}