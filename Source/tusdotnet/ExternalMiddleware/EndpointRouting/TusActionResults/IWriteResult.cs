#if endpointrouting

using System;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    /// <summary>
    /// Has to be one of WriteStatus(), BadRequest(), or Forbidden()
    /// </summary>
    public interface IWriteResult
    {
    }
}
#endif
