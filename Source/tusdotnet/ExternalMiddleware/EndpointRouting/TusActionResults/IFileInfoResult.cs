using tusdotnet.Models.Concatenation;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    /// <summary>
    /// Has to be one of FileInfo(), BadRequest(), or Forbidden()
    /// </summary>
    public interface IFileInfoResult : ITusActionResult
    {

    }
}
