using System.Threading.Tasks;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    /// <summary>
    /// Result of an controller action method
    /// </summary>
    public interface ITusActionResult
    {
        /// <summary>
        /// Extecutes the result
        /// </summary>
        public Task Execute(TusContext context);

        /// <summary>
        /// Non-success results will interrupt normal execution and return the http result
        /// </summary>
        bool IsSuccessResult { get; }
    }
}
