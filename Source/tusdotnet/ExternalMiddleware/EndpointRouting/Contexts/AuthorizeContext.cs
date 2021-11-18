using System.Reflection;
using tusdotnet.ExternalMiddleware.EndpointRouting.RequestHandlers;
using tusdotnet.Models;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public class AuthorizeContext
    {
        public IntentType IntentType { get; set; }
        public MethodInfo ControllerMethod { get; set; }
        internal RequestHandler RequestHandler { get; set; }
    }
}