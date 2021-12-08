using Microsoft.AspNetCore.Builder;
using System;

namespace tusdotnet
{
    /// <inheritdoc />
    public class ControllerEndpointConventionBuilder : IEndpointConventionBuilder
    {
        /// <inheritdoc />
        public void Add(Action<EndpointBuilder> convention)
        {
            // Do nothing for now
        }
    }
}