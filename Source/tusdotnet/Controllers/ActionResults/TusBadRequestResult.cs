using System.Net;
using System.Threading.Tasks;
using tusdotnet.Routing;

namespace tusdotnet.Controllers
{
    /// <summary>
    /// An <see cref="ITusActionResult"/> that when executed will produce a <see cref="HttpStatusCode.BadRequest"/> response.
    /// </summary>
    public class TusBadRequestResult : ISimpleResult, IFileInfoResult, IWriteResult, ICreateResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TusBadRequestResult"/> class.
        /// </summary>
        public TusBadRequestResult()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TusBadRequestResult"/> class.
        /// </summary>
        public TusBadRequestResult(string message)
        {
            Message = message;
        }

        /// <summary>
        /// Message to write into the request body if needed
        /// </summary>
        public string Message { get; set; }

        /// <inheritdoc />
        public bool IsSuccessResult => false;

        /// <inheritdoc />
        public Task Execute(TusContext context)
        {
            return new TusBaseResult(HttpStatusCode.BadRequest, Message).Execute(context);
        }
    }
}
