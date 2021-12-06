using System.Net;
using System.Threading.Tasks;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    /// <summary>
    /// An <see cref="ITusActionResult"/> that when executed will produce a <see cref="HttpStatusCode.Forbidden"/> response.
    /// </summary>
    public class TusForbiddenResult : ISimpleResult, IFileInfoResult, IWriteResult, ICreateResult
    {
        /// <inheritdoc />
        public bool IsSuccessResult => false;

        /// <inheritdoc />
        public Task Execute(TusContext context)
        {
            return new TusStatusCodeResult(HttpStatusCode.Forbidden).Execute(context);
        }
    }
}