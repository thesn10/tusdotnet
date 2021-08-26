#if endpointrouting

using System;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    /// <summary>
    /// Indicates that the type and any derived types that this attribute is applied to
    /// are considered a tus controller by the default controller discovery mechanism.
    /// </summary>
    public class TusControllerAttribute : Attribute
    {
    }
}
#endif