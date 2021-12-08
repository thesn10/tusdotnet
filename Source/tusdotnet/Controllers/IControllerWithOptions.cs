namespace tusdotnet.Controllers
{
    /// <summary>
    /// Used by endpoints.MapTus() to be able to inject the config object into the <see cref="EventsBasedTusController"/>
    /// </summary>
    internal interface IControllerWithOptions<TOptions>
    {
        /// <summary>
        /// Injected by <see cref="TusProtocolHandlerIntentBased"/>
        /// </summary>
        TOptions Options { get; set; }
    }
}
