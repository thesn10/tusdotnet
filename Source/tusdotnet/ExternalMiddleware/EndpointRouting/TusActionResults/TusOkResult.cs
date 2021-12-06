using System.Net;
using System.Threading.Tasks;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    /// <summary>
    /// An <see cref="ITusActionResult"/> that indicates success.
    /// When executed will produce a <see cref="HttpStatusCode.NoContent"/> response.
    /// </summary>
    public class TusOkResult : ISimpleResult
    {
        /// <inheritdoc />
        public bool IsSuccessResult => true;

        /// <inheritdoc />
        public Task Execute(TusContext context)
        {
            return new TusStatusCodeResult(HttpStatusCode.NoContent).Execute(context);
        }
    }
}