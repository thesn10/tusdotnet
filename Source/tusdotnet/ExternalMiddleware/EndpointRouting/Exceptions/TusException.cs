using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
	/// <summary>
	/// Exception thrown by a controller or storage client if it wishes to send a (error-)message to the client.
	/// All TusExceptions will result in a http response with the exception message
	/// as the response body.
	/// </summary>
	public abstract class TusException : Exception
	{
		/// <summary>
		/// The http status code of the resulting http response
		/// </summary>
		public HttpStatusCode StatusCode { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="TusException"/> class.
		/// </summary>
		/// <param name="statusCode">The http status code which will be used for the http response</param>
		public TusException(HttpStatusCode statusCode = HttpStatusCode.BadRequest) : base()
		{
			StatusCode = statusCode;
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="TusException"/> class.
        /// </summary>
        /// <param name="message">The message. This message will be returned to the client.</param>
        /// <param name="statusCode">The http status code which will be used for the http response</param>
        /// <param name="innerException">The inner exception</param>
        public TusException(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest, Exception? innerException = null) : base(message, innerException)
		{
			StatusCode = statusCode;
		}
    }
}
