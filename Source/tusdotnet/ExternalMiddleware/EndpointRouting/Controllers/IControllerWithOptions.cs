#if endpointrouting

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    /// <summary>
    /// Used by endpoints.MapTusEndpoint() to be able to inject the config object into the EventsBasedTusCntroller
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    internal interface IControllerWithOptions<TOptions>
    {
        /// <summary>
        /// Injected by <see cref="TusProtocolHandlerEndpointBased{TController,TControllerOptions}"/>
        /// </summary>
        TOptions Options { get; set; }
    }
}

#endif
