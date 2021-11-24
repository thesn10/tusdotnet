#if endpointrouting

using Microsoft.AspNetCore.Mvc;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    /// <summary>
    /// Has to be one of Ok(), BadRequest(), Forbidden(), or Unauthorized()
    /// </summary>
    public interface ISimpleResult : ITusActionResult
    {
        /// <summary>
        /// Translates the result into an executable IActionResult
        /// </summary>
        //IActionResult Translate();

        /// <summary>
        /// Non-success results will interrupt normal execution and return the http result
        /// </summary>
        //bool IsSuccessResult { get; }
    }
}

#endif