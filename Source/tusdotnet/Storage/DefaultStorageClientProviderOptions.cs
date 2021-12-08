using System;
using System.Collections.Generic;

namespace tusdotnet.Storage
{
    /// <summary>
    /// Options to configure the <see cref="ITusStorageClientProvider"/>
    /// </summary>
    public class DefaultStorageClientProviderOptions
    {
        /// <summary>
        /// The name of the default profile
        /// </summary>
        public string? DefaultName { get; set; } = null;

        /// <summary>
        /// Dictionary of all storage profiles (name => storage profile type)
        /// </summary>
        public Dictionary<string, Type> Profiles { get; set; } = new Dictionary<string, Type>();
    }
}
