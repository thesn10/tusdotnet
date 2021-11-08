using System;

#if endpointrouting
using tusdotnet.ExternalMiddleware.EndpointRouting;
#endif

namespace tusdotnet.Models
{
	/// <summary>
	/// Exception thrown by a store if the store wishes to send a message to the client.
	/// All TusStoreExceptions will result in a 400 Bad Request response with the exception message
	/// as the response body.
	/// </summary>
#if endpointrouting
	public class TusStoreException : TusException
#else
	public class TusStoreException : Exception
#endif
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TusStoreException"/> class.
		/// </summary>
		/// <param name="message">The message. This message will be returned to the client.</param>
		public TusStoreException(string message) : base(message)
		{
			// Left blank.
		}
	}
}
