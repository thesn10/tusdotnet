using System;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    /// <summary>
    /// Indicates that the tus controller does not support the specified extensions
    /// </summary>
    public class TusDisableExtensionAttribute : Attribute
    {
        public string[] ExtensionNames { get; set; }

        public TusDisableExtensionAttribute(params string[] extensionNames)
        {
            ExtensionNames = extensionNames;
        }
    }
}
