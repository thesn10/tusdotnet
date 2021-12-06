using Microsoft.AspNetCore.Http;

namespace tusdotnet.Routing
{
    /* introduced to avoid using IHttpContextAccessor */

    /// <summary>
    /// Creates an <see cref="ITusRoutingHelper"/> for each request
    /// </summary>
    public interface ITusRoutingHelperFactory
    {
        /// <summary>
        /// Get the <see cref="ITusRoutingHelper"/> for the current request
        /// </summary>
        public ITusRoutingHelper Get(HttpContext context);
    }
}
