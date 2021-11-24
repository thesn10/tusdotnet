using Microsoft.AspNetCore.Http;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public class TusContext
    {
        public HttpContext HttpContext { get; set; }

        public TusExtensionInfo ExtensionInfo { get; set; }

        public ITusEndpointOptions Options { get; set; }

        public string UrlPath { get; set; }
    }
}