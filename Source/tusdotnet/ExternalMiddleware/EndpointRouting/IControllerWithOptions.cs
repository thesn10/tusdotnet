#if endpointrouting

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    /// <summary>
    /// Used by endpoints.MapTusSimpleEndpoint() to be able to inject the config object into the SimpleTusCntroller
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    internal interface IControllerWithOptions<TOptions>
    {
        /// <summary>
        /// Injected by <see cref="TusProtocolHandlerEndpointBased{TController,TControllerOptions}"/>
        /// </summary>
        public TOptions Options { get; set; }
    }
}

#endif
