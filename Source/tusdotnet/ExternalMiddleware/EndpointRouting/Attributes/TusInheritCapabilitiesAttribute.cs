using System;
using System.Collections.Generic;
using System.Linq;
#if endpointrouting

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    /// <summary>
    /// Indicates that the tus controller should inherit the tus extension capabilities 
    /// of the specified tus store
    /// In other words: The tus controller will enable all tus extensions 
    /// that the tus store is capable of handling
    /// </summary>
    public class TusInheritCapabilitiesAttribute : Attribute
    {
        internal Type StoreType { get; set; }

        public TusInheritCapabilitiesAttribute(Type storeType)
        {
            StoreType = storeType;
        }
    }
}
#endif