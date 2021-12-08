#if endpointrouting

using System;

namespace tusdotnet.Controllers
{
    /// <summary>
    /// Indicates that the type and any derived types that this attribute is applied to
    /// are considered a tus controller by the default controller discovery mechanism.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TusControllerAttribute : Attribute
    {
    }
}
#endif