using System.Net;
using System.Threading.Tasks;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    /// <summary>
    /// An <see cref="ITusActionResult"/> that when executed will produce a <see cref="HttpStatusCode.Unauthorized"/> response.
    /// </summary>
    public class TusUnauthorizedResult : ISimpleResult, IFileInfoResult, IWriteResult, ICreateResult
    {
        /// <inheritdoc />
        public bool IsSuccessResult => false;

        /// <inheritdoc />
        public Task Execute(TusContext context)
        {
            return new TusStatusCodeResult(HttpStatusCode.Unauthorized).Execute(context);
        }
    }
}