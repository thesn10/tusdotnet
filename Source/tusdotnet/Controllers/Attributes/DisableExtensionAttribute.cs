using System;

namespace tusdotnet.Controllers
{
    /// <summary>
    /// Indicates that the tus controller does not support the specified extensions
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class DisableExtensionAttribute : Attribute
    {
        /// <summary>
        /// Names of the extensions to disable
        /// </summary>
        public string[] ExtensionNames { get; set; }

        /// <summary>
        /// Indicates that the tus controller does not support the specified extensions
        /// </summary>
        /// <param name="extensionNames">Names of the extensions to disable</param>
        public DisableExtensionAttribute(params string[] extensionNames)
        {
            ExtensionNames = extensionNames;
        }
    }
}
