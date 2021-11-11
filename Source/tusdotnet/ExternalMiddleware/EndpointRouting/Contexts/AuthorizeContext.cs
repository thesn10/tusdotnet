#if endpointrouting

using System.Reflection;
using tusdotnet.Models;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public class AuthorizeContext
    {
        public IntentType IntentType { get; set; }
        public MethodInfo ControllerMethod { get; set; }
    }
}

#endif