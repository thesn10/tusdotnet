using System.Net;
using System.Threading.Tasks;
using tusdotnet.Routing;

namespace tusdotnet.Controllers
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
            return new TusBaseResult(HttpStatusCode.Unauthorized).Execute(context);
        }
    }
}