#if endpointrouting

using System;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    /// <summary>
    /// Indicates that the tus controller supports the specified extensions
    /// </summary>
    public class TusEnableExtensionAttribute : Attribute
    {
        public string[] ExtensionNames { get; set; }

        public TusEnableExtensionAttribute(params string[] extensionNames)
        {
            ExtensionNames = extensionNames;
        }
    }
}
#endif