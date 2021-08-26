#if endpointrouting

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    internal interface IControllerWithOptions<TOptions>
    {
        /// <summary>
        /// Injected by <see cref="TusProtocolHandlerEndpointBased{TController,TControllerOptions}"/>
        /// </summary>
        public TOptions Options { get; set; }
    }
}

#endif
