using System;
using System.Collections.Generic;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    /// <summary>
    /// Options to configure the <see cref="ITusStorageClientProvider"/>
    /// </summary>
    public class TusStorageClientConfiguratorOptions
    {
        /// <summary>
        /// The name of the default profile
        /// </summary>
        public ITusStoreConfigurator Configurator { get; set; }
    }
}
